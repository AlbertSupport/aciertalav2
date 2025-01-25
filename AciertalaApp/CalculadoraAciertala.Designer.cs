using System.Drawing;
using System.Windows.Forms;


    partial class CalculadoraAciertala
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox textBoxResult;

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
            this.textBoxResult = new TextBox();
            this.SuspendLayout();

            // 
            // textBoxResult
            // 
            this.textBoxResult.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            this.textBoxResult.Location = new Point(10, 10);
            this.textBoxResult.Multiline = true; // Permitir múltiples líneas
            this.textBoxResult.ScrollBars = ScrollBars.Vertical; // Barra de desplazamiento si es necesario
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.ReadOnly = true;
            this.textBoxResult.Size = new Size(330, 80); // Tamaño inicial
            this.textBoxResult.TabIndex = 0;
            this.textBoxResult.TextAlign = HorizontalAlignment.Right; // Alinear el texto a la derecha
            this.textBoxResult.Text = "0"; // Valor inicial

            // 
            // CalculadoraAciertala
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(350, 500); // Tamaño inicial del formulario
            this.MinimumSize = new Size(350, 500); // Tamaño mínimo para el formulario
            this.Controls.Add(this.textBoxResult); // Añadir el TextBox al formulario
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // Bloquear el redimensionado
            this.MaximizeBox = false; // Desactivar maximizar
            this.Name = "CalculadoraAciertala";
            this.StartPosition = FormStartPosition.CenterScreen; // Centrar la ventana al abrir
            this.Text = "Calculadora";
            this.Resize += new System.EventHandler(this.CalculadoraAciertala_Resize); // Evento para redimensionar controles
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }