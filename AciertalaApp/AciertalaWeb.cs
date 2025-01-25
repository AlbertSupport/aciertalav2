using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;


    public partial class AciertalaWeb : Form
    {
        private string userDataFolder;
        private string rutaDatosUsuario;
        private string rutaCookies;
        private string rutaLocalStorage;
        private bool isClosing = false;
        private Timer autoSaveTimer;

        public AciertalaWeb()
        {
            InitializeComponent();
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
        }

        private async void AciertalaWeb_Load(object sender, EventArgs e)
        {
            try
            {
                userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "aciertalawebconfig");
                rutaDatosUsuario = Path.Combine(userDataFolder, "userdata.json");
                rutaCookies = Path.Combine(userDataFolder, "cookies.json");
                rutaLocalStorage = Path.Combine(userDataFolder, "localstorage.json");

                Log($"Carpeta de datos configurada en: {userDataFolder}");
                Log($"Ruta del archivo JSON (credenciales): {rutaDatosUsuario}");
                Log($"Ruta de las cookies: {rutaCookies}");
                Log($"Ruta de LocalStorage: {rutaLocalStorage}");

                if (!Directory.Exists(userDataFolder))
                {
                    Directory.CreateDirectory(userDataFolder);
                    Log("Carpeta creada.");
                }

                if (!File.Exists(rutaDatosUsuario))
                {
                    GuardarDatosUsuario(new JObject { ["email"] = "", ["password"] = "" });
                    Log("Archivo JSON inicializado para credenciales.");
                }

                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await browser.EnsureCoreWebView2Async(environment);
                Log("WebView2 inicializado.");

                browser.CoreWebView2.Settings.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";

                await RestoreCookiesAsync();
                await RestoreLocalStorageAsync();

                string url = "https://www.pe.aciertala.com/";
                browser.CoreWebView2.Navigate(url);
                Log($"Navegando a: {url}");

                browser.CoreWebView2.NavigationCompleted += async (s, args) =>
                {
                    if (args.IsSuccess)
                    {
                        Log($"Página cargada exitosamente: {browser.CoreWebView2.Source}");
                        await InjectModalObserverScript();
                        await SaveApplicationState();
                    }
                    else
                    {
                        Log($"Error al cargar la página: {args.WebErrorStatus}");
                    }
                };

                // Configurar el temporizador de autoguardado
                autoSaveTimer = new Timer();
                autoSaveTimer.Interval = 30000; // 30 segundos
                autoSaveTimer.Tick += async (s, args) => await SaveApplicationState();
                autoSaveTimer.Start();

                browser.CoreWebView2.WebMessageReceived += Browser_WebMessageReceived;
            }
            catch (Exception ex)
            {
                Log($"Error en AciertalaWeb_Load: {ex.Message}");
            }
        }

        private async Task InjectModalObserverScript()
        {
            try
            {
                string script = @"
(function() {
    console.log('Iniciando MutationObserver para el modal...');
    let observer = new MutationObserver(function(mutations) {
        let emailInput = document.querySelector('input[name=""username-email""]');
        let passwordInput = document.querySelector('input[name=""password""]');
        let saveButton = document.querySelector('#nvscoreLoginSubmitButton');

        if (emailInput && passwordInput && saveButton) {
            console.log('Formulario del modal encontrado! Configurando...');
            saveButton.addEventListener('click', function() {
                console.log('Botón de guardar clickeado en el modal.');
                const payload = JSON.stringify({
                    email: emailInput.value || '',
                    password: passwordInput.value || ''
                });
                window.chrome.webview.postMessage(payload);
            });

            window.chrome.webview.postMessage(JSON.stringify({ type: 'modalReady' }));
            observer.disconnect();
        }
    });

    observer.observe(document.documentElement, { childList: true, subtree: true });
    console.log('MutationObserver configurado para el modal.');
})();
";
                await browser.CoreWebView2.ExecuteScriptAsync(script);
                Log("Script de MutationObserver para el modal inyectado correctamente.");
            }
            catch (Exception ex)
            {
                Log($"Error inyectando MutationObserver: {ex.Message}");
            }
        }

        private void Browser_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string rawMsg = e.TryGetWebMessageAsString();
                Log($"Mensaje recibido desde el navegador: {rawMsg}");

                if (!string.IsNullOrWhiteSpace(rawMsg))
                {
                    string trimmed = rawMsg.Trim();
                    bool esObjetoJson = trimmed.StartsWith("{") || trimmed.StartsWith("[");
                    if (esObjetoJson)
                    {
                        JObject jData = JObject.Parse(trimmed);
                        var msgType = jData["type"]?.ToString() ?? "";

                        if (msgType == "modalReady")
                        {
                            AutocompletarDatosEnModal();
                        }
                        else
                        {
                            string email = jData["email"]?.ToString() ?? "";
                            string password = jData["password"]?.ToString() ?? "";

                            if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(password))
                            {
                                GuardarDatosUsuario(new JObject
                                {
                                    ["email"] = email,
                                    ["password"] = password
                                });
                            }
                        }
                    }
                    else
                    {
                        Log("El mensaje no es un objeto JSON. Se ignora.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error procesando el mensaje: {ex.Message}");
            }
        }

        private void GuardarDatosUsuario(JObject datosUsuario)
        {
            try
            {
                File.WriteAllText(rutaDatosUsuario, datosUsuario.ToString());
                Log($"Datos guardados: {datosUsuario}");
            }
            catch (Exception ex)
            {
                Log($"Error guardando datos: {ex.Message}");
            }
        }

        private async void AutocompletarDatosEnModal()
        {
            try
            {
                if (File.Exists(rutaDatosUsuario))
                {
                    string contenido = File.ReadAllText(rutaDatosUsuario);
                    JObject datosUsuario = JObject.Parse(contenido);

                    string email = datosUsuario["email"]?.ToString() ?? "";
                    string password = datosUsuario["password"]?.ToString() ?? "";

                    string script = $@"
(function() {{
    var emailInput = document.querySelector('input[name=\""username-email\""]');
    var passwordInput = document.querySelector('input[name=\""password\""]');

    if (emailInput) {{
        emailInput.value = '{EscapeJsValue(email)}';
        emailInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
    }}
    if (passwordInput) {{
        passwordInput.value = '{EscapeJsValue(password)}';
        passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
    }}

    console.log('Autocompletado datos: email={EscapeJsValue(email)}, pass=[oculto]');
}})();
";
                    await browser.CoreWebView2.ExecuteScriptAsync(script);
                    Log("Formulario del modal autocompletado con los datos guardados.");
                }
                else
                {
                    Log("Archivo JSON no encontrado. No hay datos para autocompletar.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error al autocompletar datos: {ex.Message}");
            }
        }

        private string EscapeJsValue(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\n", "\\n")
                .Replace("\r", "");
        }

        private async Task SaveCookiesAsync()
        {
            try
            {
                if (browser?.CoreWebView2 != null)
                {
                    var cookies = await browser.CoreWebView2.CookieManager.GetCookiesAsync(null);
                    JArray cookieArray = new JArray();

                    foreach (var cookie in cookies)
                    {
                        if (!string.IsNullOrEmpty(cookie.Name) && !string.IsNullOrEmpty(cookie.Domain))
                        {
                            cookieArray.Add(new JObject
                            {
                                ["Name"] = cookie.Name,
                                ["Value"] = cookie.Value ?? "",
                                ["Domain"] = cookie.Domain,
                                ["Path"] = cookie.Path ?? "/",
                                ["Expires"] = (cookie.Expires == DateTime.MinValue) ? DateTime.Now.AddYears(1).ToString("o") : cookie.Expires.ToString("o"),
                                ["Secure"] = cookie.IsSecure,
                                ["HttpOnly"] = cookie.IsHttpOnly
                            });
                        }
                    }


                    string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(cookieArray, Newtonsoft.Json.Formatting.Indented);


                    using (FileStream fs = new FileStream(rutaCookies, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        await sw.WriteAsync(jsonContent);
                        await sw.FlushAsync();
                        fs.Flush(true);
                    }

                    Log($"Cookies guardadas exitosamente. Total: {cookieArray.Count}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error guardando cookies: {ex.Message}");
                throw;
            }
        }

        private async Task RestoreCookiesAsync()
        {
            try
            {
                if (File.Exists(rutaCookies))
                {
                    string cookieJson = File.ReadAllText(rutaCookies);
                    JArray cookieArray = JArray.Parse(cookieJson);

                    foreach (JObject cookieData in cookieArray)
                    {
                        var cookie = browser.CoreWebView2.CookieManager.CreateCookie(
                            cookieData["Name"].ToString(),
                            cookieData["Value"].ToString(),
                            cookieData["Domain"].ToString(),
                            cookieData["Path"].ToString()
                        );

                        if (cookieData["Expires"] != null)
                        {
                            cookie.Expires = DateTime.Parse(cookieData["Expires"].ToString());
                        }

                        cookie.IsSecure = (bool)cookieData["Secure"];
                        cookie.IsHttpOnly = (bool)cookieData["HttpOnly"];

                        browser.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
                    }

                    Log("Cookies restauradas exitosamente.");
                }
                else
                {
                    Log("No se encontraron cookies para restaurar.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error restaurando cookies: {ex.Message}");
            }
        }

        private async Task SaveLocalStorageAsync()
        {
            try
            {
                if (browser?.CoreWebView2 == null)
                {
                    Log("CoreWebView2 is not initialized.");
                    return;
                }

                string script = @"
            JSON.stringify(Object.entries(localStorage).reduce((obj, [key, value]) => {
                obj[key] = value;
                return obj;
            }, {}));
        ";
                string localStorageData = await browser.CoreWebView2.ExecuteScriptAsync(script);

                File.WriteAllText(rutaLocalStorage, localStorageData);
                Log("LocalStorage guardado exitosamente.");
            }
            catch (Exception ex)
            {
                Log($"Error guardando LocalStorage: {ex.Message}");
            }
        }


        private async Task RestoreLocalStorageAsync()
        {
            try
            {
                if (File.Exists(rutaLocalStorage))
                {
                    string localStorageData = File.ReadAllText(rutaLocalStorage);

                    string script = $@"
                        const data = {localStorageData};
                        Object.entries(data).forEach(([key, value]) => {{
                            localStorage.setItem(key, value);
                        }});
                    ";
                    await browser.CoreWebView2.ExecuteScriptAsync(script);
                    Log("LocalStorage restaurado exitosamente.");
                }
                else
                {
                    Log("No se encontró LocalStorage para restaurar.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error restaurando LocalStorage: {ex.Message}");
            }
        }

        private async Task SaveApplicationState()
        {
            try
            {
                await SaveCookiesAsync();
                await SaveLocalStorageAsync();
                Log("Estado de la aplicación guardado automáticamente");
            }
            catch (Exception ex)
            {
                Log($"Error guardando el estado de la aplicación: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            try
            {
                string logPath = Path.Combine(userDataFolder, "debug.log");
                File.AppendAllText(logPath, DateTime.Now + " - " + message + Environment.NewLine);
            }
            catch
            {

            }

            Console.WriteLine(message);
        }

        private async void OnApplicationExit(object sender, EventArgs e)
        {
            if (!isClosing)
            {
                isClosing = true;
                await SaveApplicationState();
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

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            if (!isClosing)
            {
                isClosing = true;
                e.Cancel = true;

                try
                {
                    await SaveApplicationState();
                }
                catch (Exception ex)
                {
                    Log($"Error durante el guardado final: {ex.Message}");
                }
                finally
                {
                    isClosing = false;
                    Application.Exit();
                }
            }
        }
    }