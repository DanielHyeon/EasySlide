using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Easislides.Wpf.Rendering;

public enum ThumbnailSourceKind
{
    PowerPointSlide,
    ImageAsset
}

public sealed record ThumbnailCacheOptions(
    int MaxEntries = 512,
    long MaxBytes = 256L * 1024 * 1024);

public sealed record ThumbnailCacheRequest(
    string SourcePath,
    ThumbnailSourceKind SourceKind,
    int ItemNumber,
    int PixelWidth,
    int PixelHeight,
    string? Variant = null);

public sealed record ThumbnailCacheKey(
    string SourcePath,
    ThumbnailSourceKind SourceKind,
    int ItemNumber,
    int PixelWidth,
    int PixelHeight,
    string Variant,
    long LastWriteUtcTicks,
    long Length)
{
    public static ThumbnailCacheKey FromFile(ThumbnailCacheRequest request, FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        return new ThumbnailCacheKey(
            NormalizePath(request.SourcePath),
            request.SourceKind,
            Math.Max(0, request.ItemNumber),
            Math.Max(0, request.PixelWidth),
            Math.Max(0, request.PixelHeight),
            NormalizeVariant(request.Variant),
            fileInfo.LastWriteTimeUtc.Ticks,
            Math.Max(0, fileInfo.Length));
    }

    internal static string NormalizePath(string sourcePath)
        => Path.GetFullPath(sourcePath).ToUpperInvariant();

    private static string NormalizeVariant(string? variant)
        => string.IsNullOrWhiteSpace(variant) ? string.Empty : variant.Trim();
}

public sealed record ThumbnailCacheStats(int Count, long TotalBytes, int EvictionCount);

public interface IThumbnailCache
{
    bool TryGet<TSnapshot>(ThumbnailCacheKey key, out TSnapshot? snapshot)
        where TSnapshot : class;

    void Store<TSnapshot>(ThumbnailCacheKey key, TSnapshot snapshot, long byteSize = 0)
        where TSnapshot : class;

    int InvalidateSource(string sourcePath);

    void Clear();

    ThumbnailCacheStats Snapshot();
}

public sealed class ThumbnailCache : IThumbnailCache
{
    private readonly Dictionary<ThumbnailCacheKey, CacheEntry> _entries = new();
    private readonly LinkedList<ThumbnailCacheKey> _lru = new();
    private ThumbnailCacheOptions _options;
    private readonly object _lock = new();
    private long _totalBytes;
    private int _evictionCount;

    public ThumbnailCache()
        : this(new ThumbnailCacheOptions())
    {
    }

    public ThumbnailCache(ThumbnailCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = NormalizeOptions(options);
    }

    public void UpdateOptions(ThumbnailCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        lock (_lock)
        {
            _options = NormalizeOptions(options);
            Trim();
        }
    }

    public bool TryGet<TSnapshot>(ThumbnailCacheKey key, out TSnapshot? snapshot)
        where TSnapshot : class
    {
        lock (_lock)
        {
            if (_entries.TryGetValue(key, out var entry) && entry.Snapshot is TSnapshot typed)
            {
                Touch(entry);
                snapshot = typed;
                return true;
            }
        }

        snapshot = null;
        return false;
    }

    public void Store<TSnapshot>(ThumbnailCacheKey key, TSnapshot snapshot, long byteSize = 0)
        where TSnapshot : class
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        lock (_lock)
        {
            Remove(key);

            var node = _lru.AddLast(key);
            var normalizedByteSize = Math.Max(0, byteSize);
            _entries[key] = new CacheEntry(snapshot, normalizedByteSize, node);
            _totalBytes += normalizedByteSize;
            Trim();
        }
    }

    public int InvalidateSource(string sourcePath)
    {
        var normalized = ThumbnailCacheKey.NormalizePath(sourcePath);
        var removed = 0;

        lock (_lock)
        {
            foreach (var key in _entries.Keys.Where(key => string.Equals(key.SourcePath, normalized, StringComparison.OrdinalIgnoreCase)).ToArray())
            {
                if (Remove(key))
                {
                    removed++;
                }
            }
        }

        return removed;
    }

    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
            _lru.Clear();
            _totalBytes = 0;
        }
    }

    public ThumbnailCacheStats Snapshot()
    {
        lock (_lock)
        {
            return new ThumbnailCacheStats(_entries.Count, _totalBytes, _evictionCount);
        }
    }

    private void Touch(CacheEntry entry)
    {
        _lru.Remove(entry.Node);
        _lru.AddLast(entry.Node);
    }

    private void Trim()
    {
        while (_entries.Count > _options.MaxEntries)
        {
            EvictLeastRecent();
        }

        while (_entries.Count > 1 && _totalBytes > _options.MaxBytes)
        {
            EvictLeastRecent();
        }
    }

    private void EvictLeastRecent()
    {
        if (_lru.First is null)
        {
            return;
        }

        if (Remove(_lru.First.Value))
        {
            _evictionCount++;
        }
    }

    private bool Remove(ThumbnailCacheKey key)
    {
        if (!_entries.Remove(key, out var entry))
        {
            return false;
        }

        _lru.Remove(entry.Node);
        _totalBytes -= entry.ByteSize;
        return true;
    }

    private static ThumbnailCacheOptions NormalizeOptions(ThumbnailCacheOptions options)
        => options with
        {
            MaxEntries = Math.Max(1, options.MaxEntries),
            MaxBytes = Math.Max(1, options.MaxBytes)
        };

    private sealed record CacheEntry(
        object Snapshot,
        long ByteSize,
        LinkedListNode<ThumbnailCacheKey> Node);
}
