using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SubnauticaCapsule.Installer;

internal sealed class ModInstaller
{
    private const string BepInExReleaseUrl =
        "https://github.com/toebeann/BepInEx.Subnautica/releases/latest/download/Tobey.s.BepInEx.Pack.for.Subnautica.zip";

    private const string ModDllName = "SubnauticaCapsule.dll";
    private const string PluginsRelPath = @"BepInEx\plugins";
    private const string CoreRelPath = @"BepInEx\core";

    public static bool IsValidGamePath(string gamePath)
    {
        return File.Exists(Path.Combine(gamePath, "Subnautica.exe"));
    }

    public static bool IsBepInExInstalled(string gamePath)
    {
        return File.Exists(Path.Combine(gamePath, CoreRelPath, "BepInEx.dll"))
            && File.Exists(Path.Combine(gamePath, "winhttp.dll"));
    }

    public static bool IsModInstalled(string gamePath)
    {
        return File.Exists(Path.Combine(gamePath, PluginsRelPath, ModDllName));
    }

    public static async Task DownloadAndInstallBepInExAsync(
        string gamePath,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        progress?.Report("Downloading BepInEx for Subnautica...");

        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("SubnauticaCapsule-Installer/1.0");

        using var response = await http.GetAsync(BepInExReleaseUrl, ct);
        response.EnsureSuccessStatusCode();

        var tempZip = Path.GetTempFileName();
        try
        {
            progress?.Report("Saving download...");
            using (var fs = File.Create(tempZip))
            {
                await response.Content.CopyToAsync(fs, ct);
            }

            progress?.Report("Extracting BepInEx to game directory...");
            ZipFile.ExtractToDirectory(tempZip, gamePath, overwriteFiles: true);

            progress?.Report("BepInEx installed successfully.");
        }
        finally
        {
            try { File.Delete(tempZip); } catch { }
        }
    }

    public static void InstallMod(string gamePath, IProgress<string>? progress = null)
    {
        var pluginsDir = Path.Combine(gamePath, PluginsRelPath);
        Directory.CreateDirectory(pluginsDir);

        var destPath = Path.Combine(pluginsDir, ModDllName);

        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(ModDllName);

        if (stream == null)
        {
            var available = string.Join(", ",
                Assembly.GetExecutingAssembly().GetManifestResourceNames());
            throw new InvalidOperationException(
                $"Mod DLL '{ModDllName}' not found in installer resources. " +
                $"Available: [{available}]. The installer may be corrupted.");
        }

        progress?.Report($"Installing {ModDllName} to plugins folder...");
        using var fs = File.Create(destPath);
        stream.CopyTo(fs);

        progress?.Report("Mod installed successfully.");
    }

    public static void UninstallMod(string gamePath, IProgress<string>? progress = null)
    {
        var dllPath = Path.Combine(gamePath, PluginsRelPath, ModDllName);
        if (File.Exists(dllPath))
        {
            progress?.Report($"Removing {ModDllName}...");
            File.Delete(dllPath);
            progress?.Report("Mod uninstalled successfully.");
        }
        else
        {
            progress?.Report("Mod is not installed.");
        }
    }
}
