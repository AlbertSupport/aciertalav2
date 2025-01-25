using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using ZXing;
using AciertalaApp;


public partial class TerminalLogin : Form
{
    private string ticketContent;
    private Timer keyListenerTimer = new Timer();

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    private const int VK_F2 = 0x71;

    public TerminalLogin()
    {
        InitializeComponent();
    }

    private async void TerminalLogin_Load(object sender, EventArgs e)
    {
        try
        {
            string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "aciertalaapp");
            var envOptions = new CoreWebView2EnvironmentOptions();
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, envOptions);

            await browser.EnsureCoreWebView2Async(environment);

            browser.CoreWebView2.Settings.AreDevToolsEnabled = true;

            browser.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            browser.CoreWebView2.WebMessageReceived += OnWebMessage;
            browser.CoreWebView2.WebMessageReceived += OnWebMessageSave2;
            browser.CoreWebView2.NewWindowRequested += HandleNewWindowRequested;

            await InjectInterceptionScriptAsync();
            await InjectInterceptionScriptForSave2Async();

            string url = "https://www.pe.aciertala.com/sport?sportId=1";
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                browser.CoreWebView2.Navigate(url);
            }
            else
            {
                MessageBox.Show("URL no válida o vacía.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            keyListenerTimer.Interval = 100;
            keyListenerTimer.Tick += KeyListenerTimer_Tick;
            keyListenerTimer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al inicializar WebView2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task InjectInterceptionScriptAsync()
    {
        string interceptionScript = @"
            (function() {
                const originalFetch = window.fetch;
                const originalXHR = window.XMLHttpRequest;

                window.fetch = async function(...args) {
                    const response = await originalFetch.apply(this, args);
                    const clonedResponse = response.clone();
                    const requestUrl = args[0];
                    const requestInit = args[1] || {};
                    const requestBody = requestInit.body || null;

                    if (requestUrl.includes('/set-print') && requestInit.method === 'POST') {
                        clonedResponse.text().then(body => {
                            window.chrome.webview.postMessage({
                                type: 'fetch-intercept',
                                url: requestUrl,
                                method: requestInit.method,
                                requestBody: requestBody,
                                responseBody: body
                            });
                        });
                    }
                    return response;
                };

                const xhrOpen = originalXHR.prototype.open;
                const xhrSend = originalXHR.prototype.send;

                originalXHR.prototype.open = function(method, url, ...rest) {
                    this._url = url;
                    this._method = method;
                    return xhrOpen.apply(this, [method, url, ...rest]);
                };

                originalXHR.prototype.send = function(body) {
                    this.addEventListener('load', function() {
                        if (this._url.includes('/set-print') && this._method === 'POST') {
                            window.chrome.webview.postMessage({
                                type: 'xhr-intercept',
                                url: this._url,
                                method: this._method,
                                requestBody: body,
                                responseBody: this.responseText
                            });
                        }
                    });
                    return xhrSend.apply(this, [body]);
                };

                console.log('Interceptores de fetch y XHR inyectados');
            })();";
        await browser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(interceptionScript);
        Debug.WriteLine("[InjectInterceptionScriptAsync] Script inyectado correctamente.");
    }

    private async Task InjectInterceptionScriptForSave2Async()
    {
        string interceptionScript = @"
            (function() {
                const originalXHR = window.XMLHttpRequest;
                const xhrOpen = originalXHR.prototype.open;
                const xhrSend = originalXHR.prototype.send;

                originalXHR.prototype.open = function(method, url) {
                    this._url = url;
                    this._method = method;
                    return xhrOpen.apply(this, arguments);
                };

                originalXHR.prototype.send = function(body) {
                    this.addEventListener('load', function() {
                        if (this._url.includes('/ticket/save2') && this._method.toUpperCase() === 'POST') {
                            window.chrome.webview.postMessage({
                                type: 'xhr-save2',
                                url: this._url,
                                method: this._method,
                                requestBody: body,
                                responseBody: this.responseText
                            });
                        }
                    });
                    return xhrSend.apply(this, arguments);
                };
            })();";
        await browser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(interceptionScript);
        Debug.WriteLine("[InjectInterceptionScriptForSave2Async] Script inyectado para interceptar /ticket/save2.");
    }

    private void HandleNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        Debug.WriteLine($"Ventana emergente bloqueada: {e.Uri}");
    }

    private async void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            string removeScript = @"
                (function() {
                    function removeElements() {
                        const elements = document.querySelectorAll(
                            'nvscore-dynamic-element.ng-tns-c151-4.ng-tns-c435-2.visibility-true.nvs-menu-item.ng-star-inserted, ' +
                            'li.ng-star-inserted a#Casino, li.ng-star-inserted a#Live\\ Casino, ' +
                            'li.ng-star-inserted a#Habilidad, li.ng-star-inserted a#lotto, ' +
                            'nvscore-carousel.ng-star-inserted, div.nvscore-carousel.multiple, ' +
                            'div.nvscore-carousel.full-width, div#footer, div.tawk-min-container, ' +
                            'iframe[src=""about:blank""][style*=""position:fixed""]'
                        );
                        elements.forEach(el => el.remove());
                    }

                    removeElements();
                    let count = 0;
                    const interval = setInterval(() => {
                        removeElements();
                        count++;
                        if (count >= 20) clearInterval(interval);
                    }, 500);
                })();";
            await browser.CoreWebView2.ExecuteScriptAsync(removeScript);
        }
        else
        {
            Debug.WriteLine("La navegación falló o fue cancelada.");
        }
    }

    private void KeyListenerTimer_Tick(object sender, EventArgs e)
    {
        if (GetAsyncKeyState(VK_F2) != 0)
        {
            OpenSettingsForm();
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

    private void OpenSettingsForm()
    {
        var settingsForm = Application.OpenForms["FormPrintSettings"];
        if (settingsForm == null)
        {
            settingsForm = new FormPrintSettings();
            settingsForm.Show();
        }
        else
        {
            settingsForm.BringToFront();
            settingsForm.Activate();
        }
    }

    private void OnWebMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var rawJson = e.WebMessageAsJson;
            var msgObj = JsonNode.Parse(rawJson);
            var msgType = msgObj?["type"]?.ToString();

            switch (msgType)
            {
                case "fetch-intercept":
                    ProcessTicketResponse(msgObj?["responseBody"]?.ToString());
                    break;
                case "xhr-intercept":
                    ProcessTicketResponse(msgObj?["responseBody"]?.ToString());
                    break;
                default:
                    Debug.WriteLine($"[JS MSG] Tipo desconocido: {rawJson}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[OnWebMessage ERROR] {ex.Message}");
        }
    }

    private void OnWebMessageSave2(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var rawJson = e.WebMessageAsJson;
            var msgObj = JsonNode.Parse(rawJson);
            var msgType = msgObj?["type"]?.ToString();

            if (msgType == "xhr-save2")
            {
                var responseBody = msgObj?["responseBody"]?.ToString();
                ProcessTicketSave2WithDesign(responseBody);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[OnWebMessageSave2 ERROR] {ex.Message}");
        }
    }

    private void ProcessTicketResponse(string responseBody)
    {
        try
        {
            Debug.WriteLine("Response Body (set-print):");
            Debug.WriteLine(responseBody);

            var parsedResponse = JsonNode.Parse(responseBody);
            var ticketData = parsedResponse?["data"]?["ticket"];
            if (ticketData == null)
            {
                Debug.WriteLine("[ProcessTicketResponse] Datos del ticket no disponibles.");
                return;
            }

            PrintDocument printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = "TuNombreDeImpresora"; // Cambiar por tu impresora
            if (!printDoc.PrinterSettings.IsValid)
            {
                MessageBox.Show($"La impresora '{printDoc.PrinterSettings.PrinterName}' no es válida o no está instalada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            PaperSize customPaperSize = new PaperSize("Custom", 300, 1000); // Ajusta el tamaño según tus necesidades
            printDoc.DefaultPageSettings.PaperSize = customPaperSize;

            printDoc.PrintPage += (sender, e) =>
            {
                Graphics g = e.Graphics;
                g.Clear(Color.White);

                int marginLeft = 10;
                int yPos = 10;

                // Imprimir título del ticket
                Font fontTitle = new Font("Arial", 14, FontStyle.Bold);
                string ticketTitle = "VOUCHER DE APUESTA";
                SizeF titleSize = g.MeasureString(ticketTitle, fontTitle);
                g.DrawString(ticketTitle, fontTitle, Brushes.Black, (e.PageBounds.Width - titleSize.Width) / 2, yPos);
                yPos += (int)titleSize.Height + 10;

                // Imprimir información del ticket
                Font fontInfo = new Font("Arial", 10, FontStyle.Regular);
                string ticketInfo = $"Cupo: {ticketData["code"]}";
                g.DrawString(ticketInfo, fontInfo, Brushes.Black, marginLeft, yPos);
                yPos += (int)g.MeasureString(ticketInfo, fontInfo).Height + 10;

                // Imprimir detalles del ticket
                var items = ticketData["items"];
                if (items != null)
                {
                    foreach (var bet in items.AsArray())
                    {
                        string betInfo = $"{bet["event_name"]} - {bet["market_name"]}: {bet["odds_name"]} ({bet["odds_value"]})";
                        g.DrawString(betInfo, fontInfo, Brushes.Black, marginLeft, yPos);
                        yPos += (int)g.MeasureString(betInfo, fontInfo).Height + 5;
                    }
                }

                // Imprimir pie de página
                string footerText = "Gracias por su preferencia.";
                Font fontFooter = new Font("Arial", 8, FontStyle.Italic);
                SizeF footerSize = g.MeasureString(footerText, fontFooter);
                g.DrawString(footerText, fontFooter, Brushes.Black, (e.PageBounds.Width - footerSize.Width) / 2, yPos);
            };

            printDoc.Print();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProcessTicketResponse ERROR] {ex.Message}");
        }
    }


    private void ProcessTicketSave2WithDesign(string responseBody)
    {
        try
        {
            var parsed = JsonNode.Parse(responseBody);
            var data = parsed?["data"];
            if (data == null)
            {
                Debug.WriteLine("[ProcessTicketSave2WithDesign] No se encontraron datos en la respuesta.");
                return;
            }

            string voucherCode = data["code"]?.ToString();
            if (string.IsNullOrEmpty(voucherCode))
            {
                Debug.WriteLine("[ProcessTicketSave2WithDesign] El código del voucher no está presente.");
                return;
            }

            string currentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            PrintDocument printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = "TuNombreDeImpresora"; // Cambiar por tu impresora
            if (!printDoc.PrinterSettings.IsValid)
            {
                MessageBox.Show($"La impresora '{printDoc.PrinterSettings.PrinterName}' no es válida o no está instalada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            PaperSize customPaperSize = new PaperSize("Custom", 300, 1000);
            printDoc.DefaultPageSettings.PaperSize = customPaperSize;

            printDoc.PrintPage += (sender, e) =>
            {
                Graphics g = e.Graphics;
                g.Clear(Color.White);

                int marginLeft = 10;
                int yPos = 10;
                int ticketWidth = e.PageBounds.Width - (marginLeft * 2);

                // Imprimir título
                Font fontTitle = new Font("Arial", 14, FontStyle.Bold);
                string title = "VOUCHER DE APUESTA";
                SizeF titleSize = g.MeasureString(title, fontTitle);
                g.DrawString(title, fontTitle, Brushes.Black, (ticketWidth - titleSize.Width) / 2, yPos);
                yPos += (int)titleSize.Height + 10;

                // Imprimir fecha
                Font fontDate = new Font("Arial", 10, FontStyle.Regular);
                string dateText = $"Fecha: {currentDate}";
                g.DrawString(dateText, fontDate, Brushes.Black, marginLeft, yPos);
                yPos += (int)g.MeasureString(dateText, fontDate).Height + 10;

                // Imprimir código de voucher
                Font fontVoucher = new Font("Arial", 12, FontStyle.Bold);
                string voucherText = $"Código: {voucherCode}";
                g.DrawString(voucherText, fontVoucher, Brushes.Black, marginLeft, yPos);
                yPos += (int)g.MeasureString(voucherText, fontVoucher).Height + 10;

                // Generar código de barras
                BarcodeWriter writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 50,
                        Width = ticketWidth,
                        Margin = 0
                    }
                };
                var barcodeBitmap = writer.Write(voucherCode);
                g.DrawImage(barcodeBitmap, new Point(marginLeft, yPos));
                yPos += barcodeBitmap.Height + 10;

                // Imprimir pie
                string footerText = "Este voucher es válido únicamente si es autorizado por caja.";
                Font fontFooter = new Font("Arial", 8, FontStyle.Italic);
                SizeF footerSize = g.MeasureString(footerText, fontFooter);
                g.DrawString(footerText, fontFooter, Brushes.Black, (ticketWidth - footerSize.Width) / 2, yPos);
            };

            printDoc.Print();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProcessTicketSave2WithDesign ERROR] {ex.Message}");
        }
    }

}
