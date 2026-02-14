using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SubnauticaCapsule.Installer;

internal sealed class ModInstaller
{
    // Pinned to specific release — update URL and hash together when upgrading
    private const string BepInExVersion = "v5.4.23-pack.3.0.1";
    private const string BepInExReleaseUrl =
        "https://github.com/toebeann/BepInEx.Subnautica/releases/download/"
        + BepInExVersion + "/Tobey.s.BepInEx.Pack.for.Subnautica.zip";
    private const string BepInExSha256 = "a94cf4b7d8f1b890abfc369fbe6b33b77e80d28e73ab97fc92ba13b43c5556bc";
    private const long MaxDownloadBytes = 50 * 1024 * 1024; // 50 MB safety limit

    private const string ModDllName = "SubnauticaCapsule.dll";
    private const string PluginsRelPath = @"BepInEx\plugins";
    private const string ConfigRelPath = @"BepInEx\config";
    // Keep in sync with PluginInfo.GUID in SubnauticaCapsule.Mod (com.diatech.unlimitedtimecapsules)
    private const string ModConfigFileName = "com.diatech.unlimitedtimecapsules.cfg";
    private const string CoreRelPath = @"BepInEx\core";

    private static readonly HttpClient Http = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"SubnauticaCapsule-Installer/{InstallerVersion.Current}");
        return client;
    }

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
        progress?.Report($"Downloading BepInEx {BepInExVersion}...");

        using var response = await Http.GetAsync(
            BepInExReleaseUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        // Validate content length before downloading
        long? contentLength = response.Content.Headers.ContentLength;
        if (contentLength > MaxDownloadBytes)
        {
            throw new InvalidOperationException(
                $"Download rejected: response size ({contentLength} bytes) exceeds {MaxDownloadBytes} byte limit.");
        }

        var tempZip = Path.GetTempFileName();
        try
        {
            // Stream to disk with progress reporting
            using (var remoteStream = await response.Content.ReadAsStreamAsync(ct))
            using (var fs = File.Create(tempZip))
            {
                var buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;
                while ((bytesRead = await remoteStream.ReadAsync(buffer, ct)) > 0)
                {
                    totalRead += bytesRead;
                    if (totalRead > MaxDownloadBytes)
                    {
                        throw new InvalidOperationException(
                            $"Download aborted: exceeded {MaxDownloadBytes} byte limit.");
                    }
                    await fs.WriteAsync(buffer.AsMemory(0, bytesRead), ct);

                    if (contentLength > 0)
                    {
                        int pct = (int)(totalRead * 100 / contentLength.Value);
                        progress?.Report($"Downloading BepInEx {BepInExVersion}... {pct}%");
                    }
                }
            }

            // Verify integrity with constant-time comparison
            progress?.Report("Verifying download integrity...");
            byte[] actualHash = ComputeSha256Bytes(tempZip);
            byte[] expectedHash = Convert.FromHexString(BepInExSha256);
            if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
            {
                string actual = Convert.ToHexString(actualHash);
                throw new InvalidOperationException(
                    $"SHA-256 mismatch — expected {BepInExSha256}, got {actual}. " +
                    "The download may be corrupted or tampered with.");
            }

            // Validate ZIP entries against path traversal (Zip Slip)
            progress?.Report("Validating archive contents...");
            using (var archive = ZipFile.OpenRead(tempZip))
            {
                string fullDest = Path.GetFullPath(gamePath) + Path.DirectorySeparatorChar;
                foreach (var entry in archive.Entries)
                {
                    string entryDest = Path.GetFullPath(Path.Combine(gamePath, entry.FullName));
                    if (!entryDest.StartsWith(fullDest) && entryDest != Path.GetFullPath(gamePath))
                        throw new InvalidOperationException(
                            $"ZIP entry '{entry.FullName}' would extract outside target directory.");
                }
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
            throw new InvalidOperationException(
                $"Mod DLL '{ModDllName}' not found in installer resources. The installer may be corrupted.");
        }

        // Clear old config so BepInEx regenerates it with current settings
        var configPath = Path.Combine(gamePath, ConfigRelPath, ModConfigFileName);
        if (File.Exists(configPath))
        {
            try
            {
                File.Delete(configPath);
                progress?.Report("Removed old config file (will be regenerated on first launch).");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                progress?.Report($"Could not remove old config: {ex.Message}. " +
                    "You may need to delete it manually to pick up new settings.");
            }
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
        }
        else
        {
            progress?.Report("Mod is not installed.");
        }

        var configPath = Path.Combine(gamePath, ConfigRelPath, ModConfigFileName);
        if (File.Exists(configPath))
        {
            try
            {
                File.Delete(configPath);
                progress?.Report("Removed config file.");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                progress?.Report($"Could not remove config: {ex.Message}");
            }
        }

        progress?.Report("Mod uninstalled successfully.");
    }

    private static byte[] ComputeSha256Bytes(string filePath)
    {
        using var sha = SHA256.Create();
        using var fs = File.OpenRead(filePath);
        return sha.ComputeHash(fs);
    }
}
