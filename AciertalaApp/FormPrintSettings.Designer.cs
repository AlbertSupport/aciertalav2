namespace AciertalaApp
{
    partial class FormPrintSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbPrinters = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.rb58mm = new System.Windows.Forms.RadioButton();
            this.rb80mm = new System.Windows.Forms.RadioButton();
            this.Label2 = new System.Windows.Forms.Label();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.btnSeleccionarLogo = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.Button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbPrinters
            // 
            this.cmbPrinters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPrinters.FormattingEnabled = true;
            this.cmbPrinters.Location = new System.Drawing.Point(117, 45);
            this.cmbPrinters.Name = "cmbPrinters";
            this.cmbPrinters.Size = new System.Drawing.Size(151, 28);
            this.cmbPrinters.TabIndex = 0;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(29, 49);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(76, 20);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "Impresora";
            // 
            // rb58mm
            // 
            this.rb58mm.AutoSize = true;
            this.rb58mm.Location = new System.Drawing.Point(29, 101);
            this.rb58mm.Name = "rb58mm";
            this.rb58mm.Size = new System.Drawing.Size(142, 24);
            this.rb58mm.TabIndex = 2;
            this.rb58mm.TabStop = true;
            this.rb58mm.Text = "Impresión 58mm";
            this.rb58mm.UseVisualStyleBackColor = true;
            // 
            // rb80mm
            // 
            this.rb80mm.AutoSize = true;
            this.rb80mm.Location = new System.Drawing.Point(29, 147);
            this.rb80mm.Name = "rb80mm";
            this.rb80mm.Size = new System.Drawing.Size(142, 24);
            this.rb80mm.TabIndex = 3;
            this.rb80mm.TabStop = true;
            this.rb80mm.Text = "Impresión 80mm";
            this.rb80mm.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(345, 59);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(113, 20);
            this.Label2.TabIndex = 6;
            this.Label2.Text = "Logo Impresión";
            // 
            // picLogo
            // 
            this.picLogo.Location = new System.Drawing.Point(473, 28);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(149, 77);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLogo.TabIndex = 7;
            this.picLogo.TabStop = false;
            // 
            // btnSeleccionarLogo
            // 
            this.btnSeleccionarLogo.Location = new System.Drawing.Point(641, 53);
            this.btnSeleccionarLogo.Name = "btnSeleccionarLogo";
            this.btnSeleccionarLogo.Size = new System.Drawing.Size(94, 29);
            this.btnSeleccionarLogo.TabIndex = 8;
            this.btnSeleccionarLogo.Text = "Seleccionar";
            this.btnSeleccionarLogo.UseVisualStyleBackColor = true;
            this.btnSeleccionarLogo.Click += new System.EventHandler(this.btnSeleccionarLogo_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(29, 359);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(221, 26);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "Guardar";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // Button1
            // 
            this.Button1.Location = new System.Drawing.Point(589, 359);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(222, 29);
            this.Button1.TabIndex = 9;
            this.Button1.Text = "Restablecer Configuración";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // FormPrintSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 417);
            this.Controls.Add(this.Button1);
            this.Controls.Add(this.btnSeleccionarLogo);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.rb80mm);
            this.Controls.Add(this.rb58mm);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.cmbPrinters);
            this.Name = "FormPrintSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormPrintSettings";
            this.Load += new System.EventHandler(this.FormPrintSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox cmbPrinters;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.RadioButton rb58mm;
        private System.Windows.Forms.RadioButton rb80mm;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Button btnSeleccionarLogo;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button Button1;
    }
}
