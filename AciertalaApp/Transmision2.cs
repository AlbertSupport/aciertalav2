using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public partial class Transmision2 : Form
{
    public Transmision2()
    {
        InitializeComponent();
    }

    private async void Transmision2_Load(object sender, EventArgs e)
    {
        try
        {
            // Asegúrate de que WebView2 esté inicializado
            await browser.EnsureCoreWebView2Async(null);

            // Configuración del WebView2
            browser.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

            // Configurar comportamientos específicos
            ConfigureWebView2PopupBlocking();

            ConfigureWebView2RequestBlocking();

            // Navegar a la URL especificada
            string url = "https://playviper.com/";
            if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                browser.CoreWebView2.Navigate(url);
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



    private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        InjectScriptWithInterval(sender, e);
        InjectScriptToHideAds();
    }


    private void InjectScriptWithInterval(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        try
        {

            string scriptToHideSection = @"
                
                var portada = document.querySelector('.portada-principal');
                if(portada) { portada.style.display = 'none'; }

                
                document.querySelectorAll('header, footer').forEach(function(el) {
                    el.style.display = 'none';
                });

                
                var button = document.getElementById('btnIframe');
                if (button) {
                    button.style.display = 'none';
                }
            ";


            browser.CoreWebView2.ExecuteScriptAsync(scriptToHideSection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al inyectar script: {ex.Message}");
        }
    }


    private void InjectScriptToHideAds()
    {
        try
        {
            string scriptToHideAds = @"
                // Ocultar elementos de anuncios comunes
                var ads = document.querySelectorAll('.ad-container, .fullscreen-ad, [id*=""ad""]');
                ads.forEach(function(ad) {
                    ad.style.display = 'none';
                });
            ";

            browser.CoreWebView2.ExecuteScriptAsync(scriptToHideAds);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al inyectar script para anuncios: {ex.Message}");
        }
    }



    private void ConfigureWebView2RequestBlocking()
    {
        if (browser.CoreWebView2 != null)
        {

            browser.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            browser.CoreWebView2.WebResourceRequested += HandleWebResourceRequested;
        }
        else
        {

            browser.CoreWebView2InitializationCompleted += (sender, e) =>
            {
                if (e.IsSuccess)
                {
                    browser.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                    browser.CoreWebView2.WebResourceRequested += HandleWebResourceRequested;
                }
                else
                {
                    MessageBox.Show($"Error al inicializar CoreWebView2: {e.InitializationException.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }
    }

    private readonly List<string> BlockedDomains = new List<string>
    {
        "https://xml-v4.pushub.net/",
        "https://cdn.amnew.net/",
        "https://us.opencan.net/",
        "https://xml-v4.pushub.net"
         
    };

    private void HandleWebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        try
        {

            foreach (var domain in BlockedDomains)
            {
                if (e.Request.Uri.StartsWith(domain, StringComparison.OrdinalIgnoreCase))
                {

                    e.Response = browser.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Forbidden", null);
                    Debug.WriteLine($"Solicitud bloqueada: {e.Request.Uri}");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al manejar la solicitud: {ex.Message}");
        }
    }

    private void ConfigureWebView2PopupBlocking()
    {
        if (browser.CoreWebView2 != null)
        {

            browser.CoreWebView2.NewWindowRequested += HandleNewWindowRequested;
        }
        else
        {

            browser.CoreWebView2InitializationCompleted += (sender, e) =>
            {
                if (e.IsSuccess)
                {
                    browser.CoreWebView2.NewWindowRequested += HandleNewWindowRequested;
                }
                else
                {
                    MessageBox.Show($"Error al inicializar CoreWebView2: {e.InitializationException.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }
    }


    private void HandleNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        try
        {

            e.Handled = true;
            Debug.WriteLine($"Ventana emergente bloqueada: {e.Uri}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error manejando ventana emergente: {ex.Message}");
        }
    }

    private void BackButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (browser.CanGoBack)
            {
                browser.GoBack();
            }
            else
            {
                MessageBox.Show("No hay páginas anteriores para regresar.", "Atrás", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al intentar regresar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    private void RefreshButton_Click(object sender, EventArgs e)
    {
        try
        {
            browser.Reload();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al intentar recargar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}