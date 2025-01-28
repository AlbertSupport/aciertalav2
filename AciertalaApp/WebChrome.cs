using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public partial class WebChrome : Form
{
    public WebChrome()
    {
        InitializeComponent();
    }

    private async void WebChrome_Load(object sender, EventArgs e)
    {
        try
        {
            string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "aciertalaapp");
            var envOptions = new CoreWebView2EnvironmentOptions();
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, envOptions);

            await browser.EnsureCoreWebView2Async(environment);

            // Configura el evento para bloquear popups
            browser.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

            // Navegar a la URL especificada
            string url = "https://www.google.com/"; // Ajusta la URL que quieres mostrar en este formulario
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

    private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        // Cancela la solicitud de abrir un popup sin mostrar el mensaje
        e.Handled = true;
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

    private void BtnBack_Click(object sender, EventArgs e)
    {
        if (browser != null && browser.CoreWebView2 != null && browser.CoreWebView2.CanGoBack)
        {
            browser.CoreWebView2.GoBack();
        }
        else
        {
            MessageBox.Show("No hay páginas anteriores para volver.", "Atrás", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnRefresh_Click(object sender, EventArgs e)
    {
        if (browser != null && browser.CoreWebView2 != null)
        {
            browser.CoreWebView2.Reload();
        }
    }

    private void BtnForward_Click(object sender, EventArgs e)
    {
        if (browser != null && browser.CoreWebView2 != null && browser.CoreWebView2.CanGoForward)
        {
            browser.CoreWebView2.GoForward();
        }
        else
        {
            MessageBox.Show("No hay una página siguiente en el historial.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnBack_MouseEnter(object sender, EventArgs e)
    {
        btnBack.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
    }

    private void BtnBack_MouseLeave(object sender, EventArgs e)
    {
        btnBack.BackColor = System.Drawing.Color.LightGray;
    }

    private void BtnRefresh_MouseEnter(object sender, EventArgs e)
    {
        btnRefresh.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
    }

    private void BtnRefresh_MouseLeave(object sender, EventArgs e)
    {
        btnRefresh.BackColor = System.Drawing.Color.LightGray;
    }

    private void BtnForward_MouseEnter(object sender, EventArgs e)
    {
        btnForward.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
    }

    private void BtnForward_MouseLeave(object sender, EventArgs e)
    {
        btnForward.BackColor = System.Drawing.Color.LightGray;
    }

    private void WebChrome_Deactivate(object sender, EventArgs e)
    {
        this.Close(); // Cierra el formulario al quedar en segundo plano
    }

}
