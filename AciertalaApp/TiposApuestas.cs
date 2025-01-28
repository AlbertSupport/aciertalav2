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

public partial class TiposApuestas : Form
{
    public TiposApuestas()
    {
        InitializeComponent();
    }

    private async void TiposApuestas_Load(object sender, EventArgs e)
    {
        try
        {
            string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "aciertalaapp");
            var envOptions = new CoreWebView2EnvironmentOptions();
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, envOptions);

            await browser.EnsureCoreWebView2Async(environment);

            string url = "https://peru.aciertala.com/definiciones-de-apuestas";
            if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                browser.CoreWebView2.Navigate(url);

                // Suscribir al evento NavigationCompleted para ejecutar el script
                browser.CoreWebView2.NavigationCompleted += Browser_NavigationCompleted;
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

    private async void Browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            try
            {
                // Script para ocultar los divs con id 'u_row_5', 'u_row_11', 'u_column_4' y 'st-2'
                string script = @"
                (function() {
                    var result = '';

                    // Ocultar el elemento con id 'u_row_5'
                    var row5 = document.getElementById('u_row_5');
                    if (row5) {
                        row5.style.display = 'none';
                        result += 'El elemento u_row_5 se ha ocultado correctamente. ';
                    } else {
                        result += 'El elemento u_row_5 no fue encontrado. ';
                    }

                    // Ocultar el elemento con id 'u_row_11'
                    var row11 = document.getElementById('u_row_11');
                    if (row11) {
                        row11.style.display = 'none';
                        result += 'El elemento u_row_11 se ha ocultado correctamente. ';
                    } else {
                        result += 'El elemento u_row_11 no fue encontrado. ';
                    }

                    // Ocultar el elemento con id 'u_column_4'
                    var column4 = document.getElementById('u_column_4');
                    if (column4) {
                        column4.style.display = 'none';
                        result += 'El elemento u_column_4 se ha ocultado correctamente. ';
                    } else {
                        result += 'El elemento u_column_4 no fue encontrado. ';
                    }

                    // Ocultar el elemento con id 'st-2'
                    var st2 = document.getElementById('st-2');
                    if (st2) {
                        st2.style.display = 'none';
                        result += 'El elemento st-2 se ha ocultado correctamente.';
                    } else {
                        result += 'El elemento st-2 no fue encontrado.';
                    }

                    return result;
                })();
            ";

                // Ejecutar el script en la página
                string result = await browser.CoreWebView2.ExecuteScriptAsync(script);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ejecutar el script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            MessageBox.Show("La navegación no se completó correctamente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Tiposapuesta_Deactivate(object sender, EventArgs e)
    {
        this.Close(); // Cierra el formulario al quedar en segundo plano
    }


}
