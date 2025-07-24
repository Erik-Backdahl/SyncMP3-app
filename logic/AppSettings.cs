using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.Json;

class AppSettings
{
    private static string jsonFilePath = @"C:\Users\Erik\VSC\SMALLPROJECTS\AvaloniaTest\SQLiteDatabase\AppSettings.json";
    public static string ReadDownloadFolder()
    {
        string jsonData = File.ReadAllText(jsonFilePath);

        JsonFormat? settings = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (settings != null && settings.DownloadFolder != null)
            return settings.DownloadFolder;
        else
            return "Failed Download Folder not found";
    }
    public static string[] ReadAllFolderLocations()
    {
        // Read the JSON file content
        string jsonData = File.ReadAllText(jsonFilePath);

        // Deserialize into JsonFormat object
        JsonFormat? settings = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (settings != null && settings.RegisterdFolders != null)
            return settings.RegisterdFolders;
        else
            return [];
    }
    public static string GetUUID()
    {
        string jsonData = File.ReadAllText(jsonFilePath);

        JsonFormat? root = JsonSerializer.Deserialize<JsonFormat>(jsonData);
        if (root != null)
        {
            if (string.IsNullOrEmpty(root.UUID))
            {
                // Set UUID
                root.UUID = Guid.NewGuid().ToString();

                // Serialize back to JSON if needed
                string updatedJson = JsonSerializer.Serialize(root);
                File.WriteAllText(jsonFilePath, updatedJson);
                return root.UUID;
            }
            else
                return root.UUID;
        }
        else
            throw new AppSettingsFileNotFoundException();
    }

    public static string GetGUID()
    {
        string jsonData = File.ReadAllText(jsonFilePath);

        JsonFormat? root = JsonSerializer.Deserialize<JsonFormat>(jsonData);
        if (root != null)
        {
            if (string.IsNullOrEmpty(root.GUID))
            {
                throw new NoNetWorkRegisterException();
            }
            else
                return root.GUID;
        }
        else
            throw new AppSettingsFileNotFoundException();
    }
}


class JsonFormat //TODO: figure out wtf is happening here
{
    public string GUID { get; set; }
    public string? UUID { get; set; }
    public string? DownloadFolder { get; set; }
    public string[]? RegisterdFolders { get; set; }
}