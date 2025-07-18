using System;
using System.Configuration;
using System.IO;
using System.Text.Json;

class ReadAppSettings
{
    public static string[] ReadAllFolderLocations()
    {
        string jsonFilePath = @"C:\Users\Erik\VSC\SMALLPROJECTS\AvaloniaTest\SQLiteDatabase\AppSettings.json";

        // Read the JSON file content
        string jsonData = File.ReadAllText(jsonFilePath);

        // Deserialize into JsonFormat object
        JsonFormat? settings = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (settings != null && settings.RegisterdFolders != null)
        {
            return settings.RegisterdFolders;
        }
        else
        {
            return new string[0]; 
        }
    }
}

class JsonFormat //TODO: figure out wtf is happening here
{
    public string? DownloadFolder { get; set; }
    public string[]? RegisterdFolders { get; set; }
}