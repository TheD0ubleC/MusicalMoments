using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: updater <pid> <zipPath> <installDir>");
            return 1;
        }

        if (!int.TryParse(args[0], out int pid))
        {
            Console.WriteLine("Invalid pid");
            return 2;
        }

        string zipPath = args[1];
        string installDir = args[2];

        KillProcess(pid);
        ExtractZip(zipPath, installDir);
        StartApp(Path.Combine(installDir, "MusicalMoments.exe"));

        return 0;
    }

    private static void KillProcess(int pid)
    {
        try
        {
            Process process = Process.GetProcessById(pid);
            process.Kill();
            process.WaitForExit();
        }
        catch
        {
            // Ignore: target process may already be closed.
        }
    }

    private static void ExtractZip(string zipPath, string installDir)
    {
        Directory.CreateDirectory(installDir);

        using ZipArchive zip = ZipFile.OpenRead(zipPath);
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            string destinationPath = Path.GetFullPath(Path.Combine(installDir, entry.FullName));
            string installRoot = Path.GetFullPath(installDir) + Path.DirectorySeparatorChar;

            if (!destinationPath.StartsWith(installRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsafe zip entry path: {entry.FullName}");
            }

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

    private static void StartApp(string executablePath)
    {
        if (File.Exists(executablePath))
        {
            Process.Start(executablePath);
        }
    }
}
