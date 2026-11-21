using System;
using System.Diagnostics;
using System.IO;

namespace Axicultus.Global;

internal sealed class SessionTempDirectory : IDisposable
{
    private readonly DirectoryInfo directory = Directory.CreateTempSubdirectory("Axicultuss-");

    internal string PathWithSeparator => directory.FullName + Path.DirectorySeparatorChar;

    public void Dispose()
    {
        gfFileHelpers.DeleteFolderFilesSafe(directory.FullName);
        try
        {
            directory.Delete();
        }
        catch (DirectoryNotFoundException) { }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Trace.WriteLine($"Could not remove session directory {directory.FullName}: {ex.Message}");
        }
    }
}
