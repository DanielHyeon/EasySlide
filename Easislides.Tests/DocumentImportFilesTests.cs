using System;
using System.IO;
using System.Linq;
using System.Threading;
using Easislides.Util;
using Xunit;

namespace Easislides.Tests;

public sealed class DocumentImportFilesTests : IDisposable
{
    private readonly DirectoryInfo temp = Directory.CreateTempSubdirectory();

    public void Dispose() => temp.Delete(recursive: true);

    [Fact]
    public void ScansEachFolderInLegacyOrderAndKeepsRunsIndependent()
    {
        string root = temp.FullName;
        Directory.CreateDirectory(Path.Combine(root, "Z"));
        Directory.CreateDirectory(Path.Combine(root, "A", "nested"));
        foreach (var file in new[] { "last.TxT", "second.DOCX", "first.doc", "skipped.txt.bak",
                    @"Z\z.txt", @"A\a.txt", @"A\nested\n.doc" })
            File.WriteAllText(Path.Combine(root, file), "");
        var files = DocumentImportFiles.Find(root, CancellationToken.None);
        Assert.Equal(new[] { "first.doc", "second.DOCX", "last.TxT", @"A\a.txt", @"A\nested\n.doc", @"Z\z.txt" },
            files.Select(path => Path.GetRelativePath(root, path)));
        File.Delete(Path.Combine(root, "A", "a.txt"));
        var secondScan = DocumentImportFiles.Find(Path.Combine(root, "A"), CancellationToken.None);
        Assert.Equal("n.doc", Path.GetFileName(Assert.Single(secondScan)));
        Assert.Equal(6, files.Count);
    }

    [Fact]
    public void CancelledScanDoesNotReturnPartialResults()
    {
        var cancelled = new CancellationToken(canceled: true);
        Assert.Throws<OperationCanceledException>(() => DocumentImportFiles.Find(temp.FullName, cancelled));
    }
}
