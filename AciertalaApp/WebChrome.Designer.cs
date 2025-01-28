using System;

partial class WebChrome
{
    private Microsoft.Web.WebView2.WinForms.WebView2 browser;
    private System.Windows.Forms.Button btnBack;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Button btnForward; // Nuevo botón de "Adelante"

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
        this.btnBack = new System.Windows.Forms.Button();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.btnForward = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.browser)).BeginInit();
        this.SuspendLayout();

        // WebView2 Browser
        this.browser.CreationProperties = null;
        this.browser.DefaultBackgroundColor = System.Drawing.Color.White;
        this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
        this.browser.Location = new System.Drawing.Point(0, 0);
        this.browser.Name = "browser";
        this.browser.TabIndex = 0;
        this.browser.ZoomFactor = 1.0;

        // Back Button
        this.btnBack.Text = "◀ Atrás";
        this.btnBack.Location = new System.Drawing.Point(10, 100); // Más abajo (Y=100)
        this.btnBack.Size = new System.Drawing.Size(100, 35);
        this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnBack.FlatAppearance.BorderSize = 1;
        this.btnBack.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.btnBack.BackColor = System.Drawing.Color.LightGray;
        this.btnBack.ForeColor = System.Drawing.Color.Black;
        this.btnBack.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
        this.btnBack.MouseEnter += new System.EventHandler(this.BtnBack_MouseEnter);
        this.btnBack.MouseLeave += new System.EventHandler(this.BtnBack_MouseLeave);

        // Refresh Button
        this.btnRefresh.Text = "🔄 Actualizar";
        this.btnRefresh.Location = new System.Drawing.Point(10, 150); // Más abajo (Y=150)
        this.btnRefresh.Size = new System.Drawing.Size(100, 35);
        this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRefresh.FlatAppearance.BorderSize = 1;
        this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.btnRefresh.BackColor = System.Drawing.Color.LightGray;
        this.btnRefresh.ForeColor = System.Drawing.Color.Black;
        this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
        this.btnRefresh.MouseEnter += new System.EventHandler(this.BtnRefresh_MouseEnter);
        this.btnRefresh.MouseLeave += new System.EventHandler(this.BtnRefresh_MouseLeave);

        // Forward Button
        this.btnForward.Text = "Adelante ▶";
        this.btnForward.Location = new System.Drawing.Point(10, 200); // Más abajo (Y=200)
        this.btnForward.Size = new System.Drawing.Size(100, 35);
        this.btnForward.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnForward.FlatAppearance.BorderSize = 1;
        this.btnForward.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        this.btnForward.BackColor = System.Drawing.Color.LightGray;
        this.btnForward.ForeColor = System.Drawing.Color.Black;
        this.btnForward.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnForward.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnForward.Click += new System.EventHandler(this.BtnForward_Click);
        this.btnForward.MouseEnter += new System.EventHandler(this.BtnForward_MouseEnter);
        this.btnForward.MouseLeave += new System.EventHandler(this.BtnForward_MouseLeave);

        // WebChrome Form
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 600);
        this.Controls.Add(this.btnForward);
        this.Controls.Add(this.btnRefresh);
        this.Controls.Add(this.btnBack);
        this.Controls.Add(this.browser);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "WebChrome";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "WebChrome";
        this.Load += new System.EventHandler(this.WebChrome_Load);
        ((System.ComponentModel.ISupportInitialize)(this.browser)).EndInit();
        this.ResumeLayout(false);

        this.Deactivate += new EventHandler(this.WebChrome_Deactivate);
    }


}
