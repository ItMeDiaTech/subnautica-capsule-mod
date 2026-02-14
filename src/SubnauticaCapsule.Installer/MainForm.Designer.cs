namespace SubnauticaCapsule.Installer;

partial class MainForm
{
#nullable enable
    private System.ComponentModel.IContainer? components = null;
#nullable restore

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.lblPath = new System.Windows.Forms.Label();
        this.txtPath = new System.Windows.Forms.TextBox();
        this.btnBrowse = new System.Windows.Forms.Button();
        this.grpStatus = new System.Windows.Forms.GroupBox();
        this.lblStatusGame = new System.Windows.Forms.Label();
        this.lblStatusBepInEx = new System.Windows.Forms.Label();
        this.lblStatusMod = new System.Windows.Forms.Label();
        this.btnInstall = new System.Windows.Forms.Button();
        this.btnUninstall = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnClose = new System.Windows.Forms.Button();
        this.txtLog = new System.Windows.Forms.TextBox();
        this.grpStatus.SuspendLayout();
        this.SuspendLayout();

        // lblPath
        this.lblPath.AutoSize = true;
        this.lblPath.Location = new System.Drawing.Point(14, 18);
        this.lblPath.Text = "Subnautica Path:";

        // txtPath
        this.txtPath.Location = new System.Drawing.Point(126, 15);
        this.txtPath.Size = new System.Drawing.Size(330, 23);
        this.txtPath.ReadOnly = true;

        // btnBrowse
        this.btnBrowse.Location = new System.Drawing.Point(462, 14);
        this.btnBrowse.Size = new System.Drawing.Size(80, 25);
        this.btnBrowse.Text = "Browse...";
        this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);

        // grpStatus
        this.grpStatus.Controls.Add(this.lblStatusGame);
        this.grpStatus.Controls.Add(this.lblStatusBepInEx);
        this.grpStatus.Controls.Add(this.lblStatusMod);
        this.grpStatus.Location = new System.Drawing.Point(14, 50);
        this.grpStatus.Size = new System.Drawing.Size(528, 90);
        this.grpStatus.Text = "Status";

        // lblStatusGame
        this.lblStatusGame.AutoSize = true;
        this.lblStatusGame.Location = new System.Drawing.Point(12, 22);
        this.lblStatusGame.Text = "Subnautica: Checking...";

        // lblStatusBepInEx
        this.lblStatusBepInEx.AutoSize = true;
        this.lblStatusBepInEx.Location = new System.Drawing.Point(12, 42);
        this.lblStatusBepInEx.Text = "BepInEx: Checking...";

        // lblStatusMod
        this.lblStatusMod.AutoSize = true;
        this.lblStatusMod.Location = new System.Drawing.Point(12, 62);
        this.lblStatusMod.Text = "Mod: Checking...";

        // btnInstall
        this.btnInstall.Location = new System.Drawing.Point(14, 150);
        this.btnInstall.Size = new System.Drawing.Size(120, 32);
        this.btnInstall.Text = "Install";
        this.btnInstall.Enabled = false;
        this.btnInstall.Click += new System.EventHandler(this.BtnInstall_Click);

        // btnUninstall
        this.btnUninstall.Location = new System.Drawing.Point(144, 150);
        this.btnUninstall.Size = new System.Drawing.Size(120, 32);
        this.btnUninstall.Text = "Uninstall";
        this.btnUninstall.Enabled = false;
        this.btnUninstall.Click += new System.EventHandler(this.BtnUninstall_Click);

        // btnCancel
        this.btnCancel.Location = new System.Drawing.Point(274, 150);
        this.btnCancel.Size = new System.Drawing.Size(90, 32);
        this.btnCancel.Text = "Cancel";
        this.btnCancel.Visible = false;
        this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

        // btnClose
        this.btnClose.Location = new System.Drawing.Point(422, 150);
        this.btnClose.Size = new System.Drawing.Size(120, 32);
        this.btnClose.Text = "Close";
        this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);

        // txtLog
        this.txtLog.Location = new System.Drawing.Point(14, 195);
        this.txtLog.Multiline = true;
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtLog.Size = new System.Drawing.Size(528, 130);
        this.txtLog.BackColor = System.Drawing.SystemColors.Window;

        // lnkCheckUpdate
        this.lnkCheckUpdate = new System.Windows.Forms.LinkLabel();
        this.lnkCheckUpdate.AutoSize = true;
        this.lnkCheckUpdate.Location = new System.Drawing.Point(14, 335);
        this.lnkCheckUpdate.Text = "Check for updates";
        this.lnkCheckUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkCheckUpdate_LinkClicked);

        // lblVersion
        this.lblVersion = new System.Windows.Forms.Label();
        this.lblVersion.AutoSize = true;
        this.lblVersion.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblVersion.Text = "v" + InstallerVersion.Current;
        this.lblVersion.Location = new System.Drawing.Point(494, 338);

        // MainForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(556, 365);
        this.Controls.Add(this.lblPath);
        this.Controls.Add(this.txtPath);
        this.Controls.Add(this.btnBrowse);
        this.Controls.Add(this.grpStatus);
        this.Controls.Add(this.btnInstall);
        this.Controls.Add(this.btnUninstall);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnClose);
        this.Controls.Add(this.txtLog);
        this.Controls.Add(this.lnkCheckUpdate);
        this.Controls.Add(this.lblVersion);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Unlimited Time Capsules - Installer";
        this.grpStatus.ResumeLayout(false);
        this.grpStatus.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Label lblPath = null!;
    private System.Windows.Forms.TextBox txtPath = null!;
    private System.Windows.Forms.Button btnBrowse = null!;
    private System.Windows.Forms.GroupBox grpStatus = null!;
    private System.Windows.Forms.Label lblStatusGame = null!;
    private System.Windows.Forms.Label lblStatusBepInEx = null!;
    private System.Windows.Forms.Label lblStatusMod = null!;
    private System.Windows.Forms.Button btnInstall = null!;
    private System.Windows.Forms.Button btnUninstall = null!;
    private System.Windows.Forms.Button btnCancel = null!;
    private System.Windows.Forms.Button btnClose = null!;
    private System.Windows.Forms.TextBox txtLog = null!;
    private System.Windows.Forms.LinkLabel lnkCheckUpdate = null!;
    private System.Windows.Forms.Label lblVersion = null!;
}
