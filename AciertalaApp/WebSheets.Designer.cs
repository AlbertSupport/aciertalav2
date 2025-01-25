partial class WebSheets
{
    private Microsoft.Web.WebView2.WinForms.WebView2 browser;

    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.browser = new Microsoft.Web.WebView2.WinForms.WebView2();
        ((System.ComponentModel.ISupportInitialize)(this.browser)).BeginInit();
        this.SuspendLayout();

        this.browser.CreationProperties = null;
        this.browser.DefaultBackgroundColor = System.Drawing.Color.White;
        this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
        this.browser.Location = new System.Drawing.Point(0, 0);
        this.browser.Name = "browser";
        this.browser.TabIndex = 0;
        this.browser.ZoomFactor = 1.0;

        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 600);
        this.Controls.Add(this.browser);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "Web Sheets";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "Web Sheets";
        this.Load += new System.EventHandler(this.WebSheets_Load);
        ((System.ComponentModel.ISupportInitialize)(this.browser)).EndInit();
        this.ResumeLayout(false);
    }
}
