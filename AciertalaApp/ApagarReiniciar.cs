using System;
using System.Windows.Forms;
using System.Drawing;

namespace AciertalaApp
{
    public partial class ApagarReiniciar : Form
    {
        public ApagarReiniciar()
        {
            // Verificar si el formulario ya está abierto
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm is ApagarReiniciar && openForm != this)
                {
                    openForm.Close(); // Cerrar la instancia anterior
                    break;
                }
            }

            InitializeComponent();
        }

        // Evento para el botón "Apagar"
        private void BtnApagar_Click(object sender, EventArgs e)
        {
            // Mostrar el cuadro de mensaje de confirmación para apagar
            var result = MessageBox.Show("¿Estás seguro de que deseas apagar la PC?",
                "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Apagar la PC
                ApagarPC();
            }
        }

        // Evento para el botón "Reiniciar"
        private void BtnReiniciar_Click(object sender, EventArgs e)
        {
            // Mostrar el cuadro de mensaje de confirmación para reiniciar
            var result = MessageBox.Show("¿Estás seguro de que deseas reiniciar la PC?",
                "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Reiniciar la PC
                ReiniciarPC();
            }
        }

        // Método para apagar la PC
        private void ApagarPC()
        {
            try
            {
                System.Diagnostics.Process.Start("shutdown", "/s /f /t 0");  // Apagar la PC
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar apagar la PC: " + ex.Message);
            }
        }

        // Método para reiniciar la PC
        private void ReiniciarPC()
        {
            try
            {
                System.Diagnostics.Process.Start("shutdown", "/r /f /t 0");  // Reiniciar la PC
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar reiniciar la PC: " + ex.Message);
            }
        }
    }
}
