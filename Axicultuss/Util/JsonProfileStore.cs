using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.Win32;

namespace Axicultus.Util;

internal sealed class JsonProfileStore : IRegistrySettingsStore
{
    internal static readonly JsonNodeOptions Names = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions Format = new() { WriteIndented = true };
    private readonly string path;
    private readonly string mutexName;
    private readonly Func<JsonObject> import;
    private readonly object gate = new();
    private JsonObject cached;
    private (DateTime Modified, long Length) stamp;
    private JsonObject editing;
    private bool dirty;

    internal JsonProfileStore(string path, Func<JsonObject> import)
    {
        this.path = Path.GetFullPath(path);
        this.import = import;
        mutexName = @"Local\Axicultuss.Profile." + Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(this.path.ToUpperInvariant())));
    }

    private (DateTime, long) GetStamp()
    {
        var file = new FileInfo(path);
        return file.Exists ? (file.LastWriteTimeUtc, file.Length) : default;
    }

    internal void Initialize() => Snapshot();

    private JsonObject Snapshot()
    {
        lock (gate)
        {
            if (editing != null) return editing;
            if (cached == null || stamp != GetStamp()) Update(() => { });
            return cached;
        }
    }

    // Options are saved as a group. Serialize competing writers and publish only a complete document.
    internal void Update(Action change)
    {
        lock (gate)
        {
            if (editing != null) { change(); return; }
            using var mutex = new Mutex(false, mutexName);
            try { if (!mutex.WaitOne(TimeSpan.FromSeconds(10))) throw new IOException("Settings file is busy."); }
            catch (AbandonedMutexException) { /* The previous owner exited. Re-read the committed file. */ }
            try
            {
                try
                {
                    using var stream = File.OpenRead(path);
                    editing = JsonNode.Parse(stream, Names) as JsonObject ?? throw new JsonException("Expected a settings object.");
                    dirty = false;
                }
                catch (IOException ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
                {
                    editing = import();
                    dirty = true;
                }
                change();
                if (dirty) Save(editing);
                cached = editing;
                stamp = GetStamp();
            }
            finally { editing = null; mutex.ReleaseMutex(); }
        }
    }

    private void Save(JsonObject document)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(file, document, Format);
                file.Flush(flushToDisk: true);
            }
            File.Move(temp, path, overwrite: true);
        }
        finally { File.Delete(temp); }
    }

    internal void Reset() => Update(() => { editing.Clear(); dirty = true; });

    internal static JsonObject Encode(object value, RegistryValueKind kind)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new JsonObject(Names)
        {
            ["kind"] = kind.ToString(),
            ["value"] = JsonSerializer.SerializeToNode(value)
        };
    }

    public object ReadValue(string section, string name)
    {
        if (Snapshot()[section]?[name] is not JsonObject entry) return null;
        var value = entry["value"] ?? throw new JsonException("Missing setting value.");
        return entry["kind"]?.GetValue<string>() switch
        {
            "String" => value.GetValue<string>(),
            "ExpandString" => Environment.ExpandEnvironmentVariables(value.GetValue<string>()),
            "DWord" => value.GetValue<int>(),
            "QWord" => value.GetValue<long>(),
            "MultiString" => value.Deserialize<string[]>(),
            "Binary" or "None" => value.Deserialize<byte[]>(),
            _ => throw new JsonException("Unknown setting type.")
        };
    }

    public void WriteValue(string section, string name, object value, RegistryValueKind kind) => Update(() =>
    {
        if (editing[section] == null) editing[section] = new JsonObject(Names);
        var values = editing[section].AsObject();
        var next = Encode(value, kind);
        if (JsonNode.DeepEquals(values[name], next)) return;
        values[name] = next;
        dirty = true;
    });

    public void DeleteValue(string section, string name) => Update(() =>
    {
        if (editing[section] is JsonObject values && values.Remove(name)) dirty = true;
    });
}
