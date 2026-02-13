using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SubnauticaCapsule.Installer;

public partial class MainForm : Form
{
    private string? gamePath;

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

        try
        {
            if (!ModInstaller.IsBepInExInstalled(gamePath))
            {
                Log("BepInEx is required. Installing...");
                await ModInstaller.DownloadAndInstallBepInExAsync(gamePath, progress, CancellationToken.None);
            }

            ModInstaller.InstallMod(gamePath, progress);
            Log("Installation complete.");
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
            UpdateStatus();
            SetButtonsEnabled(true);
        }
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

        try
        {
            ModInstaller.UninstallMod(gamePath, new Progress<string>(Log));
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
        }
        finally
        {
            UpdateStatus();
        }
    }

    private void BtnClose_Click(object? sender, EventArgs e)
    {
        Close();
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
