using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Axicultus.Wpf.Captions;

public sealed record CaptionRecovery(int Version, long Revision, string Plan, string Draft, string? DeckPath,
    int Slide, string Box, Dictionary<int, string> Overrides, CaptionTake[] History, JsonElement[] Items, int SelectedIndex = -1);

/// <summary>
/// One writer per editor. Each submitted snapshot owns its collections. The worship order's items are kept as the host
/// wrote them (<see cref="ICaptionQueueRecovery"/>); isItem is the host's check of one (a file with a damaged item is
/// not used: its backup is). Without it any JSON object is an item.
/// </summary>
public sealed class CaptionRecoveryStore(string directory, Func<JsonElement, bool>? isItem = null) : IDisposable
{
    private readonly string _directory = directory;
    private readonly Func<JsonElement, bool> _isItem = isItem ?? (item => item.ValueKind == JsonValueKind.Object);
    private readonly Lock _sync = new();
    private Task _tail = Task.CompletedTask;

    private string PathFor(string plan) => Path.Combine(_directory,
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plan))) + ".json");

    public CaptionRecovery? Read(string plan)
    {
        var path = PathFor(plan);
        var current = TryRead(path, plan, out var error);
        if (current != null) return current;
        var backup = TryRead(path + ".bak", plan, out var backupError);
        if (backup == null && (error ?? backupError) is { } cause)
            throw new InvalidDataException(CaptionText.Get("CaptionConsole", "RecoveryUnreadable"), cause);
        return backup;
    }

    private CaptionRecovery? TryRead(string path, string plan, out Exception? error)
    {
        error = null;
        if (!File.Exists(path)) return null;
        try
        {
            using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var entry = JsonSerializer.Deserialize<CaptionRecovery>(input);
            if (entry is not { Version: 1, Draft: not null, Overrides: not null, History: not null, Items: not null } || entry.Plan != plan)
                throw new InvalidDataException(CaptionText.Get("CaptionConsole", "RecoveryUnsupported"));
            if (entry.Items.Any(item => !_isItem(item)) ||
                entry.History.Any(take => take?.Text is null) || entry.Overrides.ContainsValue(null!))
                throw new InvalidDataException(CaptionText.Get("CaptionConsole", "RecoveryDamaged"));
            return entry;
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidDataException)
        {
            error = ex;
            return null;
        }
    }

    public Task SaveAsync(CaptionRecovery snapshot)
    {
        lock (_sync) return _tail = _tail.ContinueWith(_ => Write(snapshot),
            default, TaskContinuationOptions.None, TaskScheduler.Default);
    }

    internal Task SaveQueueAsync(string plan, JsonElement[] items, int selected)
    {
        lock (_sync) return _tail = _tail.ContinueWith(_ =>
        {
            var previous = Read(plan) ?? new CaptionRecovery(1, 0, plan, "", null, 1, "", [], [], []);
            Write(previous with { Revision = previous.Revision + 1, Items = items, SelectedIndex = selected });
        }, default, TaskContinuationOptions.None, TaskScheduler.Default);
    }

    public void Dispose()
    {
        Task last;
        lock (_sync) last = _tail;
        try { last.GetAwaiter().GetResult(); }
        catch (Exception error) { System.Diagnostics.Trace.WriteLine(error); }
    }

    private void Write(CaptionRecovery snapshot)
    {
        Directory.CreateDirectory(_directory);
        var path = PathFor(snapshot.Plan);
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var previous = TryRead(path, snapshot.Plan, out _);
            var lastGood = previous ?? TryRead(path + ".bak", snapshot.Plan, out _);
            if (lastGood != null && snapshot.Revision <= lastGood.Revision)
                snapshot = snapshot with { Revision = lastGood.Revision + 1 };
            using (var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(output, snapshot);
                output.Flush(true);
            }
            if (previous != null) File.Replace(temp, path, path + ".bak");
            else File.Move(temp, path, overwrite: true);
        }
        finally { File.Delete(temp); }
    }
}
