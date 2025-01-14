using System;
using System.IO;
using System.Collections.Generic;

public class LocalStorage
{
    private string appDataPath;
    private string storageFilePath;

    public LocalStorage(string appFolderName = "aciertalaapp")
    {
        // Ruta al directorio de la aplicación en AppData (local)
        string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appFolderName);

        // Crear el directorio si no existe
        if (!Directory.Exists(appDataDirectory))
        {
            Directory.CreateDirectory(appDataDirectory);
        }

        storageFilePath = Path.Combine(appDataDirectory, "storage.txt");
    }

    // Guardar datos en formato texto (clave=valor)
    public void Save(Dictionary<string, string> data)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(storageFilePath))
            {
                foreach (var kvp in data)
                {
                    writer.WriteLine($"{kvp.Key}={kvp.Value}"); // Guardar como clave=valor
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar los datos: {ex.Message}");
        }
    }

    // Cargar datos desde el archivo de texto
    public Dictionary<string, string> Load()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();

        try
        {
            if (File.Exists(storageFilePath))
            {
                using (StreamReader reader = new StreamReader(storageFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            data[parts[0]] = parts[1];
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar los datos: {ex.Message}");
        }

        return data;
    }
}
