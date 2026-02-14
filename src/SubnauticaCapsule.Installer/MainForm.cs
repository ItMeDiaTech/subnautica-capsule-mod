using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SubnauticaCapsule.Installer;

public partial class MainForm : Form
{
    private string? gamePath;
    private CancellationTokenSource? downloadCts;

    public MainForm()
    {
        InitializeComponent();
        TryAutoDetect();
    }

    private void TryAutoDetect()
    {
        var detected = SteamDetector.DetectSubnauticaPath();
        if (detected != null)
        {
            SetGamePath(detected);
            Log($"Auto-detected Subnautica at {detected}");
        }
        else
        {
            Log("Could not auto-detect Subnautica. Use Browse to select the game folder.");
            UpdateStatus();
        }
    }

    private void SetGamePath(string path)
    {
        gamePath = path;
        txtPath.Text = path;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        bool gameFound = gamePath != null && ModInstaller.IsValidGamePath(gamePath);
        bool bepInEx = gameFound && ModInstaller.IsBepInExInstalled(gamePath!);
        bool mod = gameFound && ModInstaller.IsModInstalled(gamePath!);

        lblStatusGame.Text = gameFound
            ? "Subnautica: Detected"
            : "Subnautica: Not Found";
        lblStatusGame.ForeColor = gameFound ? Color.Green : Color.Red;

        lblStatusBepInEx.Text = bepInEx
            ? "BepInEx: Installed"
            : "BepInEx: Not Installed";
        lblStatusBepInEx.ForeColor = bepInEx ? Color.Green : Color.Red;

        lblStatusMod.Text = mod
            ? "Mod: Installed"
            : "Mod: Not Installed";
        lblStatusMod.ForeColor = mod ? Color.Green : Color.Red;

        btnInstall.Enabled = gameFound && !mod;
        btnUninstall.Enabled = gameFound && mod;
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the Subnautica installation folder (containing Subnautica.exe)",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (ModInstaller.IsValidGamePath(dialog.SelectedPath))
            {
                SetGamePath(dialog.SelectedPath);
                Log($"Selected: {dialog.SelectedPath}");
            }
            else
            {
                Log("Selected folder does not contain Subnautica.exe.");
                MessageBox.Show(
                    "The selected folder does not contain Subnautica.exe.\n" +
                    "Please select the folder where Subnautica is installed.",
                    "Invalid Path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }

    private async void BtnInstall_Click(object? sender, EventArgs e)
    {
        if (gamePath == null) return;

        SetButtonsEnabled(false);
        var progress = new Progress<string>(Log);

        downloadCts = new CancellationTokenSource();
        btnCancel.Visible = true;

        try
        {
            if (!ModInstaller.IsBepInExInstalled(gamePath))
            {
                Log("BepInEx is required. Installing...");
                await ModInstaller.DownloadAndInstallBepInExAsync(gamePath, progress, downloadCts.Token);
            }

            ModInstaller.InstallMod(gamePath, progress);
            Log("Installation complete.");
        }
        catch (OperationCanceledException) when (downloadCts?.IsCancellationRequested == true)
        {
            Log("Installation cancelled.");
        }
        catch (OperationCanceledException)
        {
            Log("Error: Download timed out. Check your internet connection.");
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
            MessageBox.Show(
                $"Installation failed:\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            if (!IsDisposed)
            {
                btnCancel.Visible = false;
                btnBrowse.Enabled = true;
                UpdateStatus();
            }
            downloadCts?.Dispose();
            downloadCts = null;
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        downloadCts?.Cancel();
        Log("Cancelling...");
    }

    private void BtnUninstall_Click(object? sender, EventArgs e)
    {
        if (gamePath == null) return;

        var result = MessageBox.Show(
            "Remove the Unlimited Time Capsules mod?\n\n" +
            "BepInEx will be left installed for other mods.",
            "Confirm Uninstall",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return;

        SetButtonsEnabled(false);
        try
        {
            ModInstaller.UninstallMod(gamePath, new Progress<string>(Log));
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
            MessageBox.Show(
                $"Uninstall failed:\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnBrowse.Enabled = true;
            UpdateStatus();
        }
    }

    private void BtnClose_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private async void LnkCheckUpdate_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        lnkCheckUpdate.Enabled = false;
        Log("Checking for updates...");

        try
        {
            var release = await UpdateChecker.GetLatestReleaseAsync();

            if (release == null)
            {
                Log("Could not check for updates. GitHub may be unreachable.");
                MessageBox.Show(
                    "Unable to check for updates.\n\n" +
                    "Please check your internet connection or try again later.",
                    "Update Check Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!UpdateChecker.IsNewerThan(release.Version, InstallerVersion.Current))
            {
                Log($"You are running the latest version (v{InstallerVersion.Current}).");
                MessageBox.Show(
                    $"You are running the latest version (v{InstallerVersion.Current}).",
                    "No Updates Available",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (release.AssetUrl == null)
            {
                Log($"Update {release.TagName} found but installer asset is missing.");
                MessageBox.Show(
                    $"Version {release.TagName} is available, but the installer download " +
                    "is not yet published.\n\nPlease try again later or download manually from GitHub.",
                    "Update Incomplete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Prompt user
            var sizeMB = (release.AssetSize / (1024.0 * 1024.0)).ToString("F1");
            var result = MessageBox.Show(
                $"A new version is available: {release.TagName}\n" +
                $"Current version: v{InstallerVersion.Current}\n\n" +
                $"Download size: ~{sizeMB} MB\n\n" +
                "Download and install the update now?\n" +
                "The installer will restart automatically.",
                "Update Available",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                Log("Update declined.");
                return;
            }

            // Disable UI during download
            SetButtonsEnabled(false);
            downloadCts = new CancellationTokenSource();
            btnCancel.Visible = true;

            var progress = new Progress<string>(Log);
            await UpdateChecker.ApplySelfUpdateAsync(release.AssetUrl, progress, downloadCts.Token);

            Log("Update downloaded. Restarting...");
            Application.Exit();
        }
        catch (OperationCanceledException)
        {
            Log("Update cancelled.");
        }
        catch (Exception ex)
        {
            Log($"Update error: {ex.Message}");
            MessageBox.Show(
                $"Update failed:\n{ex.Message}\n\n" +
                "You can download the latest version manually from:\n" +
                "https://github.com/ItMeDiaTech/subnautica-capsule-mod/releases/latest",
                "Update Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            if (!IsDisposed)
            {
                btnCancel.Visible = false;
                lnkCheckUpdate.Enabled = true;
                UpdateStatus();
            }
            downloadCts?.Dispose();
            downloadCts = null;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        downloadCts?.Cancel();
        base.OnFormClosing(e);
    }

    private void SetButtonsEnabled(bool enabled)
    {
        btnInstall.Enabled = enabled;
        btnUninstall.Enabled = enabled;
        btnBrowse.Enabled = enabled;
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => Log(message));
            return;
        }

        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}
