using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

public partial class Transmision1 : Form
{
    public Transmision1()
    {
        InitializeComponent();
    }

    private async void Transmision1_Load(object sender, EventArgs e)
    {
        try
        {
            string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "aciertalaapp");
            var envOptions = new CoreWebView2EnvironmentOptions();
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, envOptions);

            await browser.EnsureCoreWebView2Async(environment);

            // Navegar a la URL especificada
            string url = "https://365livesport.org/";
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

    private async void Browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            try
            {
                // Script para ocultar las dos secciones específicas
                string script = @"(function() {
    const hideElements = () => {
        const selectors = [
            '.sc-bwsPYA.hCUxjX', 
            'a.header__logo', 
            '.sc-hLirLb.sc-hbqYmb.jGPdMe.fpmbEs',
            '.sc-hLirLb.sc-hbqYmb.jGPdMe.cafran',
            '.sc-hLirLb.sc-hbqYmb.jGPdMe.cafran img[alt=""Subscription""]',
            '.sc-hLirLb.sc-hbqYmb.jGPdMe.kszbty',
            'div.search.false', 
            'a.button.tg.undefined',
            'button.sc-jOiSOi.iFwTcU.sc-nTrUm.eDSTMV',
            'section.sc-enHPVx.icbqGE', 
            'a.j6075e732.__web-inspector-hide-shortcut__',
            'a[href*=""ads.adfox.ru/699683/clickURL""]',
            'jdiv.button__bEFyn',
            'jdiv.wrap__mwjDj._orientationRight__FZyz2._show__HwnHL._hoverMenu__NHxTH.__jivoDesktopButton.__web-inspector-hide-shortcut__',
            'div.share.__web-inspector-hide-shortcut__',
            'div.ya-share2.ya-share2_inited',
            'jdiv.iconWrap__SceC7',
            'jdiv.button__bEFyn[style*=""background: linear-gradient(95deg, rgb(211, 55, 55)""]',
            'div.share__text',
            '.sc-GKYbw.lkPYer', // Highlights elemento
            '.sc-itMJkM.jkTSKs', 

            
            // Elementos que deseas ocultar por su id
            '#Promociones',
            '#Escríbenos',
            '#Recarga\\ al\\ toque', // Escapamos el espacio con doble barra invertida
            '#Habilidad',
            
            // Nuevos elementos a ocultar
            '#Casino', 
            '#Live\\ Casino', // Escapamos el espacio con doble barra invertida
            
            // Nuevo elemento a ocultar: nvscore-carousel
            'nvscore-carousel' 
            


        ];

        // Seleccionamos los elementos completos que contienen los enlaces y ocultamos todo su contenedor <li>
        selectors.forEach(selector => {
            document.querySelectorAll(selector).forEach(el => {
                let li = el.closest('li'); // Buscar el <li> más cercano al <a> con el id
                if (li) li.style.display = 'none'; // Ocultar el <li>
                else el.style.display = 'none'; // Si no es <li>, ocultar directamente el elemento
            });
        });
        
        // Intentamos acceder al contenedor con la clase ""tawk-min-container"" cada 500ms, hasta un máximo de 5 segundos
        let attempts = 0;
        const maxAttempts = 10; // Intentos de 500ms (500ms * 10 = 5 segundos)

        const intervalId = setInterval(() => {
            const tawkContainer = document.querySelector('.tawk-min-container');
            if (tawkContainer) {
                tawkContainer.style.display = 'none'; // Ocultar el elemento estableciendo display: none
                clearInterval(intervalId); // Detener el intervalo una vez que encontramos el elemento
            }
            attempts++;
            if (attempts >= maxAttempts) {
                clearInterval(intervalId); // Detener el intervalo si se alcanzan los intentos máximos
            }
        }, 500); // Cada 500ms
    };

    document.addEventListener('DOMContentLoaded', hideElements);

    const observer = new MutationObserver(hideElements);
    observer.observe(document.body, { childList: true, subtree: true });
})();";

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

}


