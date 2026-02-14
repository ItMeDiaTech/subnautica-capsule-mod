using System;
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
}
