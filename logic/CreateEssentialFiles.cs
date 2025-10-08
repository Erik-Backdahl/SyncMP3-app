using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.Json;

class CreateEssentialFiles
{
    private static string DataDirectory => GetDataDirectory();
    private static string MessagesPath => Path.Combine(DataDirectory, "Messages.json");
    private static string AppSettingsPath => Path.Combine(DataDirectory, "AppSettings.json");
    private static string SqlitePath => Path.Combine(DataDirectory, "DeviceMusicData.db");

    public static void CheckEssentialFiles()
    {
        try
        {
            Directory.CreateDirectory(DataDirectory);
            TryCreateEmptySQLiteDatabase();
            TryCreateEmptyMessagesJson();
            TryCreateEmptyAppSettingsJson();
        }
        catch (Exception ex)
        {
            // write to console/log so published app doesn't silently swallow errors
            File.AppendAllText(Path.Combine(DataDirectory, "create_files_error.log"),
                $"[{DateTime.UtcNow:O}] {ex}\n");
            throw;
        }
    }

    // Look for a "Data" folder by searching upward from the exe location.
    // In DEBUG build this will find the project Data folder (keeps same Data in debug).
    private static string GetDataDirectory()
    {
        var baseDir = AppContext.BaseDirectory;
        var dir = new DirectoryInfo(baseDir);

        // search upward for the project file "AvaloniaTest.csproj"
        for (int i = 0; i < 12 && dir != null; i++)
        {
            var csprojFile = dir.GetFiles("AvaloniaTest.csproj").FirstOrDefault();
            if (csprojFile != null)
            {
                var projectData = Path.Combine(dir.FullName, "Data");
                if (!Directory.Exists(projectData))
                    Directory.CreateDirectory(projectData);
                return projectData;
            }
            dir = dir.Parent;
        }

        // fallback: Data next to the running exe
        var exeSide = Path.Combine(AppContext.BaseDirectory, "Data");
        if (!Directory.Exists(exeSide))
            Directory.CreateDirectory(exeSide);
        return exeSide;
    }

    private static void TryCreateEmptyMessagesJson()
    {
        if (File.Exists(MessagesPath))
            return;

        var jsonFormat = new
        {
            Messages = new List<string> { }
        };
        string jsonString = JsonSerializer.Serialize(jsonFormat, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(MessagesPath, jsonString);
    }

    private static void TryCreateEmptyAppSettingsJson()
    {
        if (File.Exists(AppSettingsPath))
            return;

        var jsonFormat = new
        {
            GUID = "",
            UUID = "",
            DownloadFolder = "",
            RegisteredFolders = new List<string> { }
        };
        string jsonString = JsonSerializer.Serialize(jsonFormat, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(AppSettingsPath, jsonString);
    }

    private static void TryCreateEmptySQLiteDatabase()
    {
        if (File.Exists(SqlitePath))
            return;

        // If your UserDatabase.OpenSQLiteConnection uses the same path,
        // update it to accept SqlitePath or create connection here:
        SQLiteConnection.CreateFile(SqlitePath);
        using var connection = new SQLiteConnection($"Data Source={SqlitePath};Version=3;");
        connection.Open();
        CreateDatabaseTables(connection);
        connection.Close();
    }

    private static void CreateDatabaseTables(SQLiteConnection connection)
    {
        using var cmd = new SQLiteCommand(
            "CREATE TABLE IF NOT EXISTS allMusic (" +
            "id INTEGER PRIMARY KEY AUTOINCREMENT," +
            "songID VARCHAR(38) NOT NULL UNIQUE," +
            "name TEXT," +
            "absolutepath TEXT," +
            "uploadfile BOOLEAN DEFAULT 1)", connection);
        cmd.ExecuteNonQuery();
    }
}