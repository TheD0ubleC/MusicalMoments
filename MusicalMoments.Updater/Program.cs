using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

internal static class Program
{
    private const int ProcessWaitTimeoutMs = 15_000;
    private const int ProcessKillWaitTimeoutMs = 8_000;
    private const int FileOperationRetries = 12;
    private const int FileOperationRetryDelayMs = 250;

    private static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: updater <pid> <zipPath> <installDir>");
            return 1;
        }

        if (!int.TryParse(args[0], out int pid) || pid <= 0)
        {
            Console.WriteLine("Invalid pid");
            return 2;
        }

        string zipPath = args[1];
        string installDir = args[2];

        try
        {
            if (string.IsNullOrWhiteSpace(zipPath) || !File.Exists(zipPath))
            {
                throw new FileNotFoundException($"Update package not found: {zipPath}");
            }

            if (string.IsNullOrWhiteSpace(installDir))
            {
                throw new ArgumentException("Install directory is empty.", nameof(installDir));
            }

            string normalizedInstallDir = Path.GetFullPath(installDir);
            Directory.CreateDirectory(normalizedInstallDir);

            WaitForProcessExit(pid);

            string updaterProcessPath = GetCurrentProcessPath();
            ApplyUpdatePackage(zipPath, normalizedInstallDir, updaterProcessPath);
            TryDeleteFile(zipPath);

            StartApp(Path.Combine(normalizedInstallDir, "MusicalMoments.exe"));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Update failed: {ex.Message}");
            Console.Error.WriteLine(ex);
            return 3;
        }

        return 0;
    }

    private static void WaitForProcessExit(int pid)
    {
        try
        {
            Process process = Process.GetProcessById(pid);
            if (!process.WaitForExit(ProcessWaitTimeoutMs))
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(ProcessKillWaitTimeoutMs);
            }
        }
        catch
        {
            // Ignore: target process may already be closed or inaccessible.
        }
    }

    private static void ApplyUpdatePackage(string zipPath, string installDir, string updaterProcessPath)
    {
        string stagingDir = CreateStagingDirectory();

        try
        {
            ExtractZip(zipPath, stagingDir);
            string payloadRoot = ResolvePayloadRoot(stagingDir);
            CopyDirectoryContents(payloadRoot, installDir, updaterProcessPath);
        }
        finally
        {
            TryDeleteDirectory(stagingDir);
        }
    }

    private static string CreateStagingDirectory()
    {
        string stagingRoot = Path.Combine(Path.GetTempPath(), "MusicalMoments", "Updater");
        Directory.CreateDirectory(stagingRoot);

        string stagingDir = Path.Combine(stagingRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stagingDir);
        return stagingDir;
    }

    private static void ExtractZip(string zipPath, string stagingDir)
    {
        using ZipArchive zip = ZipFile.OpenRead(zipPath);
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.FullName))
            {
                continue;
            }

            string destinationPath = ResolveSafeDestinationPath(stagingDir, entry.FullName);
            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destinationPath);
                continue;
            }

            string? destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            entry.ExtractToFile(destinationPath, overwrite: true);
        }
    }

    private static string ResolveSafeDestinationPath(string rootDir, string entryFullName)
    {
        string normalizedEntryPath = entryFullName.Replace('\\', '/');
        while (normalizedEntryPath.StartsWith("./", StringComparison.Ordinal))
        {
            normalizedEntryPath = normalizedEntryPath.Substring(2);
        }

        if (string.IsNullOrWhiteSpace(normalizedEntryPath) || normalizedEntryPath.StartsWith("/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unsafe zip entry path: {entryFullName}");
        }

        string destinationPath = Path.GetFullPath(Path.Combine(rootDir, normalizedEntryPath));
        string relativePath = Path.GetRelativePath(rootDir, destinationPath);

        if (relativePath.Equals("..", StringComparison.Ordinal)
            || relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            || Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException($"Unsafe zip entry path: {entryFullName}");
        }

        return destinationPath;
    }

    private static string ResolvePayloadRoot(string stagingDir)
    {
        string[] rootFiles = Directory.GetFiles(stagingDir);
        string[] rootDirectories = Directory.GetDirectories(stagingDir);

        if (rootFiles.Length == 0 && rootDirectories.Length == 1)
        {
            return rootDirectories[0];
        }

        return stagingDir;
    }

    private static void CopyDirectoryContents(string sourceDir, string targetDir, string updaterProcessPath)
    {
        foreach (string directory in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceDir, directory);
            string destinationDirectory = Path.Combine(targetDir, relativePath);
            Directory.CreateDirectory(destinationDirectory);
        }

        foreach (string sourceFile in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
            string destinationPath = Path.GetFullPath(Path.Combine(targetDir, relativePath));

            if (ShouldSkipFile(destinationPath, updaterProcessPath))
            {
                continue;
            }

            string? destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            CopyFileWithRetry(sourceFile, destinationPath);
        }
    }

    private static bool ShouldSkipFile(string destinationPath, string updaterProcessPath)
    {
        string fileName = Path.GetFileName(destinationPath);
        if (fileName.StartsWith("MusicalMoments.Updater", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(updaterProcessPath))
        {
            return false;
        }

        return PathsEqual(destinationPath, updaterProcessPath);
    }

    private static void CopyFileWithRetry(string sourceFile, string destinationPath)
    {
        for (int attempt = 1; attempt <= FileOperationRetries; attempt++)
        {
            try
            {
                File.Copy(sourceFile, destinationPath, overwrite: true);
                return;
            }
            catch (IOException) when (attempt < FileOperationRetries)
            {
                Thread.Sleep(FileOperationRetryDelayMs);
            }
            catch (UnauthorizedAccessException) when (attempt < FileOperationRetries)
            {
                Thread.Sleep(FileOperationRetryDelayMs);
            }
        }
    }

    private static string GetCurrentProcessPath()
    {
        try
        {
            return Environment.ProcessPath
                ?? Process.GetCurrentProcess().MainModule?.FileName
                ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool PathsEqual(string left, string right)
    {
        string normalizedLeft = Path.GetFullPath(left)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string normalizedRight = Path.GetFullPath(right)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Best effort cleanup.
        }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best effort cleanup.
        }
    }

    private static void StartApp(string executablePath)
    {
        if (File.Exists(executablePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty,
                UseShellExecute = true
            });
        }
    }
}
