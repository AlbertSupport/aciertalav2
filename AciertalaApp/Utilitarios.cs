using System;
using System.Linq;
using System.Windows.Forms;

public partial class Utilitarios : Form
{
    public Utilitarios()
    {
        InitializeComponent();
    }

    public static void ShowAsPopup()
    {
        using (var popup = new Utilitarios())
        {
            popup.ShowDialog(); // Mostrar como ventana modal (popup)
        }
    }

    private void buttonCalculadora_Click_1(object sender, EventArgs e)
    {
        try
        {
            // Verificar si el formulario CalculadoraAciertala ya está abierto
            var calculadoraForm = Application.OpenForms.OfType<CalculadoraAciertala>().FirstOrDefault();

            if (calculadoraForm == null)
            {
                // Si no está abierto, crear una nueva instancia y mostrarla
                var nuevaCalculadora = new CalculadoraAciertala();
                nuevaCalculadora.Show(); // Mostrar el formulario
            }
            else
            {
                // Si ya está abierto, activarlo
                calculadoraForm.BringToFront(); // Traer al frente
                calculadoraForm.Activate();    // Activarlo
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al abrir la Calculadora: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void buttonExcel_Click(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start("excel"); // Abrir Excel
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al abrir Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void buttonWord_Click(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start("winword"); // Abrir Word
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al abrir Word: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
