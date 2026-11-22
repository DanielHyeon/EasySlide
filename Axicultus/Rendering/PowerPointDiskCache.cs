using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Axicultus.Wpf.Rendering;

/// <summary>File-backed PNG exports keyed by deck revision, slide and render bounds.</summary>
internal sealed class PowerPointDiskCache(string directory)
{
    private readonly Lock _sync = new();
    private const int HeaderSize = 12 + SHA256.HashSizeInBytes;
    private static ReadOnlySpan<byte> PngSignature => [137,80,78,71,13,10,26,10];
    private string PathFor(ThumbnailCacheKey key) => Path.Combine(directory,
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(key))) + ".slide");

    internal Task<PowerPointSlideSnapshot?> ReadAsync(ThumbnailCacheKey key, long limit) => Task.Run(() =>
    {
        lock (_sync)
        {
            if (limit <= HeaderSize) return null;
            try
            {
                var path = PathFor(key);
                if (!File.Exists(path)) return null; // a miss is the common case; do not pay for an exception each time
                using var input = new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read | FileShare.Delete);
                if (input.Length <= HeaderSize || input.Length > Math.Min(limit, int.MaxValue)) return null;
                Span<byte> header = stackalloc byte[HeaderSize];
                input.ReadExactly(header);
                int count = BinaryPrimitives.ReadInt32LittleEndian(header);
                int width = BinaryPrimitives.ReadInt32LittleEndian(header[4..]);
                int height = BinaryPrimitives.ReadInt32LittleEndian(header[8..]);
                if (count < key.ItemNumber || width < 1 || width > key.PixelWidth || height < 1 || height > key.PixelHeight) return null;
                byte[] png = new byte[checked((int)input.Length - HeaderSize)];
                input.ReadExactly(png);
                if (!png.AsSpan().StartsWith(PngSignature) || !header[12..].SequenceEqual(SHA256.HashData(png))) return null;
                input.Close();
                File.SetLastWriteTimeUtc(path,DateTime.UtcNow);
                return new PowerPointSlideSnapshot(key.SourcePath, key.ItemNumber, count, width, height,
                    png, "image/png", DateTimeOffset.UtcNow) { RenderBounds = (key.PixelWidth,key.PixelHeight) };
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
        }
    });

    internal Task StoreAsync(ThumbnailCacheKey key, PowerPointSlideSnapshot slide, long limit) => Task.Run(() =>
    {
        if (slide.ImageBytes.LongLength + HeaderSize > limit || !slide.ImageBytes.AsSpan().StartsWith(PngSignature)) return;
        lock (_sync)
        {
            string? temp = null;
            try
            {
                Directory.CreateDirectory(directory);
                var path = PathFor(key);
                temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                using (var output = new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None))
                {
                    Span<byte> header = stackalloc byte[HeaderSize];
                    BinaryPrimitives.WriteInt32LittleEndian(header,slide.SlideCount);
                    BinaryPrimitives.WriteInt32LittleEndian(header[4..],slide.PixelWidth);
                    BinaryPrimitives.WriteInt32LittleEndian(header[8..],slide.PixelHeight);
                    SHA256.HashData(slide.ImageBytes, header[12..]);
                    output.Write(header);
                    output.Write(slide.ImageBytes);
                }
                File.Move(temp,path,true);
                Trim(limit);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { System.Diagnostics.Debug.WriteLine(ex.Message); }
            finally
            {
                if (temp != null) try { File.Delete(temp); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }
    });

    internal void Trim(long limit)
    {
        lock (_sync)
        {
            try
            {
                var dir = new DirectoryInfo(directory);
                if (!dir.Exists) return;
                long bytes = 0;
                int count = 0;
                foreach (var file in dir.EnumerateFiles("*.slide").OrderByDescending(file => file.LastWriteTimeUtc))
                {
                    if (++count <= 512 && file.Length <= limit - bytes) bytes += file.Length;
                    else file.Delete();
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }
    }
}
