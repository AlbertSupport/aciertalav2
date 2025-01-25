partial class TerminalLogin
{
    /// <summary>
    /// Variable del diseñador necesaria.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Limpiar los recursos que se estén usando.
    /// </summary>
    /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Código generado por el Diseñador de Windows Forms

    /// <summary>
    /// Método necesario para admitir el Diseñador. No modifique
    /// el contenido de este método con el editor de código.
    /// </summary>
    private void InitializeComponent()
    {
        this.browser = new Microsoft.Web.WebView2.WinForms.WebView2();
        this.SuspendLayout();

        // 
        // browser
        // 
        this.browser.CreationProperties = null;
        this.browser.DefaultBackgroundColor = System.Drawing.Color.White;
        this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
        this.browser.Location = new System.Drawing.Point(0, 0);
        this.browser.Name = "browser";
        this.browser.Size = new System.Drawing.Size(800, 450);
        this.browser.TabIndex = 0;
        this.browser.ZoomFactor = 1D;

        // 
        // TerminalLogin
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.browser);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "TerminalLogin";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "TerminalLogin";
        this.Load += new System.EventHandler(this.TerminalLogin_Load);
        this.ResumeLayout(false);
    }

    #endregion

    private Microsoft.Web.WebView2.WinForms.WebView2 browser;
}
