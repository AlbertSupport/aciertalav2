using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpCompress.Archives;
using SharpCompress.Common;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.Security.Principal;

public class AppUpdater
{
    private static readonly string downloadUrl = "https://apk.solutions/prueba/jsonaciertala/AciertalaApp.rar";
    private static readonly string downloadPath = @"C:\Actualizaciones\AciertalaApp.rar";
    private static readonly string extractPath = @"C:\Actualizaciones\Descomprimido";
    private static readonly string versionFilePath = Path.Combine(extractPath, "version.json");
    private static readonly string localVersionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AciertalaApp", "version.json");

    public static async Task CheckForUpdatesAsync()
    {
        try
        {
            // Descargar y descomprimir el archivo
            DownloadFile(downloadUrl, downloadPath);
            ExtractRar(downloadPath, extractPath);

            // Leer la versión del archivo descargado
            string newVersion = ReadVersionFromJson(versionFilePath);

            // Verificar la versión guardada localmente
            string currentVersion = ReadLocalVersion();

            if (currentVersion != newVersion)
            {
                // Mostrar la ventana personalizada
                using (var updateDialog = new CustomUpdateDialog())
                {
                    updateDialog.ShowDialog();

                    if (updateDialog.UserAccepted)
                    {
                        // Guardar la nueva versión localmente
                        SaveLocalVersion(newVersion);

                        // Desinstalar la aplicación actual y ejecutar setup.exe
                        await UninstallAndInstallSetupAsync(extractPath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error durante el proceso de actualización: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }



    // Método para desinstalar y luego ejecutar setup.exe
    public static async Task UninstallAndInstallSetupAsync(string extractPath)
    {
        try
        {
            // Ruta de la carpeta Apps en AppData\Local
            string appsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Apps");

            // Eliminar la carpeta Apps de forma asíncrona si existe
            if (Directory.Exists(appsPath))
            {
                await Task.Run(() => DeleteFolder(appsPath));
                Console.WriteLine("La carpeta Apps ha sido eliminada correctamente.");
            }
            else
            {
                Console.WriteLine($"La carpeta {appsPath} no existe.");
            }

            // Ejecutar setup.exe desde la ruta especificada
            string setupPath = Path.Combine(extractPath, "setup.exe");
            if (File.Exists(setupPath))
            {
                await Task.Run(() => ExecuteSetup(setupPath));
            }
            else
            {
                Console.WriteLine("No se encontró el archivo setup.exe en la ruta especificada.");
            }

            // Confirmar reinicio del sistema
            RestartComputer();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante el proceso de desinstalación e instalación: {ex.Message}");
        }
    }

    // Método para reiniciar la computadora
    private static void RestartComputer()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/r /t 0", // Reinicio inmediato
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al intentar reiniciar la computadora: {ex.Message}");
        }
    }


    // Método para eliminar una carpeta de forma segura
    private static void DeleteFolder(string folderPath)
    {
        try
        {
            // Verificar si la carpeta existe
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"La carpeta {folderPath} no existe.");
                return;
            }

            // Eliminar todos los archivos dentro de la carpeta
            var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                DeleteFile(file);
            }

            // Eliminar todos los subdirectorios
            var directories = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);
            foreach (var directory in directories)
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true); // Eliminar subdirectorio
                }
            }

            // Eliminar la carpeta principal
            Directory.Delete(folderPath, true);
            Console.WriteLine($"La carpeta {folderPath} fue eliminada correctamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar la carpeta: {ex.Message}");
        }
    }


    // Método para eliminar un archivo de forma segura
    private static void DeleteFile(string filePath)
    {
        for (int i = 0; i < 3; i++) // Intentar hasta 3 veces
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Remover atributos de solo lectura, si existen
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath); // Eliminar el archivo
                }
                break; // Salir del bucle si el archivo se elimina correctamente
            }
            catch (Exception ex)
            {
                if (i == 2) // Registrar error después del tercer intento
                {
                    Console.WriteLine($"No se pudo eliminar el archivo '{filePath}': {ex.Message}");
                }
            }
        }
    }

    // Método para ejecutar setup.exe
    private static void ExecuteSetup(string setupPath)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = setupPath,
                UseShellExecute = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit(); // Esperar a que el proceso termine
                MessageBox.Show("La instalación se ha completado correctamente.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No se pudo iniciar el instalador.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al ejecutar el instalador: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }




    // Descargar archivo usando WebClient
    private static void DownloadFile(string url, string filePath)
    {
        string folderPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        using (WebClient client = new WebClient())
        {
            client.DownloadFile(url, filePath);
        }
    }

    // Descomprimir archivo RAR usando SharpCompress
    private static void ExtractRar(string rarFilePath, string destinationPath)
    {
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        using (var archive = ArchiveFactory.Open(rarFilePath))
        {
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    entry.WriteToDirectory(destinationPath, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }
    }

    // Leer la versión del archivo JSON
    private static string ReadVersionFromJson(string jsonFilePath)
    {
        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            JObject jsonObject = JObject.Parse(jsonContent);
            return jsonObject["version"]?.ToString();
        }
        throw new Exception("No se pudo leer la versión del archivo JSON.");
    }

    // Leer la versión guardada localmente
    private static string ReadLocalVersion()
    {
        if (File.Exists(localVersionPath))
        {
            string jsonContent = File.ReadAllText(localVersionPath);
            JObject jsonObject = JObject.Parse(jsonContent);
            return jsonObject["version"]?.ToString();
        }
        return null; // Si no existe, devolver null
    }

    // Guardar la nueva versión localmente
    private static void SaveLocalVersion(string version)
    {
        string folderPath = Path.GetDirectoryName(localVersionPath);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        JObject jsonObject = new JObject
        {
            ["version"] = version
        };

        File.WriteAllText(localVersionPath, jsonObject.ToString());
    }
}
