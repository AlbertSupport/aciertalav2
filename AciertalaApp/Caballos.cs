using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public partial class Caballos : Form
{
    public Caballos()
    {
        InitializeComponent();
    }

    private async void Caballos_Load(object sender, EventArgs e)
    {
        try
        {
            string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "aciertalaapp");
            var envOptions = new CoreWebView2EnvironmentOptions();
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, envOptions);

            await browser.EnsureCoreWebView2Async(environment);

            string url = "https://retailhorse.aciertala.com/";
            if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                browser.CoreWebView2.Navigate(url);

                // Ajustar el factor de zoom en el control WebView2
                browser.ZoomFactor = 1.0; // Escala al 100% (puedes cambiar este valor para ajustar el zoom)
            }
            else
            {
                MessageBox.Show("URL no válida o vacía.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al inicializar WebView2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }



    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Obtén la pantalla actual
        var currentScreen = Screen.FromPoint(Cursor.Position);
        int screenWidth = currentScreen.Bounds.Width;
        int fixedHeight = 1000;

        // Configura el tamaño y posición del formulario antes de que sea visible
        this.ClientSize = new Size(screenWidth, fixedHeight);
        this.Location = new Point(currentScreen.Bounds.X, currentScreen.Bounds.Y + 80); // Respeta el desplazamiento vertical de 80 píxeles
    }

    private void Caballos_Deactivate(object sender, EventArgs e)
    {
        this.Close(); // Cierra el formulario al quedar en segundo plano
    }

}
