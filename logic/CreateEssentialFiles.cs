using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;

class CreateEssentialFiles
{
    public static void CheckEssentialFiles()
    {
        try
        {
            TryCreateEmptySQLiteDatabase();
            TryCreateEmptyMessagesJson();
            TryCreateEmptyAppSettingsJson();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private static void TryCreateEmptyMessagesJson()
    {
        var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\Messages.json"));

        if (File.Exists(path))
            return;
        else
        {
            var jsonFormat = new
            {
                Messages = new List<string> { }
            };

            string jsonString = JsonSerializer.Serialize(jsonFormat, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(path, jsonString);
        }
    }
    private static void TryCreateEmptyAppSettingsJson()
    {
        var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\AppSettings.json"));

        if (File.Exists(path))
            return;
        else
        {
            var jsonFormat = new
            {
                GUID = "",
                UUID = "",
                DownloadFolder = "",
                RegisteredFolders = new List<string> { }
            };
            string jsonString = JsonSerializer.Serialize(jsonFormat, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(path, jsonString);
        }
    }
    private static void TryCreateEmptySQLiteDatabase()
    {
        var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\DeviceMusicData.db"));

        if (File.Exists(path))
            return;
        else
        {
            var connection = UserDatabase.OpenSQLiteConnection();
            CreateDatabaseTables(connection);
        }

    }
    private static void CreateDatabaseTables(SQLiteConnection connection)
    {
        var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [allMusic]" +
        "( id  INTEGER NOT NULL," +
        "songID   VARCHAR(38) NOT NULL UNIQUE," +
        "name  TEXT," +
        "absolutepath  TEXT," +
        "uploadfile    BOOLEAN DEFAULT 'TRUE', " +
        "PRIMARY KEY('id' AUTOINCREMENT))", connection);

        cmd.ExecuteNonQuery();
    }
}