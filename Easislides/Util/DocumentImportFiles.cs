using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Easislides.Util;

internal static class DocumentImportFiles
{
    public static List<string> Find(string sourceFolder, CancellationToken cancellationToken)
    {
        List<string> documents = [];
        List<string> docxFiles = [];
        List<string> textFiles = [];
        List<DirectoryInfo> subfolders = [];
        Stack<DirectoryInfo> pending = new();
        HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);
        var options = new EnumerationOptions { AttributesToSkip = 0, IgnoreInaccessible = true };
        pending.Push(new DirectoryInfo(sourceFolder));
        while (pending.TryPop(out var folder))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((folder.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                if (folder.ResolveLinkTarget(returnFinalTarget: true) is not DirectoryInfo target || !target.Exists)
                    continue;
                folder = target;
            }
            if (folder.Name.Equals("System Volume Information", StringComparison.OrdinalIgnoreCase) && folder.Parent?.Parent == null)
                continue;
            if (!visited.Add(Path.TrimEndingDirectorySeparator(folder.FullName)))
                continue;

            // Keep the original per-folder import order: .doc, .docx, .txt, then sorted subdirectories.
            foreach (var entry in folder.EnumerateFileSystemInfos("*", options))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (entry is DirectoryInfo child)
                {
                    subfolders.Add(child);
                    continue;
                }
                var extension = Path.GetExtension(entry.Name.AsSpan());
                if (extension.Equals(".doc", StringComparison.OrdinalIgnoreCase))
                    documents.Add(entry.FullName);
                else if (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                    docxFiles.Add(entry.FullName);
                else if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    textFiles.Add(entry.FullName);
            }
            documents.AddRange(docxFiles);
            documents.AddRange(textFiles);
            docxFiles.Clear();
            textFiles.Clear();
            subfolders.Sort(static (first, second) => StringComparer.CurrentCulture.Compare(second.Name, first.Name));
            foreach (var child in subfolders)
                pending.Push(child);
            subfolders.Clear();
        }
        return documents;
    }
}
