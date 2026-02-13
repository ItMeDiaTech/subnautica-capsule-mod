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
        this.SuspendLayout();
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(500, 200);
        this.Name = "MainForm";
        this.Text = "Unlimited Time Capsules - Installer";
        this.ResumeLayout(false);
    }
}
