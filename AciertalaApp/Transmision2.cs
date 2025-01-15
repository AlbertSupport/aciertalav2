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
    private bool isFullScreen = false; // Variable para controlar el estado de pantalla completa

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

            // Ajustar la posición del botón "Actualizar" de manera automática
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int refreshButtonWidth = refreshButton.Width;
            int margin = 20;
            int refreshButtonX = screenWidth - refreshButtonWidth - margin;
            refreshButton.Location = new Point(refreshButtonX, 10);

            // Estilo para los botones
            backButton.BackColor = Color.FromArgb(0, 123, 255);
            backButton.ForeColor = Color.White;
            backButton.FlatStyle = FlatStyle.Flat;
            backButton.FlatAppearance.BorderSize = 0;
            backButton.Font = new Font("Arial", 10, FontStyle.Bold);
            backButton.Cursor = Cursors.Hand;

            refreshButton.BackColor = Color.FromArgb(40, 167, 69);
            refreshButton.ForeColor = Color.White;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Font = new Font("Arial", 10, FontStyle.Bold);
            refreshButton.Cursor = Cursors.Hand;

            fullscreenButton.BackColor = Color.FromArgb(0, 123, 255);  // Estilo similar al de "Atrás"
            fullscreenButton.ForeColor = Color.White;
            fullscreenButton.FlatStyle = FlatStyle.Flat;
            fullscreenButton.FlatAppearance.BorderSize = 0;
            fullscreenButton.Font = new Font("Arial", 10, FontStyle.Bold);
            fullscreenButton.Cursor = Cursors.Hand;

            // Configuración de los efectos visuales cuando el cursor pasa sobre los botones
            refreshButton.MouseEnter += (s, eventArgs) =>
            {
                refreshButton.BackColor = Color.FromArgb(32, 134, 45);
            };
            refreshButton.MouseLeave += (s, eventArgs) =>
            {
                refreshButton.BackColor = Color.FromArgb(40, 167, 69);
            };

            backButton.MouseEnter += (s, eventArgs) =>
            {
                backButton.BackColor = Color.FromArgb(0, 105, 217);
            };
            backButton.MouseLeave += (s, eventArgs) =>
            {
                backButton.BackColor = Color.FromArgb(0, 123, 255);
            };

            fullscreenButton.MouseEnter += (s, eventArgs) =>
            {
                fullscreenButton.BackColor = Color.FromArgb(0, 105, 217);
            };
            fullscreenButton.MouseLeave += (s, eventArgs) =>
            {
                fullscreenButton.BackColor = Color.FromArgb(0, 123, 255);
            };

            // Aseguramos que los botones estén visibles al inicio
            backButton.Visible = true;
            refreshButton.Visible = true;
            fullscreenButton.Visible = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al inicializar WebView2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        var currentScreen = Screen.FromPoint(Cursor.Position);
        int screenWidth = currentScreen.Bounds.Width;
        int fixedHeight = 1000;

        this.ClientSize = new Size(screenWidth, fixedHeight);
        this.Location = new Point(currentScreen.Bounds.X, currentScreen.Bounds.Y + 80);
    }

    private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        try
        {
            // Script para eliminar los elementos de inmediato
            string scriptToRemoveSection = @"
            var elements = document.querySelectorAll('.portada-principal, header, footer, #btnIframe, #boton_full_screen');
            elements.forEach(function(el) {
                el.remove();
            });
        ";

            // Ejecutar el script de inmediato para eliminar los elementos
            browser.CoreWebView2.ExecuteScriptAsync(scriptToRemoveSection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al inyectar script: {ex.Message}");
        }

        // Ejecutar otros scripts que no dependen de la carga de elementos
        InjectScriptToHideAds();
        InjectCustomScript();
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
                var button = document.getElementById('boton_full_screen');
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

    private void InjectCustomScript()
    {
        try
        {
            string script = @"
        (function() {
            function removeAds() {
                document.body.classList.remove('blur');
                document.body.style.filter = 'none';

                document.querySelectorAll('iframe').forEach(iframe => {
                    const style = window.getComputedStyle(iframe);
                    if (
                        !iframe.src ||
                        iframe.src.includes('about:blank') ||
                        style.opacity === '0' ||
                        style.display === 'none' ||
                        iframe.offsetWidth <= 1 ||
                        iframe.offsetHeight <= 1
                    ) {
                        iframe.remove();
                    }
                });

                document.querySelectorAll('*').forEach(el => {
                    const styles = window.getComputedStyle(el);
                    if (
                        parseInt(styles.zIndex) > 1000 ||
                        styles.position === 'fixed' ||
                        el.innerText?.includes('Giros de Suerte') ||
                        el.innerText?.includes('meridiancasino')
                    ) {
                        el.remove();
                    }
                });

                document.querySelectorAll('.item_video').forEach(function(el) {
                    el.style.display = 'none';
                });
            }

            function toggleFullScreenForAnyVideo() {
                const videos = document.querySelectorAll('video, iframe'); 
                for (const video of videos) {
                    const style = window.getComputedStyle(video);
                    if (
                        video.offsetWidth > 1 && 
                        video.offsetHeight > 1 &&
                        style.display !== 'none' &&
                        style.visibility !== 'hidden'
                    ) {
                        if (!document.fullscreenElement) {
                            if (video.requestFullscreen) {
                                video.requestFullscreen();
                            } else if (video.mozRequestFullScreen) {
                                video.mozRequestFullScreen();
                            } else if (video.webkitRequestFullscreen) {
                                video.webkitRequestFullscreen();
                            } else if (video.msRequestFullscreen) {
                                video.msRequestFullscreen();
                            }
                        } else {
                            if (document.exitFullscreen) {
                                document.exitFullscreen();
                            } else if (document.mozCancelFullScreen) {
                                document.mozCancelFullScreen();
                            } else if (document.webkitExitFullscreen) {
                                document.webkitExitFullscreen();
                            } else if (document.msExitFullscreen) {
                                document.msExitFullscreen();
                            }
                        }
                        break;                     
                    }
                }
            }

            const observer = new MutationObserver((mutations) => {
                removeAds();
            });

            observer.observe(document.documentElement, {
                childList: true,
                subtree: true,
                attributes: true
            });

            window.toggleFullScreenForAnyVideo = toggleFullScreenForAnyVideo;
            removeAds();
        })();
        ";

            browser.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al inyectar script personalizado: {ex.Message}");
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
        // Función para ir hacia atrás en el navegador
        if (browser.CoreWebView2.CanGoBack)
        {
            browser.CoreWebView2.GoBack();
        }
        else
        {
            MessageBox.Show("No hay historial para regresar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void RefreshButton_Click(object sender, EventArgs e)
    {
        // Función para recargar la página
        browser.CoreWebView2.Reload();
    }

    private void FullscreenButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Cambiar el estado de pantalla completa
            isFullScreen = !isFullScreen;

            // Si estamos en pantalla completa, ocultamos los botones de Atrás y Actualizar
            if (isFullScreen)
            {
                backButton.Visible = false;
                refreshButton.Visible = false;
            }
            else
            {
                backButton.Visible = true;
                refreshButton.Visible = true;
            }

            // Ejecutamos el script para poner el video en pantalla completa
            string script = "window.toggleFullScreenForAnyVideo();";
            browser.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al ejecutar el script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
