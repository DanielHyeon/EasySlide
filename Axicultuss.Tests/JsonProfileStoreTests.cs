using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Axicultus.Util;
using Microsoft.Win32;
using Xunit;

namespace Axicultus.Tests;

public sealed class JsonProfileStoreTests : IDisposable
{
    private readonly DirectoryInfo directory = Directory.CreateTempSubdirectory();
    private string FilePath => Path.Combine(directory.FullName, "Axicultuss", "settings.json");
    private JsonProfileStore Open() => new(FilePath, () => new JsonObject(JsonProfileStore.Names));
    public void Dispose() => directory.Delete(recursive: true);

    [Fact]
    public void AppProfilesAndFreshWorkingFoldersUseDifferentProductNames()
    {
        Assert.Equal(Path.Combine(directory.FullName, "Axicultuss"), ProductStorage.WinFormsProfileDirectory(directory.FullName));
        Assert.Equal(Path.Combine(directory.FullName, "Axicultus"), ProductStorage.WpfProfileDirectory(directory.FullName));
        Assert.Equal(ProductStorage.WinFormsProfileDirectory(directory.FullName), ProductStorage.WinFormsWorkingDirectory(directory.FullName));
        var previous = Directory.CreateDirectory(ProductStorage.WorkingDirectory(directory.FullName));
        Assert.Equal(previous.FullName, ProductStorage.WinFormsWorkingDirectory(directory.FullName));
    }

    [Fact]
    public void RoundTripsValuesAndResetWithoutReadingRegistryAgain()
    {
        var first = Open();
        first.Update(() =>
        {
            first.WriteValue("settings", "label", "큰 글씨", RegistryValueKind.String);
            first.WriteValue("MONITORS", "x", -1920, RegistryValueKind.DWord);
            first.WriteValue("history", "time", long.MaxValue, RegistryValueKind.QWord);
            first.WriteValue(@"custom\sub", "", new[] { "", "가", "A" }, RegistryValueKind.MultiString);
        });
        var second = new JsonProfileStore(FilePath, () => throw new InvalidOperationException("Unexpected registry import"));
        Assert.Equal("큰 글씨", second.ReadValue("SETTINGS", "LABEL"));
        Assert.Equal(-1920, second.ReadValue("monitors", "X"));
        Assert.Equal(long.MaxValue, second.ReadValue("history", "time"));
        Assert.Equal(new[] { "", "가", "A" }, Assert.IsType<string[]>(second.ReadValue(@"custom\sub", "")));
        second.DeleteValue("monitors", "x");
        Assert.Null(first.ReadValue("monitors", "x"));
        second.Reset();
        Assert.Empty(JsonNode.Parse(File.ReadAllText(FilePath)).AsObject());
        Assert.Null(first.ReadValue("settings", "label"));
    }

    [Fact]
    public void ConcurrentWritersPreserveIndependentEditsAndFailedBatchKeepsLastCommit()
    {
        Parallel.For(0, 16, i => Open().WriteValue("items", i.ToString(), i, RegistryValueKind.DWord));
        var store = Open();
        for (int i = 0; i < 16; i++) Assert.Equal(i, store.ReadValue("ITEMS", i.ToString()));
        Assert.Throws<IOException>(() => store.Update(() =>
        {
            store.DeleteValue("items", "0");
            throw new IOException("Cancelled edit");
        }));
        Assert.Equal(0, Open().ReadValue("items", "0"));
    }

    [Fact]
    public void InvalidJsonIsPreservedAndReported()
    {
        var store = Open();
        store.Initialize();
        File.WriteAllText(FilePath, "{broken");
        Assert.ThrowsAny<JsonException>(() => store.WriteValue("settings", "font", 12, RegistryValueKind.DWord));
        Assert.Equal("{broken", File.ReadAllText(FilePath));
    }
}
