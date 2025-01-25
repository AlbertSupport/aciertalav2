using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

    public partial class Transmision3 : Form
    {
        public Transmision3()
        {
            InitializeComponent();
        }

        private async void Transmision3_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;

            // Ajustar el tamaño del formulario y su ubicación
            var currentScreen = Screen.FromPoint(Cursor.Position);
            int screenWidth = currentScreen.Bounds.Width;
            int fixedHeight = 1000; // Altura fija
            this.ClientSize = new Size(screenWidth, fixedHeight);
            this.Location = new Point(currentScreen.Bounds.X, currentScreen.Bounds.Y + 80); // Posición manual

            this.TopMost = false;

            try
            {
                await browser.EnsureCoreWebView2Async();
                ConfigureWebView2PopupBlocking();

                browser.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

                browser.CoreWebView2.Navigate("https://rojadirectaenhd.net/");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inicializando WebView2: " + ex.Message);
            }
        }



        private void ConfigureWebView2PopupBlocking()
        {
            browser.CoreWebView2.NewWindowRequested += HandleNewWindowRequested;
        }

        private void HandleNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            try
            {
                string targetUri = e.Uri;

            https://rojadirectaenhd.net/
                if (targetUri.StartsWith("https://rojadirectaenhd.net/"))
                {

                    e.Handled = true;
                    browser.CoreWebView2.Navigate(targetUri);
                }
                else
                {

                    e.Handled = true;
                    Debug.WriteLine($"Ventana emergente bloqueada: {e.Uri}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error manejando ventana emergente: {ex.Message}");
            }
        }


        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                browser.CoreWebView2.ExecuteScriptAsync(@"
                
                document.querySelectorAll('header, footer').forEach(function(el) {
                    el.style.display = 'none';
                });
                
                var titleDiv = document.querySelector('.title');
            if (titleDiv) {
                titleDiv.style.display = 'none';
            }

            
            var shareButtons = document.querySelector('.sharethis-inline-share-buttons.st-center.st-lang-es.st-has-labels.st-inline-share-buttons.st-animated');
            if (shareButtons) {
                shareButtons.style.display = 'none';
            }
               
                var button = document.getElementById('btnIframe');
                if (button) {
                    button.style.display = 'none';
                }
            ");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inyectando script: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (browser.CoreWebView2.CanGoBack)
            {
                browser.CoreWebView2.GoBack();
            }
            else
            {
                MessageBox.Show("No hay historial para volver atrás.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            browser.CoreWebView2.Reload();
        }
    }