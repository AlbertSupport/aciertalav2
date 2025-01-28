using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;


    public partial class WebSheets : Form
    {
    public WebSheets()
    {
        // Verificar si el formulario ya está abierto
        foreach (Form openForm in Application.OpenForms)
        {
            if (openForm is WebSheets && openForm != this)
            {
                openForm.Close(); // Cerrar la instancia anterior
                break;
            }
        }

        InitializeComponent();
    }



    private async void WebSheets_Load(object sender, EventArgs e)
        {
            try
            {

                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "aciertalaapp"
                );

                // 2. Crear el environment de WebView2 con la carpeta de usuario
                var environment = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: userDataFolder,
                    options: null  // Opcionalmente, podrías usar nuevas opciones
                );

                // 3. Asegurarte de inicializar la instancia de WebView2 con el environment
                await browser.EnsureCoreWebView2Async(environment);

                // 4. Navegar a la URL de Google Sheets.
                //    "u/0/" indica la cuenta principal; a veces se usa "u/1/" para otra cuenta, etc.
                string url = "https://docs.google.com/spreadsheets/u/0/";

                // 5. Verificar que la URL sea válida (no es estrictamente necesario, pero es buena práctica)
                if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    browser.CoreWebView2.Navigate(url);
                }
                else
                {
                    MessageBox.Show(
                        "URL no válida o vacía.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar WebView2: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Ajusta el tamaño de la ventana al ancho del monitor actual
            var currentScreen = Screen.FromPoint(Cursor.Position);
            int screenWidth = currentScreen.Bounds.Width;
            int fixedHeight = 1000;

            // Configurar el tamaño y la ubicación de la ventana
            this.ClientSize = new Size(screenWidth, fixedHeight);
            this.Location = new Point(currentScreen.Bounds.X, currentScreen.Bounds.Y + 80);
        }

        private void WebSheets_Deactivate(object sender, EventArgs e)
        {
            this.Close(); // Cierra el formulario al quedar en segundo plano
        }


}

