partial class Transmision2
{
    private Microsoft.Web.WebView2.WinForms.WebView2 browser;
    private System.Windows.Forms.Button backButton;
    private System.Windows.Forms.Button refreshButton;
    private System.Windows.Forms.Button fullscreenButton; // Nuevo botón "Fullscreen"

    /// <summary>
    /// Requerido por el diseñador.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Limpiar los recursos que se estén usando.
    /// </summary>
    /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario.</param>
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
    /// Método necesario para admitir el Diseñador. No se puede modificar
    /// el contenido de este método con el editor de código.
    /// </summary>
    private void InitializeComponent()
    {
        this.browser = new Microsoft.Web.WebView2.WinForms.WebView2();
        this.backButton = new System.Windows.Forms.Button();
        this.refreshButton = new System.Windows.Forms.Button();
        this.fullscreenButton = new System.Windows.Forms.Button(); // Declaración del nuevo botón
        ((System.ComponentModel.ISupportInitialize)(this.browser)).BeginInit();
        this.SuspendLayout();
        // 
        // browser
        // 
        this.browser.CreationProperties = null;
        this.browser.DefaultBackgroundColor = System.Drawing.Color.White;
        this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
        this.browser.Location = new System.Drawing.Point(0, 50); // Dejar espacio en la parte superior para los botones
        this.browser.Name = "browser";
        this.browser.TabIndex = 0;
        this.browser.ZoomFactor = 1.0;
        this.browser.NavigationCompleted += InjectScriptWithInterval;
        // 
        // backButton
        // 
        this.backButton.Location = new System.Drawing.Point(10, 10); // Ubicación en la parte superior izquierda
        this.backButton.Name = "backButton";
        this.backButton.Size = new System.Drawing.Size(100, 30);
        this.backButton.TabIndex = 1;
        this.backButton.Text = "Atrás";
        this.backButton.UseVisualStyleBackColor = true;
        this.backButton.Click += new System.EventHandler(this.BackButton_Click);
        // 
        // refreshButton
        // 
        this.refreshButton.Location = new System.Drawing.Point(120, 10); // Ubicación a la derecha del botón "Atrás"
        this.refreshButton.Name = "refreshButton";
        this.refreshButton.Size = new System.Drawing.Size(100, 30);
        this.refreshButton.TabIndex = 2;
        this.refreshButton.Text = "Actualizar";
        this.refreshButton.UseVisualStyleBackColor = true;
        this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
        // 
        // fullscreenButton
        // 
        this.fullscreenButton.Location = new System.Drawing.Point(230, 10); // Ubicación a la derecha del botón "Actualizar"
        this.fullscreenButton.Name = "fullscreenButton";
        this.fullscreenButton.Size = new System.Drawing.Size(100, 30);
        this.fullscreenButton.TabIndex = 3;
        this.fullscreenButton.Text = "Fullscreen";
        this.fullscreenButton.UseVisualStyleBackColor = true;
        this.fullscreenButton.Click += new System.EventHandler(this.FullscreenButton_Click); 
        // 
        // Transmision2
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 600); // Tamaño inicial, se ajustará en Load
        this.Controls.Add(this.backButton); // Colocar el botón Atrás
        this.Controls.Add(this.refreshButton); // Colocar el botón Actualizar
        this.Controls.Add(this.fullscreenButton); // Colocar el botón Fullscreen
        this.Controls.Add(this.browser); // Colocar el WebView2 debajo de los botones
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "Transmision2";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "Transmision2";
        this.Load += new System.EventHandler(this.Transmision2_Load);
        ((System.ComponentModel.ISupportInitialize)(this.browser)).EndInit();
        this.ResumeLayout(false);
    }

    #endregion
}
