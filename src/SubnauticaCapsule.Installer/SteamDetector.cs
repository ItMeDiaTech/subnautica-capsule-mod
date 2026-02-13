using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SubnauticaCapsule.Installer;

internal static class SteamDetector
{
    private const int SubnauticaAppId = 264710;

    public static string? DetectSubnauticaPath()
    {
        string? steamPath = GetSteamInstallPath();
        if (steamPath == null) return null;

        var libraryFolders = GetLibraryFolders(steamPath);
        foreach (var folder in libraryFolders)
        {
            string manifestPath = Path.Combine(folder, "steamapps", $"appmanifest_{SubnauticaAppId}.acf");
            if (!File.Exists(manifestPath)) continue;

            string? installDir = ParseInstallDir(manifestPath);
            if (installDir == null) continue;

            string gamePath = Path.Combine(folder, "steamapps", "common", installDir);
            if (File.Exists(Path.Combine(gamePath, "Subnautica.exe")))
                return gamePath;
        }

        return null;
    }

    private static string? GetSteamInstallPath()
    {
        string[] registryPaths =
        {
            @"SOFTWARE\WOW6432Node\Valve\Steam",
            @"SOFTWARE\Valve\Steam",
        };

        foreach (var regPath in registryPaths)
        {
            using var key = Registry.LocalMachine.OpenSubKey(regPath);
            if (key?.GetValue("InstallPath") is string path && Directory.Exists(path))
                return path;
        }

        return null;
    }

    private static List<string> GetLibraryFolders(string steamPath)
    {
        var folders = new List<string> { steamPath };

        string vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdfPath)) return folders;

        try
        {
            string content = File.ReadAllText(vdfPath);
            // Match "path" entries in the VDF file
            var matches = Regex.Matches(content, @"""path""\s+""([^""]+)""");
            foreach (Match match in matches)
            {
                string dir = match.Groups[1].Value.Replace("\\\\", "\\");
                if (Directory.Exists(dir) && !folders.Contains(dir))
                    folders.Add(dir);
            }
        }
        catch (Exception)
        {
            // Silently continue with just the main Steam path
        }

        return folders;
    }

    private static string? ParseInstallDir(string manifestPath)
    {
        try
        {
            string content = File.ReadAllText(manifestPath);
            var match = Regex.Match(content, @"""installdir""\s+""([^""]+)""");
            return match.Success ? match.Groups[1].Value : null;
        }
        catch
        {
            return null;
        }
    }
}
