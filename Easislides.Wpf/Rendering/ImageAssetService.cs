using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Easislides.Wpf.Rendering;

public enum ImageFillMode
{
    Fit,
    Fill,
    Stretch,
    Center
}

public enum ImageAssetErrorKind
{
    None,
    InvalidRequest,
    FileNotFound,
    UnsupportedFile,
    FileLocked,
    DecodeFailed,
    Cancelled,
    Unknown
}

public sealed record ImageAssetRequest(
    string FilePath,
    int ViewportWidth,
    int ViewportHeight,
    ImageFillMode FillMode = ImageFillMode.Fit);

public sealed record ImageAssetSnapshot(
    string FilePath,
    int PixelWidth,
    int PixelHeight,
    string ContentType,
    DateTimeOffset LastWriteTimeUtc);

public sealed record ImagePlacement(int Left, int Top, int Width, int Height)
{
    public static ImagePlacement Empty { get; } = new(0, 0, 0, 0);
}

public sealed record ImageAssetResult(
    ImageAssetErrorKind ErrorKind,
    ImageAssetSnapshot? Asset,
    ImagePlacement Placement,
    string? ErrorMessage)
{
    public bool Succeeded => ErrorKind == ImageAssetErrorKind.None && Asset is not null;
}

public interface IImageAssetService
{
    Task<ImageAssetResult> LoadAsync(
        ImageAssetRequest request,
        CancellationToken cancellationToken = default);

    ImagePlacement CalculatePlacement(
        int imageWidth,
        int imageHeight,
        int viewportWidth,
        int viewportHeight,
        ImageFillMode fillMode);
}

public sealed class ImageAssetService : IImageAssetService
{
    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".bmp"] = "image/bmp",
        [".gif"] = "image/gif",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".tif"] = "image/tiff",
        [".tiff"] = "image/tiff"
    };

    public Task<ImageAssetResult> LoadAsync(
        ImageAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Failure(ImageAssetErrorKind.Cancelled, "Cancelled"));
        }

        if (!TryNormalizeRequest(request, out var normalized, out var fileInfo, out var contentType, out var failure))
        {
            return Task.FromResult(failure);
        }

        if (!CanOpenForRead(normalized.FilePath, out failure))
        {
            return Task.FromResult(failure);
        }

        try
        {
            using var stream = new FileStream(normalized.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile,
                BitmapCacheOption.OnLoad);

            if (decoder.Frames.Count == 0)
            {
                return Task.FromResult(Failure(ImageAssetErrorKind.DecodeFailed, "Image has no decodable frames."));
            }

            var frame = decoder.Frames[0];
            var asset = new ImageAssetSnapshot(
                normalized.FilePath,
                frame.PixelWidth,
                frame.PixelHeight,
                contentType!,
                new DateTimeOffset(fileInfo!.LastWriteTimeUtc, TimeSpan.Zero));

            var placement = CalculatePlacement(
                asset.PixelWidth,
                asset.PixelHeight,
                normalized.ViewportWidth,
                normalized.ViewportHeight,
                normalized.FillMode);

            return Task.FromResult(new ImageAssetResult(
                ImageAssetErrorKind.None,
                asset,
                placement,
                ErrorMessage: null));
        }
        catch (IOException ex)
        {
            return Task.FromResult(Failure(ImageAssetErrorKind.FileLocked, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Task.FromResult(Failure(ImageAssetErrorKind.FileLocked, ex.Message));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Failure(ImageAssetErrorKind.DecodeFailed, ex.Message));
        }
    }

    public ImagePlacement CalculatePlacement(
        int imageWidth,
        int imageHeight,
        int viewportWidth,
        int viewportHeight,
        ImageFillMode fillMode)
    {
        if (imageWidth < 1 || imageHeight < 1 || viewportWidth < 1 || viewportHeight < 1)
        {
            return ImagePlacement.Empty;
        }

        var imageRatio = (double)imageWidth / imageHeight;
        var viewportRatio = (double)viewportWidth / viewportHeight;
        int width;
        int height;

        switch (fillMode)
        {
            case ImageFillMode.Stretch:
                width = viewportWidth;
                height = viewportHeight;
                break;
            case ImageFillMode.Center:
                width = imageWidth;
                height = imageHeight;
                break;
            case ImageFillMode.Fill:
                if (viewportRatio > imageRatio)
                {
                    width = viewportWidth;
                    height = (int)Math.Round(width / imageRatio);
                }
                else
                {
                    height = viewportHeight;
                    width = (int)Math.Round(height * imageRatio);
                }
                break;
            default:
                if (viewportRatio < imageRatio)
                {
                    width = viewportWidth;
                    height = (int)Math.Round(width / imageRatio);
                }
                else
                {
                    height = viewportHeight;
                    width = (int)Math.Round(height * imageRatio);
                }
                break;
        }

        return new ImagePlacement(
            (viewportWidth - width) / 2,
            (viewportHeight - height) / 2,
            width,
            height);
    }

    private static bool TryNormalizeRequest(
        ImageAssetRequest request,
        out ImageAssetRequest normalized,
        out FileInfo? fileInfo,
        out string? contentType,
        out ImageAssetResult failure)
    {
        normalized = request;
        fileInfo = null;
        contentType = null;
        failure = Failure(ImageAssetErrorKind.InvalidRequest, "Invalid image asset request.");

        if (string.IsNullOrWhiteSpace(request.FilePath)
            || request.ViewportWidth < 1
            || request.ViewportHeight < 1)
        {
            return false;
        }

        var extension = Path.GetExtension(request.FilePath);
        if (!ContentTypes.TryGetValue(extension, out contentType))
        {
            failure = Failure(ImageAssetErrorKind.UnsupportedFile, $"Unsupported image file: {extension}");
            return false;
        }

        var fullPath = Path.GetFullPath(request.FilePath);
        fileInfo = new FileInfo(fullPath);
        if (!fileInfo.Exists)
        {
            failure = Failure(ImageAssetErrorKind.FileNotFound, $"File not found: {fullPath}");
            return false;
        }

        normalized = request with { FilePath = fullPath };
        return true;
    }

    private static bool CanOpenForRead(string path, out ImageAssetResult failure)
    {
        failure = Failure(ImageAssetErrorKind.FileLocked, $"File is locked: {path}");

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return true;
        }
        catch (IOException ex)
        {
            failure = Failure(ImageAssetErrorKind.FileLocked, ex.Message);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            failure = Failure(ImageAssetErrorKind.FileLocked, ex.Message);
            return false;
        }
    }

    private static ImageAssetResult Failure(ImageAssetErrorKind kind, string message)
        => new(kind, Asset: null, ImagePlacement.Empty, message);
}
