using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

public partial class RegistroQr : Form
{
    public RegistroQr()
    {
        InitializeComponent();
    }

    private async void RegistroQr_Load(object sender, EventArgs e)
    {
        try
        {
            // Ruta al archivo storage.txt
            string storageFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "aciertalaapp", "storage.txt");

            // Leer la URL desde el archivo storage.txt
            string qrURL = ReadQrUrlFromStorage(storageFilePath);

            // Validar la URL leída
            if (!string.IsNullOrEmpty(qrURL) && Uri.IsWellFormedUriString(qrURL, UriKind.Absolute))
            {
                string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "aciertalaapp");
                var envOptions = new CoreWebView2EnvironmentOptions();
                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, envOptions);

                await browser.EnsureCoreWebView2Async(environment);

                // Navegar a la URL leída
                browser.CoreWebView2.Navigate(qrURL);
            }
            else
            {
                MessageBox.Show("URL no válida o vacía en el archivo de configuración.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al inicializar WebView2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Lee la URL asociada a qrURL desde el archivo storage.txt.
    /// </summary>
    /// <param name="filePath">Ruta completa al archivo storage.txt.</param>
    /// <returns>El valor de qrURL si existe; de lo contrario, cadena vacía.</returns>
    private string ReadQrUrlFromStorage(string filePath)
    {
        if (!File.Exists(filePath))
        {
            MessageBox.Show("No se encontró el archivo de configuración.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.Empty;
        }

        try
        {
            // Leer todas las líneas del archivo
            var lines = File.ReadAllLines(filePath);

            // Buscar la línea que contiene "qrURL"
            foreach (var line in lines)
            {
                if (line.StartsWith("qrURL=", StringComparison.OrdinalIgnoreCase))
                {
                    // Extraer el valor después de "qrURL="
                    return Uri.UnescapeDataString(line.Substring("qrURL=".Length));
                }
            }

            MessageBox.Show("No se encontró 'qrURL' en el archivo de configuración.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al leer el archivo de configuración: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return string.Empty;
    }

    private void RegistroQr_Deactivate(object sender, EventArgs e)
    {
        this.Close(); // Cierra el formulario al quedar en segundo plano
    }

}
