
    partial class Utilitarios
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.GroupBox groupBoxOffice;
        private System.Windows.Forms.GroupBox groupBoxTools;
        private System.Windows.Forms.Button buttonExcel;
        private System.Windows.Forms.Button buttonWord;
        private System.Windows.Forms.Button buttonCalculadora;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.groupBoxOffice = new System.Windows.Forms.GroupBox();
            this.buttonExcel = new System.Windows.Forms.Button();
            this.buttonWord = new System.Windows.Forms.Button();
            this.groupBoxTools = new System.Windows.Forms.GroupBox();
            this.buttonCalculadora = new System.Windows.Forms.Button();
            this.groupBoxOffice.SuspendLayout();
            this.groupBoxTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxOffice
            // 
            this.groupBoxOffice.Controls.Add(this.buttonExcel);
            this.groupBoxOffice.Controls.Add(this.buttonWord);
            this.groupBoxOffice.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBoxOffice.ForeColor = System.Drawing.Color.White;
            this.groupBoxOffice.Location = new System.Drawing.Point(15, 16);
            this.groupBoxOffice.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxOffice.Name = "groupBoxOffice";
            this.groupBoxOffice.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxOffice.Size = new System.Drawing.Size(195, 98);
            this.groupBoxOffice.TabIndex = 0;
            this.groupBoxOffice.TabStop = false;
            this.groupBoxOffice.Text = "Herramientas de Office";
            // 
            // buttonExcel
            // 
            this.buttonExcel.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.buttonExcel.FlatAppearance.BorderSize = 0;
            this.buttonExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExcel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.buttonExcel.ForeColor = System.Drawing.Color.White;
            this.buttonExcel.Location = new System.Drawing.Point(15, 24);
            this.buttonExcel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonExcel.Name = "buttonExcel";
            this.buttonExcel.Size = new System.Drawing.Size(165, 24);
            this.buttonExcel.TabIndex = 1;
            this.buttonExcel.Text = "Excel";
            this.buttonExcel.UseVisualStyleBackColor = false;
            this.buttonExcel.Click += new System.EventHandler(this.buttonExcel_Click);
            // 
            // buttonWord
            // 
            this.buttonWord.BackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonWord.FlatAppearance.BorderSize = 0;
            this.buttonWord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonWord.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.buttonWord.ForeColor = System.Drawing.Color.White;
            this.buttonWord.Location = new System.Drawing.Point(15, 57);
            this.buttonWord.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonWord.Name = "buttonWord";
            this.buttonWord.Size = new System.Drawing.Size(165, 24);
            this.buttonWord.TabIndex = 2;
            this.buttonWord.Text = "Word";
            this.buttonWord.UseVisualStyleBackColor = false;
            this.buttonWord.Click += new System.EventHandler(this.buttonWord_Click);
            // 
            // groupBoxTools
            // 
            this.groupBoxTools.Controls.Add(this.buttonCalculadora);
            this.groupBoxTools.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBoxTools.ForeColor = System.Drawing.Color.White;
            this.groupBoxTools.Location = new System.Drawing.Point(15, 130);
            this.groupBoxTools.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxTools.Name = "groupBoxTools";
            this.groupBoxTools.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxTools.Size = new System.Drawing.Size(195, 65);
            this.groupBoxTools.TabIndex = 1;
            this.groupBoxTools.TabStop = false;
            this.groupBoxTools.Text = "Herramientas";
            // 
            // buttonCalculadora
            // 
            this.buttonCalculadora.BackColor = System.Drawing.Color.DarkOrange;
            this.buttonCalculadora.FlatAppearance.BorderSize = 0;
            this.buttonCalculadora.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCalculadora.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.buttonCalculadora.ForeColor = System.Drawing.Color.White;
            this.buttonCalculadora.Location = new System.Drawing.Point(15, 24);
            this.buttonCalculadora.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonCalculadora.Name = "buttonCalculadora";
            this.buttonCalculadora.Size = new System.Drawing.Size(165, 24);
            this.buttonCalculadora.TabIndex = 3;
            this.buttonCalculadora.Text = "Calculadora";
            this.buttonCalculadora.UseVisualStyleBackColor = false;
            this.buttonCalculadora.Click += new System.EventHandler(this.buttonCalculadora_Click_1);
            // 
            // Utilitarios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(225, 211);
            this.Controls.Add(this.groupBoxTools);
            this.Controls.Add(this.groupBoxOffice);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Utilitarios";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Utilitarios";
            this.groupBoxOffice.ResumeLayout(false);
            this.groupBoxTools.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }