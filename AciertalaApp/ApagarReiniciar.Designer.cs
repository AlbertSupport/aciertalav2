using System.Drawing;
using System.Windows.Forms;

namespace AciertalaApp
{
    partial class ApagarReiniciar
    {
        private System.ComponentModel.IContainer components = null;

        // Elimina los recursos al cerrar el formulario
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            // Ajuste automático del tamaño de la ventana según los controles
            this.ClientSize = new System.Drawing.Size(250, 150);  // El tamaño de la ventana se ajustará a los botones
            this.Text = "Apagar/Reiniciar PC";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;  // Evitar cambiar tamaño
            this.MaximizeBox = false;  // Deshabilitar maximizar
            this.StartPosition = FormStartPosition.CenterScreen; // Centrar formulario en la pantalla

            // Establecer un fondo de color
            this.BackColor = Color.FromArgb(32, 32, 32);  // Color oscuro para el fondo

            // Botón "Apagar"
            Button btnApagar = new Button();
            btnApagar.Text = "Apagar PC";
            btnApagar.Width = 180;  // Ajustado para que sea más pequeño
            btnApagar.Height = 40;  // Ajustado para un tamaño más compacto
            btnApagar.BackColor = Color.Red;
            btnApagar.ForeColor = Color.White;
            btnApagar.Font = new Font("Arial", 10, FontStyle.Bold);  // Fuente más pequeña
            btnApagar.Location = new Point(30, 20); // Posición ajustada
            btnApagar.Click += BtnApagar_Click;
            btnApagar.FlatStyle = FlatStyle.Flat;
            btnApagar.FlatAppearance.BorderSize = 0;
            btnApagar.Cursor = Cursors.Hand;

            // Botón "Reiniciar"
            Button btnReiniciar = new Button();
            btnReiniciar.Text = "Reiniciar PC";
            btnReiniciar.Width = 180;  // Ajustado para que sea más pequeño
            btnReiniciar.Height = 40;  // Ajustado para un tamaño más compacto
            btnReiniciar.BackColor = Color.Orange;
            btnReiniciar.ForeColor = Color.White;
            btnReiniciar.Font = new Font("Arial", 10, FontStyle.Bold);  // Fuente más pequeña
            btnReiniciar.Location = new Point(30, 70); // Posición ajustada
            btnReiniciar.Click += BtnReiniciar_Click;
            btnReiniciar.FlatStyle = FlatStyle.Flat;
            btnReiniciar.FlatAppearance.BorderSize = 0;
            btnReiniciar.Cursor = Cursors.Hand;

            // Agregar los botones al formulario
            this.Controls.Add(btnApagar);
            this.Controls.Add(btnReiniciar);

            // Ajustar tamaño de la ventana al tamaño de los controles (botones)
            this.ClientSize = new Size(250, btnReiniciar.Bottom + 20); // Ajuste automático de la altura de la ventana
        }

        #endregion
    }
}
