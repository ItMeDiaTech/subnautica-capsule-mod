using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SubnauticaCapsule.Installer;

internal static class UpdateChecker
{
    private const string Owner = "ItMeDiaTech";
    private const string Repo = "subnautica-capsule-mod";
    private const string AssetName = "SubnauticaCapsule.Installer.exe";
    private const long MaxDownloadBytes = 200 * 1024 * 1024; // 200 MB safety limit

    private static readonly HttpClient Http = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"SubnauticaCapsule-Installer/{InstallerVersion.Current}");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");
        return client;
    }

    public sealed record ReleaseInfo(
        string TagName,
        Version Version,
        string? AssetUrl,
        long AssetSize);

    public static async Task<ReleaseInfo?> GetLatestReleaseAsync(CancellationToken ct = default)
    {
        var url = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";

        using var response = await Http.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = doc.RootElement;

        var tagName = root.GetProperty("tag_name").GetString() ?? "";
        var versionStr = tagName.StartsWith('v') ? tagName[1..] : tagName;

        if (!Version.TryParse(versionStr, out var version))
            return null;

        string? assetUrl = null;
        long assetSize = 0;

        if (root.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString();
                if (string.Equals(name, AssetName, StringComparison.OrdinalIgnoreCase))
                {
                    assetUrl = asset.GetProperty("browser_download_url").GetString();
                    assetSize = asset.GetProperty("size").GetInt64();
                    break;
                }
            }
        }

        return new ReleaseInfo(tagName, version, assetUrl, assetSize);
    }

    public static bool IsNewerThan(Version remote, string currentVersion)
    {
        return Version.TryParse(currentVersion, out var local) && remote > local;
    }

    public static async Task ApplySelfUpdateAsync(
        string downloadUrl,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        var currentExe = Environment.ProcessPath
            ?? throw new InvalidOperationException("Cannot determine current executable path.");
        var tempExe = currentExe + ".update";

        try
        {
            // Download new exe next to current one
            progress?.Report("Downloading update...");
            using var response = await Http.GetAsync(downloadUrl,
                HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            long? contentLength = response.Content.Headers.ContentLength;
            if (contentLength > MaxDownloadBytes)
                throw new InvalidOperationException(
                    $"Download too large ({contentLength} bytes). Aborting.");

            using (var remoteStream = await response.Content.ReadAsStreamAsync(ct))
            using (var fs = File.Create(tempExe))
            {
                var buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;
                while ((bytesRead = await remoteStream.ReadAsync(buffer, ct)) > 0)
                {
                    totalRead += bytesRead;
                    if (totalRead > MaxDownloadBytes)
                        throw new InvalidOperationException("Download exceeded size limit.");
                    await fs.WriteAsync(buffer.AsMemory(0, bytesRead), ct);

                    if (contentLength > 0)
                    {
                        int pct = (int)(totalRead * 100 / contentLength.Value);
                        progress?.Report($"Downloading update... {pct}%");
                    }
                }
            }

            // Write batch script to swap exe after this process exits
            progress?.Report("Applying update...");
            var batchPath = currentExe + ".update.cmd";
            int pid = Environment.ProcessId;

            var script = $"""
                @echo off
                :wait
                tasklist /FI "PID eq {pid}" 2>NUL | find /I "{pid}" >NUL
                if not errorlevel 1 (
                    timeout /t 1 /nobreak >NUL
                    goto wait
                )
                if exist "{currentExe}.old" del /f "{currentExe}.old"
                move /y "{currentExe}" "{currentExe}.old"
                move /y "{tempExe}" "{currentExe}"
                start "" "{currentExe}"
                del /f "{currentExe}.old" 2>NUL
                del /f "%~f0"
                """;

            File.WriteAllText(batchPath, script);

            // Launch batch script hidden and let caller exit the app
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batchPath}\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            });
        }
        catch
        {
            // Clean up partial download on any failure
            try { if (File.Exists(tempExe)) File.Delete(tempExe); } catch { }
            throw;
        }
    }
}
