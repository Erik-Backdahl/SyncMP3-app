using System;
using System.IO;
using System.Text.Json;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System.Security.Principal;
using DynamicData;
using System.Text.Json.Nodes;
using System.Linq;
using System.Reflection;

class ModifyAppSettings
{
    public static readonly string PathToDataFolder = GetPathToFolder();


    public static readonly string PathToAppSetting = Path.Combine(PathToDataFolder, "AppSettings.json");
    public static readonly string PathToSQLData = Path.Combine(PathToDataFolder, "DeviceMusicData.db");
    public static readonly string PathToMessages = Path.Combine(PathToDataFolder, "Messages.json");
    private static string GetPathToFolder()
    {
        string exeDirectory = AppContext.BaseDirectory;

        if (exeDirectory.ToLower().Contains("debug"))
        {
            // Go up 3 directories
            DirectoryInfo dir = new DirectoryInfo(exeDirectory);
            for (int i = 0; i < 3; i++)
            {
                if (dir.Parent != null)
                {
                    dir = dir.Parent;
                }
                else
                {
                    throw new Exception("cannot find path to Data folder. if you are getting this exception try putting the executible in another subdirectory");
                }
            }

            Console.WriteLine("Moved up 3 folders. New path:");
            Console.WriteLine(dir.FullName);
            return Path.Combine(dir.FullName, "Data");
        }
        else
        {
            Console.WriteLine("Not running from Debug directory. Current path:");
            Console.WriteLine(exeDirectory);
            return Path.Combine(exeDirectory, "Data");
        }
    }
    public static string ReadDownloadFolder()
    {
        string jsonData = File.ReadAllText(PathToAppSetting);

        JsonFormat? settings = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (settings != null && !string.IsNullOrEmpty(settings.DownloadFolder))
            return settings.DownloadFolder;
        else
            throw new Exception("No download Folder registered");
    }
    public static string[] ReadRegisteredMusicFolders()
    {
        // Read the JSON file content
        string jsonData = File.ReadAllText(PathToAppSetting);

        // Deserialize into JsonFormat object
        JsonFormat? settings = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (settings != null && settings.RegisteredFolders != null && settings.RegisteredFolders.Length > 0)
            return settings.RegisteredFolders;
        else
            throw new Exception("No Folders Registered to search, please click the \"Folder\" button");
    }
    public static void AddRegisteredFolder(string unEscapedFolder)
    {
        string folder = unEscapedFolder.Replace("/", "\\");
        string jsonData = File.ReadAllText(PathToAppSetting);

        JsonFormat? root = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (root == null || root.RegisteredFolders == null)
            throw new Exception("AppSettings file Not Found");

        foreach (string currentlyRegistered in root.RegisteredFolders)
        {
            if (folder == currentlyRegistered)
            {
                throw new Exception("Folder already registered");
            }
        }

        var list = root.RegisteredFolders.ToList();
        list.Add(folder);
        root.RegisteredFolders = list.ToArray();

        if (root.RegisteredFolders.Length == 0)
        {
            AddDownloadFolder(folder);
        }

        string updatedJson = JsonSerializer.Serialize(root);
        File.WriteAllText(PathToAppSetting, updatedJson);
    }
    public static void AddDownloadFolder(string folder)
    {
        string jsonData = File.ReadAllText(PathToAppSetting);

        JsonFormat? root = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (root == null || root.DownloadFolder == null)
            throw new Exception("AppSettings file Not Found");

        root.DownloadFolder = folder;

        string updatedJson = JsonSerializer.Serialize(root);
        File.WriteAllText(PathToAppSetting, updatedJson);
    }
    public static string GetUUID()
    {
        string jsonData = File.ReadAllText(PathToAppSetting);

        JsonFormat? root = JsonSerializer.Deserialize<JsonFormat>(jsonData);
        if (root != null)
        {
            if (string.IsNullOrEmpty(root.UUID))
            {
                // Set UUID
                root.UUID = Guid.NewGuid().ToString();

                // Serialize back to JSON if needed
                string updatedJson = JsonSerializer.Serialize(root);
                File.WriteAllText(PathToAppSetting, updatedJson);
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
        string jsonData = File.ReadAllText(PathToAppSetting);

        JsonFormat? root = JsonSerializer.Deserialize<JsonFormat>(jsonData);
        if (root != null)
        {
            return root.GUID;
        }
        else
            throw new AppSettingsFileNotFoundException();
    }
    internal static void RegisterGUID(string GUID)
    {
        string jsonData = File.ReadAllText(PathToAppSetting);

        JsonFormat? root = JsonSerializer.Deserialize<JsonFormat>(jsonData);

        if (root != null)
        {
            if (string.IsNullOrEmpty(root.GUID))
            {
                root.GUID = GUID;
                string updatedJson = JsonSerializer.Serialize(root);
                File.WriteAllText(PathToAppSetting, updatedJson);
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
    public string[]? RegisteredFolders { get; set; }
}