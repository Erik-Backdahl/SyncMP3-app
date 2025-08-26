using System;
using System.IO;
using System.Text.Json;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System.Security.Principal;

class ModifyAppSettings
{
    private static string jsonFilePath = @"C:\Users\Erik\VSC\SMALLPROJECTS\SyncMP3App\Data\AppSettings.json";
    public static string ReadDownloadFolder()
    {
        string jsonData = File.ReadAllText(jsonFilePath);

        JsonFormat? settings = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (settings != null && settings.DownloadFolder != null)
            return settings.DownloadFolder;
        else
            return "Failed Download Folder not found";
    }
    public static string[] ReadRegisteredMusicFolders()
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
    internal static void AddRegisteredFolder(string folder)
    {
        string jsonData = File.ReadAllText(jsonFilePath);

        JsonFormat? root = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (root != null)
        {
            //check if the folder already exists
           
           
        }
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
    internal static void RegisterGUID(string GUID)
    {
        string jsonData = File.ReadAllText(jsonFilePath);

        JsonFormat? root = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (root != null)
        {
            if (string.IsNullOrEmpty(root.GUID))
            {
                root.GUID = GUID;
                string updatedJson = JsonSerializer.Serialize(root);
                File.WriteAllText(jsonFilePath, updatedJson);
            }
            else
            {
                throw new Exception("a GUID is already registered");
            }
        }
        else
            throw new AppSettingsFileNotFoundException();
    }
}


class JsonFormat //TODO: figure out wtf is happening here
{
    public required string GUID { get; set; }
    public string? UUID { get; set; }
    public string? DownloadFolder { get; set; }
    public string[]? RegisterdFolders { get; set; }
}