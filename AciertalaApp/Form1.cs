using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Diagnostics;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using AciertalaApp;




namespace TerminalV2
{
    public class Form1 : Form
    {
        private static WebViewForm webViewForm = null;
        private const int WM_HOTKEY = 0x0312;

        // Definimos las constantes de los eventos del mouse
        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;

        private string registroURL = "https://www.pe.aciertala.com/register"; // URL predeterminada
        private LocalStorage localStorage; // Instancia de LocalStorage
        private string configFilePath = "config.txt"; // Ruta al archivo de configuración
        private CheckBox chkCaballosEnabled;
        private CheckBox chkDisableCaballos;
        private bool isInitializeComponents3 = false; // Variable de control
        private Panel buttonsPanel;  // Panel donde se van a agregar los botones
        private Button homeButton;   // El botón "Home" que muestra los botones


        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public Form1()
        {
            // Verificar si hay actualizaciones disponibles antes de continuar
            //AppUpdater.CheckForUpdatesAsync().Wait(); // Esperar a que se complete la verificación de la actualización
            // Otras inicializaciones
            RegisterHotKey(this.Handle, 2, 0, (uint)Keys.F5); // Registrar la tecla F5

            // Crear instancia de LocalStorage
            localStorage = new LocalStorage();

            // Intentar cargar la URL y la configuración desde el almacenamiento local
            LoadRegistroURLFromLocalStorage();

            // Si no hay URL configurada, mostrar el formulario de entrada
            if (string.IsNullOrEmpty(registroURL))
            {
                PromptForURL();
            }
            else
            {
                // Ya está configurado, no mostrar el formulario y continuar con la inicialización
                this.Show();
            }
        }



        private void InstallAciertala(string downloadUrl, string zipPath, string extractedPath)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(downloadUrl, zipPath);
                }

                if (Directory.Exists(extractedPath))
                {
                    Directory.Delete(extractedPath, true);
                }
                ZipFile.ExtractToDirectory(zipPath, extractedPath);

                string installerPath = Directory.GetFiles(extractedPath, "*.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrEmpty(installerPath))
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = installerPath,
                            UseShellExecute = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                }
                else
                {
                    MessageBox.Show("No se encontró un instalador válido en el archivo descargado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error durante la instalación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Método para cargar la URL y la configuración desde el almacenamiento local
        private void LoadRegistroURLFromLocalStorage()
        {
            var data = localStorage.Load();

            if (data.ContainsKey("registroURL"))
            {
                // Decodificar la URL correctamente
                registroURL = Uri.UnescapeDataString(data["registroURL"]);

                bool caballosEnabled = false;
                bool disableCaballos = false;

                // Cargar el estado de los CheckBoxes desde LocalStorage
                if (data.ContainsKey("caballosEnabled"))
                {
                    caballosEnabled = bool.Parse(data["caballosEnabled"]);
                }

                if (data.ContainsKey("disableCaballos"))
                {
                    disableCaballos = bool.Parse(data["disableCaballos"]);
                }

                // Ejecutar la inicialización según los valores de caballosEnabled y disableCaballos
                if (disableCaballos)
                {
                    // Ejecutar la aplicación VBOX
                    string vboxAppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "VBOX.appref-ms");
                    if (File.Exists(vboxAppPath))
                    {
                        Process.Start(vboxAppPath); // Ejecutar la aplicación
                    }
                    else
                    {
                        MessageBox.Show("No se encontró la aplicación VBOX.appref-ms en el escritorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    InitializeComponents3();  // Modo Cajero
                }
                else if (caballosEnabled)
                {
                    string downloadUrl = "https://releases.xpressgaming.net/tech.xpress.aciertala/win32/Aciertala-setup-2.7.2.zip";
                    string localPath = @"C:\Aciertala";
                    string zipPath = Path.Combine(localPath, "Aciertala-setup.zip");
                    string extractedPath = Path.Combine(localPath, "extracted");

                    if (!Directory.Exists(localPath))
                    {
                        Directory.CreateDirectory(localPath);
                    }

                    InstallAciertala(downloadUrl, zipPath, extractedPath);
                    InitializeComponents();  // Caballos habilitado

                    RegisterHotKey(this.Handle, 0, 0, (uint)Keys.F11); // F11
                    RegisterHotKey(this.Handle, 1, 0, (uint)Keys.F6);  // F6
                }
                else
                {
                    InitializeComponents2();  // Flujo por defecto
                }

                // No mostrar la ventana de configuración URL, ya que ya está configurada
                this.Show();
            }
            else
            {
                // Si no se encuentra la URL, solo entonces mostrar el formulario de configuración
                PromptForURL();
            }
        }


        private void SaveRegistroURLToLocalStorage(bool caballosEnabled, bool disableCaballos)
        {
            var data = new Dictionary<string, string>
    {
        { "registroURL", Uri.EscapeDataString(registroURL) },  // Codificamos la URL
        { "caballosEnabled", caballosEnabled.ToString() },
        { "disableCaballos", disableCaballos.ToString() } // Guardamos el estado de chkDisableCaballos
    };

            localStorage.Save(data);
        }


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id == 0) // F11
                {
                    HandleHotkey("https://universalrace.net/download/BotonesAciertala.rar",
                                 @"C:\BotonesAciertala",
                                 @"C:\BotonesAciertala\BotonF11.exe");
                }
                else if (id == 1) // F6
                {
                    HandleHotkey("https://universalrace.net/download/BotonesAciertala.rar",
                                 @"C:\BotonesAciertala",
                                 @"C:\BotonesAciertala\BotonF6.exe");
                }
                else if (id == 2) // F5
                {
                    RestartAciertala();
                }
            }
            base.WndProc(ref m);
        }


        private void RestartAciertala()
        {
            try
            {
                // Cerrar todos los procesos con nombre "Aciertala"
                foreach (var process in Process.GetProcessesByName("Aciertala"))
                {
                    try
                    {
                        process.Kill(); // Finaliza el proceso
                        process.WaitForExit(); // Espera a que finalice completamente
                    }
                    catch (InvalidOperationException)
                    {
                        // Ocurre si el proceso ya terminó antes de llamar a Kill()
                        continue;
                    }
                }

                // Esperar 1 segundos antes de reabrir la aplicación
                Task.Delay(1000).Wait();

                // Ruta al acceso directo en el escritorio
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string aciertalaPath = Path.Combine(desktopPath, "Aciertala.lnk");

                if (File.Exists(aciertalaPath))
                {
                    // Abrir la aplicación desde el acceso directo
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = aciertalaPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("No se encontró el acceso directo de 'Aciertala' en el escritorio.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al reiniciar Aciertala: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void HandleHotkey(string downloadUrl, string extractPath, string executablePath)
        {
            string rarPath = Path.Combine(extractPath, "BotonesAciertala.rar");

            try
            {
                // Crear directorio si no existe
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                // Descargar el archivo si no existe
                if (!File.Exists(rarPath))
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(downloadUrl, rarPath);
                        MessageBox.Show("Archivo descargado correctamente.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // Descomprimir el archivo si no se ha hecho ya
                if (!Directory.GetFiles(extractPath, "*.exe", SearchOption.TopDirectoryOnly).Any())
                {
                    ExtractRar(rarPath, extractPath);
                    MessageBox.Show("Archivo descomprimido correctamente.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Ejecutar el archivo especificado
                if (File.Exists(executablePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = executablePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show($"El archivo {Path.GetFileName(executablePath)} no se encontró tras la extracción.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al manejar la tecla rápida: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExtractRar(string rarPath, string extractPath)
        {
            try
            {
                using (var archive = ArchiveFactory.Open(rarPath))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(extractPath, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al descomprimir el archivo .rar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Verificar si el CheckBox existe antes de acceder a él
            if (chkCaballosEnabled != null && chkDisableCaballos != null)
            {
                // Obtener el estado de los CheckBox
                bool caballosEnabled = chkCaballosEnabled.Checked;
                bool disableCaballos = chkDisableCaballos.Checked;

                // Guardar los estados en LocalStorage
                SaveRegistroURLToLocalStorage(caballosEnabled, disableCaballos);
            }

            UnregisterHotKey(this.Handle, 0);
            UnregisterHotKey(this.Handle, 1);

            base.OnFormClosing(e);
        }


        private void InitializeComponents()
        {
            // Limpiar los controles existentes en el formulario
            this.Controls.Clear();

            // Obtener el tamaño de la pantalla
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // Configuración del formulario
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            this.Size = new Size(screenWidth, 80); // Usar todo el ancho de la pantalla

            this.BackColor = Color.Lime;
            this.TransparencyKey = Color.Lime;

            // Variable para la configuración de botones
            ButtonConfig[] buttonConfigs = null;

            // Definir las ubicaciones iniciales de los botones
            int startX = 5;
            int startY = 400;

            // Configuración según resolución
            if (screenWidth == 1920 && screenHeight == 1080) // Full HD
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 130, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 130, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 130, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 130, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 130, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 130, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 130, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("CHROME", "https://www.google.com/", 130, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("REGISTRO", "https://www.registro.com/", 130, 50, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                    new ButtonConfig("ACTUALIZAR", null, 130, 50, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 130, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                };


                // Cambiar la ubicación inicial para los botones en Full HD
                startX = 1710; // Modificado para Full HD
                startY = 80; // Modificado para Full HD

                if (buttonConfigs == null || buttonConfigs.Length == 0)
                {
                    throw new ArgumentNullException(nameof(buttonConfigs), "La colección de configuraciones de botones no puede ser nula o vacía.");
                }

                // Crear el botón "Home" para la resolución 1920x1080
                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 130,
                    Height = 80,
                    Left = 1710,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 18, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 40, 40),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(0, 0, 0, 0)
                };
            }
            else if (screenWidth == 1680 && screenHeight == 1050) // 1680x1050
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 275, 50, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 275, 50, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                // Cambiar la ubicación inicial para los botones en Full HD
                startX = 1320; // Modificado para Full HD
                startY = 80; // Modificado para Full HD



                // Crear el botón "Home" para la resolución 1920x1080
                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 275,
                    Height = 80,
                    Left = 1320,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(0, 0, 0, 0)
                };
            }
            else if (screenWidth == 1600 && screenHeight == 900) // 1600x900
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 275, 50, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 275, 50, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 275, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1240; 
                startY = 80; 

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 275,
                    Height = 80,
                    Left = 1240,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(60, 0, 60, 0)
                };
            }
            else if (screenWidth == 1440 && screenHeight == 900) // 1440x900
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 200, 50, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 200, 50, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1160;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 200,
                    Height = 80,
                    Left = 1160,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1400 && screenHeight == 1050)
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 200, 50, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 200, 50, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 200, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1120;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 200,
                    Height = 80,
                    Left = 1120,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1366 && screenHeight == 768)
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 200, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 200, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };


                startX = 1090;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 200,
                    Height = 80,
                    Left = 1090,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1360 && screenHeight == 768)
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 200, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 200, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1080;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 200,
                    Height = 80,
                    Left = 1080,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 1024) 
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 200, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 200, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1000;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 200,
                    Height = 80,
                    Left = 1000,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 960)
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 200, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 200, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1000;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 200,
                    Height = 80,
                    Left = 1000,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 800)
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 200, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 200, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1000;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 200,
                    Height = 80,
                    Left = 1000,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 768)
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 200, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 200, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1000;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 200,
                    Height = 80,
                    Left = 1000,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 720) 
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 200, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 200, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 200, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1000;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 200,
                    Height = 80,
                    Left = 1000,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1152 && screenHeight == 864)
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 240, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 240, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 850;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 240,
                    Height = 80,
                    Left = 850,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(40, 0, 40, 0)
                };
            }
            else if (screenWidth == 1024 && screenHeight == 768)
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 240, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 240, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 240, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 720;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 240,
                    Height = 80,
                    Left = 720,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(40, 0, 40, 0)
                };
            }
            else if (screenWidth == 800 && screenHeight == 600)
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 150, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 150, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("MARCADORES EN VIVO", "https://statshub.sportradar.com/novusoft/es/sport/1", 150, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 150, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 150, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 1", "https://365livesport.org/", 150, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("TRANSMISION 2", "https://playviper.com/", 150, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("CHROME", "https://www.google.com/", 150, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 150, 40, ColorTranslator.FromHtml("#1A24B1"), false, 0),
                new ButtonConfig("ACTUALIZAR", null, 150, 40, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 150, 40, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 590;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 150,
                    Height = 80,
                    Left = 590,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(20, 0, 20, 0)
                };
            }



            // Añadir el evento de clic del botón "Home"
            homeButton.Click += HomeButton_Click;

            // Crear el panel que contendrá los botones
            buttonsPanel = new Panel()
            {
                Width = screenWidth,  // Ajustamos al ancho de la pantalla
                Height = screenHeight - 100,  // Ajustamos al alto de la pantalla (dejando espacio para el botón Home)
                Left = 0,
                Top = 0,
                Visible = false,  // Inicialmente el panel estará oculto
            };

            // Agregar el botón "Home" y el panel a los controles del formulario
            this.Controls.Add(homeButton);
            this.Controls.Add(buttonsPanel);

            // Llamada a CreateButtons con las ubicaciones iniciales de los botones según la resolución
            CreateButtons(buttonConfigs, startX, startY); // startX y startY dependientes de la resolución
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            // Cuando se haga clic en "Home", mostramos/ocultamos el panel con los botones
            buttonsPanel.Visible = !buttonsPanel.Visible;
        }


        private void InitializeComponents2()
        {
            // Limpiar los controles existentes en el formulario
            this.Controls.Clear();

            // Obtener el tamaño de la pantalla
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // Configuración del formulario
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            this.Size = new Size(screenWidth, 80); // Usar todo el ancho de la pantalla

            this.BackColor = Color.Lime;
            this.TransparencyKey = Color.Lime;

            // Variable para la configuración de botones
            ButtonConfig[] buttonConfigs = null;

            // Configuración según resolución
            if (screenWidth == 1920 && screenHeight == 1080) // Full HD
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 115, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 115, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 115, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("TRANSMISION", "https://365livesport.org/", 115, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("CHROME", "https://www.google.com/", 115, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 115, 50, ColorTranslator.FromHtml("#1A24B1"), false, 5),
                };
            }
            else if (screenWidth == 1680 && screenHeight == 1050) // 1680x1050
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 120, 55, Color.Red, true, 15),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 120, 55, Color.Green, false, 12),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 120, 55, Color.Blue, true, 20),
                new ButtonConfig("TRANSMISION", "https://365livesport.org/", 120, 55, Color.Purple, false, 25),
                new ButtonConfig("CHROME", "https://www.google.com/", 120, 55, Color.Yellow, false, 5),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 140, 65, Color.Orange, false, 10)
                };
            }
            else if (screenWidth == 1600 && screenHeight == 900) // 1600x900
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 85, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 85, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 85, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("TRANSMISION", "https://365livesport.org/", 90, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("CHROME", "https://www.google.com/", 85, 50, ColorTranslator.FromHtml("#313439"), false, 5),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 85, 50, ColorTranslator.FromHtml("#1A24B1"), false, 5),
                };
            }
            else if (screenWidth == 1440 && screenHeight == 900) // 1440x900
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 100, 50, Color.Red, true, 12),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 100, 50, Color.Green, false, 8),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 100, 50, Color.Blue, true, 10),
                new ButtonConfig("TRANSMISION", "https://365livesport.org/", 100, 50, Color.Purple, false, 12),
                new ButtonConfig("CHROME", "https://www.google.com/", 100, 50, Color.Yellow, false, 5),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 110, 55, Color.Orange, false, 8)
                };
            }
            else if (screenWidth == 1366 && screenHeight == 768) // 1366x768
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 90, 45, Color.Red, true, 10),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 90, 45, Color.Green, false, 8),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 90, 45, Color.Blue, true, 12),
                new ButtonConfig("TRANSMISION", "https://365livesport.org/", 90, 45, Color.Purple, false, 15),
                new ButtonConfig("CHROME", "https://www.google.com/", 90, 45, Color.Yellow, false, 5),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 100, 50, Color.Orange, false, 8)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 720) // 1280x720
            {
                buttonConfigs = new ButtonConfig[]
                {
                new ButtonConfig("RESULTADO EN VIVO", "https://statsinfo.co/live/1/", 80, 40, Color.Red, true, 8),
                new ButtonConfig("ESTADISTICA", "https://statsinfo.co/stats/1/c/26/", 80, 40, Color.Green, false, 5),
                new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 80, 40, Color.Blue, true, 10),
                new ButtonConfig("TRANSMISION", "https://365livesport.org/", 80, 40, Color.Purple, false, 12),
                new ButtonConfig("CHROME", "https://www.google.com/", 80, 40, Color.Yellow, false, 3),
                new ButtonConfig("REGISTRO", "https://www.registro.com/", 90, 45, Color.Orange, false, 5)
                };
            }

            // Llamada a CreateButtons con una ubicación inicial diferente
            CreateButtons(buttonConfigs, 100, 50); // startX = 100, startY = 50 para este caso
        }

        private void InitializeComponents3()
        {
            // Limpiar los controles existentes en el formulario
            this.Controls.Clear();

            // Establecer la variable de control antes de crear los botones
            isInitializeComponents3 = true;

            // Obtener el tamaño de la pantalla
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // Configuración del formulario
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            this.Size = new Size(screenWidth, 80); // Usar todo el ancho de la pantalla

            this.BackColor = Color.Lime;
            this.TransparencyKey = Color.Lime;

            // Variable para la configuración de botones
            ButtonConfig[] buttonConfigs = null;
            Button homeButton = null;
            int startX = 5;
            int startY = 200; // Ubicación inicial predeterminada para los botones

            // Configuración según resolución
            if (screenWidth == 1920 && screenHeight == 1080) // Full HD
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 160, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 160, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 160, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 160, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("CHROME", "https://www.google.com/", 160, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ACTUALIZAR", null, 160, 50, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 160, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                // Cambiar la ubicación inicial para los botones en Full HD
                startX = 1480; // Modificado para Full HD
                startY = 80; // Modificado para Full HD


                // Crear el botón "Home" para la resolución 1920x1080
                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 160,
                    Height = 80,
                    Left = 1480,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 18, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 40, 40),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(15, 0, 15, 0)
                };
            }
            else if (screenWidth == 1680 && screenHeight == 1050) 
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 250, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 250, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 250, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 250, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("CHROME", "https://www.google.com/", 250, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ACTUALIZAR", null, 250, 50, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 250, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                // Cambiar la ubicación inicial para los botones en Full HD
                startX = 1155; // Modificado para Full HD
                startY = 80; // Modificado para Full HD



                // Crear el botón "Home" para la resolución 1920x1080
                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 250,
                    Height = 80,
                    Left = 1155,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(50, 0, 50, 0)
                };
            }
            else if (screenWidth == 1600 && screenHeight == 900) // 1600x900
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 235, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 235, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 235, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 235, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("CHROME", "https://www.google.com/", 235, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ACTUALIZAR", null, 235, 50, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 250, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1085;
                startY = 80;

                homeButton = new Button()
                {
                    Text = "Home",
                    Width = 235,
                    Height = 80,
                    Left = 1085,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(60, 0, 60, 0)
                };
            }
            else if (screenWidth == 1440 && screenHeight == 900) // 1440x900
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 120, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 120, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 120, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 120, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("CHROME", "https://www.google.com/", 120, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                    new ButtonConfig("ACTUALIZAR", null, 235, 50, ColorTranslator.FromHtml("#313439"), false, 0, null,(sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 250, 50, ColorTranslator.FromHtml("#313439"), false, 0),
                };

                startX = 1045;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 120,
                    Height = 80,
                    Left = 1045,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 12, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 35, 0)
                };
            }
            else if (screenWidth == 1400 && screenHeight == 1050) 
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 1010;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 110,
                    Height = 80,
                    Left = 1010,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1366 && screenHeight == 768)
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };


                startX = 975;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 110,
                    Height = 80,
                    Left = 975,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1360 && screenHeight == 768) 
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 970;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 110,
                    Height = 80,
                    Left = 970,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 1024)
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 890;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 110,
                    Height = 80,
                    Left = 890,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 960)
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 890;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 110,
                    Height = 80,
                    Left = 890,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 800)
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 895;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 100,
                    Height = 80,
                    Left = 895,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 25, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 768)
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 100, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 100, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 100, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 895;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 100,
                    Height = 80,
                    Left = 895,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 25, 0)
                };
            }
            else if (screenWidth == 1280 && screenHeight == 720)
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 890;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 110,
                    Height = 80,
                    Left = 890,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(30, 0, 30, 0)
                };
            }
            else if (screenWidth == 1152 && screenHeight == 864)
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 860;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 110,
                    Height = 80,
                    Left = 860,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(40, 0, 30, 0)
                };
            }
            else if (screenWidth == 1024 && screenHeight == 768)
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 730;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 110,
                    Height = 80,
                    Left = 730,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(40, 0, 30, 0)
                };
            }
            else if (screenWidth == 800 && screenHeight == 600)
            {
                buttonConfigs = new ButtonConfig[]
                {
                    new ButtonConfig("CABALLOS", "https://retailhorse.aciertala.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CABALLOS.ico"),
                    new ButtonConfig("ADMIN GOLDEN", "https://america-admin.virtustec.com/backoffice/login", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ADMIN_GOLDEN.ico"),
                    new ButtonConfig("SHEETS", "https://docs.google.com/spreadsheets/u/0/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.SHEETS.ico"),
                    new ButtonConfig("ACIERTALA WEB", "https://www.pe.aciertala.com/sport", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.ACIERTALA WEB.ico"),
                    new ButtonConfig("CHROME", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), true, 0, "AciertalaApp.CHROME.ico"),
                    new ButtonConfig("ACTUALIZAR", null, 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.ACTUALIZAR.ico", (sender, args) => RestartAciertala()),
                    new ButtonConfig("CONEXION REMOTA", "https://www.google.com/", 110, 50, ColorTranslator.FromHtml("#313439"), false, 0, "AciertalaApp.CONEXION_REMOTA.ico"),
                };

                startX = 675;
                startY = 80;

                homeButton = new Button()
                {
                    //Text = "Home",
                    Width = 110,
                    Height = 80,
                    Left = 675,
                    Top = 0,
                    BackColor = ColorTranslator.FromHtml("#1A24B1"),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 15, FontStyle.Regular),

                    // Cargar el ícono usando la función LoadEmbeddedImage
                    Image = LoadEmbeddedImage("AciertalaApp.mas.ico", 30, 30),  // Especifica el recurso y el tamaño deseado del ícono
                    // Alinear la imagen al lado derecho del texto
                    ImageAlign = ContentAlignment.MiddleRight,
                    // Alinear el texto a la izquierda
                    TextAlign = ContentAlignment.MiddleLeft,
                    // Ajustar el espacio entre el texto y la imagen
                    Padding = new Padding(20, 0, 30, 0)
                };
            }

            // Ahora inicializamos el panel como una variable de clase
            buttonsPanel = new Panel()  // No se declara localmente
            {
                Width = screenWidth,  // Ajustamos al ancho de la pantalla
                Height = screenHeight - 100,  // Ajustamos al alto de la pantalla (dejando espacio para el botón Home)
                Left = 0,
                Top = 0,
                Visible = false,  // Inicialmente el panel estará oculto
            };

            // Añadir el evento de clic del botón "Home"
            homeButton.Click += HomeButton_Click;

            // Agregar el botón "Home" y el panel a los controles del formulario
            this.Controls.Add(homeButton);
            this.Controls.Add(buttonsPanel);

            // Llamada a CreateButtons con las ubicaciones iniciales de los botones según la resolución
            CreateButtons(buttonConfigs, startX, startY); // startX y startY dependientes de la resolución
        }




        /// <summary>
        /// Método para crear botones con imágenes o texto.
        /// </summary>
        private void CreateButtons(ButtonConfig[] buttonConfigs, int startX, int startY)
        {
            // Validar que buttonConfigs no sea nulo ni vacío
            if (buttonConfigs == null || buttonConfigs.Length == 0)
            {
                throw new ArgumentNullException(nameof(buttonConfigs), "La colección de configuraciones de botones no puede ser nula o vacía.");
            }

            // Validar que ningún elemento dentro de buttonConfigs sea nulo
            if (buttonConfigs.Any(config => config == null))
            {
                throw new ArgumentException("Todos los elementos de buttonConfigs deben ser instancias válidas.");
            }

            // Calcular el alto total requerido para el formulario basado en los botones
            int totalHeight = buttonConfigs.Sum(config => config.Height) +
                              (buttonConfigs.Length - 1) * buttonConfigs[0].Spacing;

            // Ajustar el tamaño del formulario para que pueda contener todos los botones
            this.Size = new Size(Width, totalHeight + 700); // Ajustamos solo el tamaño de altura

            // Iterar sobre cada configuración de botón
            foreach (var config in buttonConfigs)
            {
                Button button = new Button()
                {
                    Width = config.Width,
                    Height = config.Height,
                    Left = startX,  // La posición X se mantiene fija
                    Top = startY,   // La posición Y se va ajustando para cada botón
                    BackColor = config.BackgroundColor,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White
                };

                // Aquí se decide si el botón debe mostrar una imagen o texto
                if (config.UseImages)
                {
                    if (!string.IsNullOrEmpty(config.ImageName))
                    {
                        button.BackgroundImage = LoadEmbeddedImage(config.ImageName, config.Width, config.Height);
                        button.BackgroundImageLayout = ImageLayout.Center; // Ajustar la imagen dentro del botón
                        button.Text = ""; // No mostrar texto
                    }
                }
                else
                {
                    button.Text = config.Label;
                }

                // Ajustar la fuente de los botones dependiendo de la resolución de pantalla
                button.Font = new Font("Arial", 9, FontStyle.Bold);

                // Eliminar el borde del botón
                button.FlatAppearance.BorderSize = 0;

                // Configurar el color de fondo cuando el ratón pasa sobre el botón
                button.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#5D5D5D");

                // Lógica específica para cada botón
                if (config.Label == "ACTUALIZAR")
                {
                    // Configuración específica para el botón "ACTUALIZAR"
                    button.Click += (s, e) =>
                    {
                        RestartAciertala();  // Llamada directa al método para reiniciar la aplicación
                        buttonsPanel.Visible = false;  // Cerrar el panel al hacer clic en cualquier botón
                    };
                }
                else if (config.Label == "CONEXION REMOTA")
                {
                    // Configuración específica para el botón "CONEXION REMOTA"
                    button.Click += (s, e) =>
                    {
                        OpenRemoteConnection(@"C:\BotonesAciertala\BotonF11.exe");  // Llamar al método para abrir la aplicación
                        buttonsPanel.Visible = false;  // Cerrar el panel al hacer clic en cualquier botón
                    };
                }
                else if (config.Label == "CABALLOS")
                {
                    // Configuración específica para el botón "CABALLOS"
                    button.Click += (s, e) =>
                    {
                        // Cerrar el formulario Caballos si está abierto
                        var caballosForm = Application.OpenForms["Caballos"]; // Verifica si el formulario ya está abierto
                        if (caballosForm != null)
                        {
                            caballosForm.Close(); // Cierra el formulario si ya está abierto
                        }

                        // Abre una nueva instancia del formulario Caballos
                        var newCaballosForm = new Caballos();
                        newCaballosForm.Show(); // Mostrar el formulario Caballos

                        // Cerrar el panel de botones
                        buttonsPanel.Visible = false;
                    };
                }

                else if (config.Label == "TRANSMISION 2")
                {
                    // Configuración específica para el botón "TRANSMISION 2"
                    button.Click += (s, e) =>
                    {
                        // Cerrar el formulario Transmision2 si está abierto
                        var transmision2Form = Application.OpenForms["Transmision2"]; // Verifica si el formulario ya está abierto
                        if (transmision2Form != null)
                        {
                            transmision2Form.Close(); // Cierra el formulario
                        }

                        // Abre una nueva instancia del formulario Transmision2
                        OpenTransmision2Form();

                        // Cerrar el panel de botones
                        buttonsPanel.Visible = false;
                    };
                }

                else if (config.Label == "REGISTRO")
                {
                    button.BackgroundImage = LoadEmbeddedImage($"AciertalaApp.{config.Label}.ico", config.Width, config.Height);
                    button.BackgroundImageLayout = ImageLayout.Center;

                    // Acción específica para el botón "REGISTRO"
                    button.Click += (s, e) =>
                    {
                        OpenWebView(config.URL, button);  // Llamada a la función que abre la URL para REGISTRO
                        buttonsPanel.Visible = false;  // Cerrar el panel al hacer clic en cualquier botón
                    };
                }
                else
                {
                    // Para los demás botones, se usa la misma función para abrir una URL cuando se hace clic en ellos
                    button.Click += (s, e) =>
                    {
                        OpenWebView(config.URL, button);  // Llamada a la función que abre la URL
                        buttonsPanel.Visible = false;  // Cerrar el panel al hacer clic en cualquier botón
                    };
                }

                // Agregar el botón al formulario (asegúrate de agregarlo al panel de botones)
                buttonsPanel.Controls.Add(button);

                // Actualizar la posición Y para el siguiente botón (con espacio entre ellos)
                startY += config.Height + config.Spacing;  // Mover hacia abajo con espacio entre botones
            }
        }


        private void OpenTransmision2Form()
        {
            try
            {
                // Crear y mostrar el formulario Transmision2
                Transmision2 form = new Transmision2();
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario Transmisión 2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void OpenRemoteConnection(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true // Ejecutar con el shell del sistema
                    });
                }
                else
                {
                    MessageBox.Show($"No se encontró el archivo en la ruta especificada: {filePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al intentar abrir la conexión remota: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }






        private Image LoadEmbeddedImage(string resourceName, int width, int height)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    Image img = Image.FromStream(stream);

                    // Redimensionar la imagen al tamaño deseado
                    return new Bitmap(img, new Size(40, 40)); 
                }
            }
            return null;
        }


        // Método para mostrar el formulario de configuración de URL
        private void PromptForURL()
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 450;
                prompt.Height = 300; // Aumentar la altura para incluir el CheckBox
                prompt.Text = "Configurar URL";
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;

                Label textLabel = new Label()
                {
                    Left = 20,
                    Top = 20,
                    Width = 400,
                    Text = "Ingrese la URL para el botón REGISTRO:",
                    Font = new Font("Arial", 10, FontStyle.Regular)
                };

                TextBox textBox = new TextBox()
                {
                    Left = 20,
                    Top = 60,
                    Width = 400,
                    Text = registroURL,
                    Font = new Font("Arial", 10, FontStyle.Regular),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // CheckBox para habilitar/deshabilitar el botón Caballos
                CheckBox chkCaballosEnabled = new CheckBox()
                {
                    Text = "Mostrar botón Caballos",
                    Left = 20,
                    Top = 100, // Colocar debajo del TextBox
                    Checked = false, // Valor predeterminado
                    Width = 200
                };

                // Nuevo CheckBox para deshabilitar caballosEnabled
                CheckBox chkDisableCaballos = new CheckBox()
                {
                    Text = "Modo Cajero",
                    Left = 20,
                    Top = 130, // Colocar debajo de chkCaballosEnabled
                    Checked = false, // Valor predeterminado
                    Width = 250
                };

                Button confirmation = new Button()
                {
                    Text = "Aceptar",
                    Left = 260,
                    Width = 80,
                    Top = 220, // Mover el botón de aceptación hacia abajo para que haya espacio
                    DialogResult = DialogResult.OK,
                    BackColor = ColorTranslator.FromHtml("#4CAF50"),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                confirmation.FlatAppearance.BorderSize = 0;

                Button cancel = new Button()
                {
                    Text = "Cancelar",
                    Left = 350,
                    Width = 80,
                    Top = 220, // Mover el botón de cancelación hacia abajo
                    DialogResult = DialogResult.Cancel,
                    BackColor = ColorTranslator.FromHtml("#F44336"),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                cancel.FlatAppearance.BorderSize = 0;

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(chkCaballosEnabled); // Añadir el CheckBox
                prompt.Controls.Add(chkDisableCaballos); // Añadir el nuevo CheckBox
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancel);

                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;

                // Manejo de cambio de estado de chkDisableCaballos
                chkDisableCaballos.CheckedChanged += (sender, e) =>
                {
                    chkCaballosEnabled.Enabled = !chkDisableCaballos.Checked;
                    chkCaballosEnabled.Visible = !chkDisableCaballos.Checked;
                };

                if (prompt.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(textBox.Text))
                {
                    string inputURL = textBox.Text;  // La URL ingresada por el usuario

                    if (Uri.IsWellFormedUriString(inputURL, UriKind.Absolute))  // Verificar si la URL es válida
                    {
                        registroURL = inputURL;  // Asignar la URL ingresada
                        bool caballosEnabled = chkCaballosEnabled.Checked;  // Guardar el estado del CheckBox
                        bool disableCaballos = chkDisableCaballos.Checked;  // Guardar el estado del nuevo CheckBox
                        SaveRegistroURLToLocalStorage(caballosEnabled, disableCaballos);  // Guardar en LocalStorage

                        MessageBox.Show("URL configurada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Continuar con la inicialización según el valor de los CheckBox
                        if (disableCaballos)
                        {
                            string vboxAppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "VBOX.appref-ms");
                            if (File.Exists(vboxAppPath))
                            {
                                Process.Start(vboxAppPath); // Ejecutar la aplicación
                            }
                            else
                            {
                                MessageBox.Show("No se encontró la aplicación VBOX.appref-ms en el escritorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            InitializeComponents3();  // Llamar a InitializeComponents3 si está en "Modo Cajero"
                        }
                        else if (caballosEnabled)
                        {
                            string downloadUrl = "https://releases.xpressgaming.net/tech.xpress.aciertala/win32/Aciertala-setup-2.7.2.zip";
                            string localPath = @"C:\Aciertala";
                            string zipPath = Path.Combine(localPath, "Aciertala-setup.zip");
                            string extractedPath = Path.Combine(localPath, "extracted");

                            if (!Directory.Exists(localPath))
                            {
                                Directory.CreateDirectory(localPath);
                            }

                            InstallAciertala(downloadUrl, zipPath, extractedPath);
                            InitializeComponents();
                            RegisterHotKey(this.Handle, 0, 0, (uint)Keys.F11);
                            RegisterHotKey(this.Handle, 1, 0, (uint)Keys.F6);
                        }
                        else
                        {
                            string downloadUrl = "https://releases.xpressgaming.net/tech.xpress.aciertala/win32/Aciertala-setup-2.7.2.zip";
                            string localPath = @"C:\Aciertala";
                            string zipPath = Path.Combine(localPath, "Aciertala-setup.zip");
                            string extractedPath = Path.Combine(localPath, "extracted");

                            if (!Directory.Exists(localPath))
                            {
                                Directory.CreateDirectory(localPath);
                            }

                            InstallAciertala(downloadUrl, zipPath, extractedPath);
                            RegisterHotKey(this.Handle, 0, 0, (uint)Keys.F11);
                            RegisterHotKey(this.Handle, 1, 0, (uint)Keys.F6);
                            InitializeComponents2();
                        }

                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show("La URL ingresada no es válida. Inténtelo nuevamente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    this.Show();  // Si el usuario canceló o no ingresó nada, solo mostramos la ventana principal
                }

            }
        }



        private void OpenWebView(string url, Button button)
        {
            // Si el formulario de WebView ya está abierto, ciérralo
            if (webViewForm != null && !webViewForm.IsDisposed)
            {
                webViewForm.Close();
            }

            // Verificar si el botón es "REGISTRO" y usar la URL configurada
            if (button.Text == "REGISTRO")
            {
                url = registroURL; // Usar la URL configurada o predeterminada
            }
            else
            {
                button.BackColor = ColorTranslator.FromHtml("#313439"); // Restaurar el color del botón
            }

            // Abrir el formulario de WebView con la URL
            webViewForm = new WebViewForm(url);
            webViewForm.Show();
        }

    }

    public class ButtonConfig
    {
        public string Label { get; set; }            // Texto del botón
        public string URL { get; set; }              // URL que se abrirá al hacer clic (si aplica)
        public int Width { get; set; }               // Ancho del botón
        public int Height { get; set; }              // Altura del botón
        public Color BackgroundColor { get; set; }   // Color de fondo del botón
        public bool UseImages { get; set; }          // Indica si se usan imágenes en el botón
        public int Spacing { get; set; }             // Espaciado entre botones
        public string ImageName { get; set; }        // Nombre de la imagen asociada al botón
        public EventHandler OnClick { get; set; }    // Delegado para manejar eventos Click personalizados

        // Constructor
        public ButtonConfig(
            string label,
            string url,
            int width,
            int height,
            Color backgroundColor,
            bool useImages,
            int spacing,
            string imageName = null,
            EventHandler onClick = null)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("El texto del botón (Label) no puede ser nulo o vacío.", nameof(label));
            }

            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("El ancho y la altura del botón deben ser mayores que cero.", nameof(width));
            }

            // Asignación de propiedades
            Label = label;
            URL = url;
            Width = width;
            Height = height;
            BackgroundColor = backgroundColor;
            UseImages = useImages;
            Spacing = spacing;
            ImageName = imageName;
            OnClick = onClick;
        }
    }






    public class WebViewForm : Form
    {
        private WebView2 browser;

        public WebViewForm(string url)
        {
            this.Text = "";
            this.StartPosition = FormStartPosition.Manual;

            // Obtener la información del monitor donde se abrirá el formulario
            Screen currentScreen = Screen.FromPoint(Cursor.Position); // Monitor en el que está el cursor

            // Establecer el tamaño al ancho y altura fija del monitor actual
            int screenWidth = currentScreen.Bounds.Width;
            int fixedHeight = 1050; // Altura fija
            this.Size = new Size(screenWidth, fixedHeight - 50); // Reducir ligeramente la altura para evitar cortes

            this.Location = new Point(currentScreen.Bounds.X, currentScreen.Bounds.Y + 80); // Ajustar la ubicación al monitor actual

            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.BackColor = ColorTranslator.FromHtml("#313439");

            // Establecer el ícono del formulario con validación
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "AciertalaApp.Resources.AciertalaICO.ico";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Console.WriteLine("Recursos disponibles en el ensamblado:");
                        foreach (var resource in assembly.GetManifestResourceNames())
                        {
                            Console.WriteLine(resource);
                        }

                        throw new Exception($"No se pudo encontrar el recurso '{resourceName}'. Verifica la ruta y configuración del recurso.");
                    }

                    this.Icon = new Icon(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el ícono: {ex.Message}");

                string fallbackIconPath = @"C:\Ruta\AciertalaICO.ico"; // Cambiar a la ruta real en caso de prueba
                if (File.Exists(fallbackIconPath))
                {
                    this.Icon = new Icon(fallbackIconPath);
                }
                else
                {
                    Console.WriteLine("No se encontró el ícono de respaldo. El formulario se cargará sin ícono.");
                }
            }

            browser = new WebView2
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 20) // Añadir margen en la parte inferior para evitar cortes
            };

            // Esto asegura que el CoreWebView2 está inicializado antes de suscribirse a eventos.
            this.Load += async (s, e) =>
            {
                try
                {
                    // Asegúrate de que CoreWebView2 está inicializado
                    await browser.EnsureCoreWebView2Async(null);

                    // Navega solo si la URL es válida
                    if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        browser.CoreWebView2.Navigate(url);
                    }
                    else
                    {
                        MessageBox.Show("URL no válida o vacía.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // Suscribir eventos después de la inicialización
                    browser.CoreWebView2.NewWindowRequested += (sender, args) =>
                    {
                        args.Handled = true; // Cancelar nueva ventana
                        var newForm = new WebViewForm(args.Uri.ToString());
                        newForm.Show();
                        newForm.Deactivate += (sender2, e2) => newForm.Close();
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al inicializar WebView2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };



            browser.NavigationCompleted += InjectScriptWithInterval;

            this.Controls.Add(browser);
        }




        private void InjectScriptWithInterval(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var scriptToModifyElements = @"(function() {
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
            '.sc-itMJkM.jkTSKs', // Nuevo elemento agregado

            
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

            browser.CoreWebView2.ExecuteScriptAsync(scriptToModifyElements);
        }

    }
}


