using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public partial class AdminGolden : Form
{

    private string userDataFolder;
    private string rutaDatosUsuario;


    private string currentDomain = "";
    private string currentUsername = "";
    private string currentPassword = "";

    public AdminGolden()
    {
        // Verificar si el formulario ya está abierto
        foreach (Form openForm in Application.OpenForms)
        {
            if (openForm is AdminGolden && openForm != this)
            {
                openForm.Close(); // Cerrar la instancia anterior
                break;
            }
        }

        InitializeComponent();
    }

    private async void AdminGolden_Load(object sender, EventArgs e)
    {
        try
        {

            userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "aciertalaconfig"
            );
            rutaDatosUsuario = Path.Combine(userDataFolder, "userdata.json");


            if (!Directory.Exists(userDataFolder))
            {
                Directory.CreateDirectory(userDataFolder);
                Console.WriteLine($"Carpeta creada: {userDataFolder}");
            }


            if (!File.Exists(rutaDatosUsuario))
            {
                var inicial = new JObject
                {
                    ["domain"] = "",
                    ["username"] = "",
                    ["password"] = ""
                };
                File.WriteAllText(rutaDatosUsuario, inicial.ToString());
                Console.WriteLine($"Archivo JSON creado en: {rutaDatosUsuario}");
            }


            var environment = await CoreWebView2Environment.CreateAsync();
            await browser.EnsureCoreWebView2Async(environment);
            browser.CoreWebView2.Settings.IsWebMessageEnabled = true;


            browser.CoreWebView2.Navigate("https://america-admin.virtustec.com/backoffice/login");

            browser.CoreWebView2.NavigationCompleted += async (s, args) =>
            {
                if (args.IsSuccess)
                {
                    string currentUrl = browser.CoreWebView2.Source;
                    Console.WriteLine($"Página cargada: {currentUrl}");


                    if (currentUrl.Contains("/backoffice/login"))
                    {
                        Console.WriteLine("Inyectando scripts de captura y login...");
                        await InjectInputCaptureScript();
                        await InjectLoginClickScript();
                        AutocompletarDatos();
                    }
                    else
                    {

                    }
                }
            };

            browser.CoreWebView2.WebMessageReceived += Browser_WebMessageReceived;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inicializando WebView2: {ex.Message}");
        }
    }


    private async Task InjectInputCaptureScript()
    {

        string script = @"
                (function() {
                    console.log('Iniciando script de captura al escribir...');

                    var domainInput = document.querySelector('input[placeholder=""Domain""]');
                    var usernameInput = document.querySelector('input[placeholder=""Username""]');
                    var passwordInput = document.querySelector('input[placeholder=""Password""]');

                    function sendValuesToCSharp() {
                        const payload = {
                            domain: domainInput ? domainInput.value : '',
                            username: usernameInput ? usernameInput.value : '',
                            password: passwordInput ? passwordInput.value : ''
                        };
                        console.log('Capturando en memoria (al escribir):', payload);
                        window.chrome.webview.postMessage(JSON.stringify({
                            type: 'partial',
                            data: payload
                        }));
                    }

                    if (domainInput) {
                        domainInput.addEventListener('input', sendValuesToCSharp);
                    }
                    if (usernameInput) {
                        usernameInput.addEventListener('input', sendValuesToCSharp);
                    }
                    if (passwordInput) {
                        passwordInput.addEventListener('input', sendValuesToCSharp);
                    }

                    console.log('Eventos de input configurados.');
                })();
            ";

        await browser.CoreWebView2.ExecuteScriptAsync(script);
        Console.WriteLine("Script de captura al escribir inyectado correctamente.");
    }

    private async Task InjectLoginClickScript()
    {

        string script = @"
                (function() {
                    console.log('Iniciando script de login click...');

                    var domainInput = document.querySelector('input[placeholder=""Domain""]');
                    var usernameInput = document.querySelector('input[placeholder=""Username""]');
                    var passwordInput = document.querySelector('input[placeholder=""Password""]');
                    var loginButton = document.querySelector('button.signin-button');

                    function sendLoginToCSharp() {
                        const payload = {
                            domain: domainInput ? domainInput.value : '',
                            username: usernameInput ? usernameInput.value : '',
                            password: passwordInput ? passwordInput.value : ''
                        };
                        console.log('Clic en Login. Enviando credenciales definitivas:', payload);
                        window.chrome.webview.postMessage(JSON.stringify({
                            type: 'login',
                            data: payload
                        }));
                    }

                    if (loginButton) {
                        loginButton.addEventListener('click', function() {
                            console.log('Botón login presionado...');
                            sendLoginToCSharp();
                        });
                    } else {
                        console.warn('No se encontró el botón .signin-button');
                    }

                    console.log('Script de login click configurado.');
                })();
            ";

        await browser.CoreWebView2.ExecuteScriptAsync(script);
        Console.WriteLine("Script de login click inyectado.");
    }


    private void Browser_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            Console.WriteLine("Mensaje recibido de JS.");
            string rawMsg = e.WebMessageAsJson;
            Console.WriteLine($"Contenido crudo: {rawMsg}");

            string decodedJson = JsonConvert.DeserializeObject<string>(rawMsg);


            JObject msgObj = JObject.Parse(decodedJson);
            string msgType = msgObj["type"]?.ToString() ?? "";
            JObject payload = (JObject)msgObj["data"] ?? new JObject();

            string domain = payload["domain"]?.ToString() ?? "";
            string username = payload["username"]?.ToString() ?? "";
            string password = payload["password"]?.ToString() ?? "";


            if (msgType == "partial")
            {
                Console.WriteLine($"(Partial) Actualizando en memoria: domain={domain}, user={username}, pass={password}");
                currentDomain = domain;
                currentUsername = username;
                currentPassword = password;
            }

            else if (msgType == "login")
            {
                Console.WriteLine($"(Login) Sobrescribiendo JSON con domain={domain}, user={username}, pass={password}");
                SobrescribirJSON(domain, username, password);
            }
            else
            {
                Console.WriteLine("Tipo de mensaje desconocido. No se hace nada.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en Browser_WebMessageReceived: {ex.Message}");
        }
    }


    private void SobrescribirJSON(string domain, string username, string password)
    {
        try
        {
            var obj = new JObject
            {
                ["domain"] = domain,
                ["username"] = username,
                ["password"] = password
            };

            File.WriteAllText(rutaDatosUsuario, obj.ToString());
            Console.WriteLine("JSON sobrescrito con:");
            Console.WriteLine(obj);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al sobrescribir JSON: {ex.Message}");
        }
    }


    private void AutocompletarDatos()
    {
        try
        {
            if (File.Exists(rutaDatosUsuario))
            {
                string contenido = File.ReadAllText(rutaDatosUsuario);
                var obj = JObject.Parse(contenido);
                string domain = obj["domain"]?.ToString() ?? "";
                string username = obj["username"]?.ToString() ?? "";
                string password = obj["password"]?.ToString() ?? "";

                string script = $@"
                        (function() {{
                            console.log('Ejecutando autocompletar...');
                            var domainInput = document.querySelector('input[placeholder=""Domain""]');
                            var usernameInput = document.querySelector('input[placeholder=""Username""]');
                            var passwordInput = document.querySelector('input[placeholder=""Password""]');

                            if (domainInput) {{
                                domainInput.value = '{EscapeJsValue(domain)}';
                                domainInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            }}
                            if (usernameInput) {{
                                usernameInput.value = '{EscapeJsValue(username)}';
                                usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            }}
                            if (passwordInput) {{
                                passwordInput.value = '{EscapeJsValue(password)}';
                                passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            }}
                        }})();
                    ";

                browser.CoreWebView2.ExecuteScriptAsync(script);
                Console.WriteLine("Autocompletado con datos del archivo JSON.");
            }
            else
            {
                Console.WriteLine("No existe userdata.json, no se autocompleta.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en AutocompletarDatos: {ex.Message}");
        }
    }


    private string EscapeJsValue(string value)
    {
        return value.Replace("\\", "\\\\")
                    .Replace("'", "\\'")
                    .Replace("\n", "\\n")
                    .Replace("\r", "");
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

    private void AdminGolden_Deactivate(object sender, EventArgs e)
    {
        this.Close(); // Cierra el formulario al quedar en segundo plano
    }

}
