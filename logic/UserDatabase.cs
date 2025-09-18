using System;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Text.Json;
using System.Data;
using Tmds.DBus.Protocol;
using System.Linq;
using System.Threading.Tasks;
class UserDatabase
{
    public static string ConvertDatabaseToJSON()
    {
        var connection = OpenSQLiteConnection();

        string tableName = "allMusic";

        var data = new List<Dictionary<string, object>>();

        string query = $"SELECT name, songID FROM {tableName} WHERE uploadfile = @uploadfile";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@uploadfile", true);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }
            data.Add(row);
        }

        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        return json;
    }
    public static Task AddAllMusicNotInDatabase()
    {
        using var connection = OpenSQLiteConnection();

        string[] allFolders = ModifyAppSettings.ReadRegisteredMusicFolders();

        string[] allMusicNames = GetFileNamesFromAllFolders();


        foreach (string individualFolder in allFolders)
        {
            string[] currentFoldersSongsNames = Directory.GetFiles(individualFolder);

            for (int i = 0; i < currentFoldersSongsNames.Length; i++)
            {
                string songTag = ReadTag(currentFoldersSongsNames[i]);

                if (songTag.StartsWith("Failed to find ID"))
                {//does not have ID give it one
                    songTag = TagFile(currentFoldersSongsNames[i]);
                }

                if (!FoundSongInDataBase(songTag, connection))
                {
                    SaveInSQLite(individualFolder, currentFoldersSongsNames[i], songTag, connection);
                }
                else
                {
                    //VerifySQLiteEntry(musicFolder[i], folder, songTag);
                    //TODO verify each song in the db eg: folderlocation has changed or name has changed 
                }
            }
        }
        return Task.CompletedTask;
    }
    public static Task DeleteEmptyEntiresInDatabase()
    {
        using var connection = OpenSQLiteConnection();

        string syntax = "SELECT absolutepath, name FROM allMusic";
        var cmd = new SQLiteCommand(syntax, connection);

        var reader = cmd.ExecuteReader();

        List<string> allAbsolutePathsInDatabase = [];

        while (reader.Read())
        {
            allAbsolutePathsInDatabase.Add(reader.GetString(0) + reader.GetString(1));
        }
        string[] allFolders = ModifyAppSettings.ReadRegisteredMusicFolders();

        List<string> allCurrentDownloadedSongs = [];

        foreach (string folder in allFolders)
        {
            string[] currentFoldersSongsNames = Directory.GetFiles(folder);

            foreach (string songPath in currentFoldersSongsNames)
            {
                allCurrentDownloadedSongs.Add(songPath);
            }
        }

        foreach (string songInDatabase in allAbsolutePathsInDatabase)
        {
            bool songFound = false;
            foreach (string currentDownloadedSong in allCurrentDownloadedSongs)
            {
                if (songInDatabase == currentDownloadedSong)
                {
                    songFound = true;
                    break;
                }
            }

            if (!songFound)
            {
                DeleteEntryInDatabase(Path.GetFileName(songInDatabase), connection);
            }
        }

        return Task.CompletedTask;
    }
    private static void DeleteEntryInDatabase(string songName, SQLiteConnection connection)
    {
        string syntax = "DELETE FROM allMusic WHERE name=@name";

        var cmd = new SQLiteCommand(syntax, connection);
        cmd.Parameters.AddWithValue("@name", songName);

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new Exception("Failed to delete entry in Database");
        }
    }

    private static bool FoundSongInDataBase(string songID, SQLiteConnection connection)
    {//TODO check if there is a song that has duplicate name in db
        string syntax = $"SELECT songID FROM allMusic WHERE songID=@songID";

        var cmd = new SQLiteCommand(syntax, connection);
        cmd.Parameters.AddWithValue("@songID", $"{songID}");

        try
        {
            using var reader = cmd.ExecuteReader();
            if (reader.HasRows)
                return true;
            else
                return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new Exception(ex.Message);
        }
    }
    private static void SaveInSQLite(string folderName, string songName, string songID, SQLiteConnection connection, bool uploadFile = true)
    {
        var cmd = new SQLiteCommand($"INSERT INTO allMusic(songID, name, absolutepath, uploadfile) VALUES (@songID, @songName, @absolutepath, @uploadfile)", connection);

        cmd.Parameters.AddWithValue("@songID", songID);
        cmd.Parameters.AddWithValue("@songName", songName);
        cmd.Parameters.AddWithValue("@absolutepath", folderName);
        cmd.Parameters.AddWithValue("@uploadfile", uploadFile);

        try
        {
            using var reader = cmd.ExecuteReader();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new Exception(ex.Message);
        }
    }
    public static string TagFile(string filePath)
    {
        try
        {
            var file = TagLib.File.Create(filePath);

            string uniqueId = Guid.NewGuid().ToString();
            file.Tag.Comment = $"UniqueID:{uniqueId}";

            file.Save();
            return uniqueId;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new Exception(ex.Message);
        }
    }
    public static string ReadTag(string filePath)
    {
        var file = TagLib.File.Create(filePath);

        string comment = file.Tag.Comment;

        if (comment != null && comment.StartsWith("UniqueID:"))
        {
            string embeddedId = comment.Substring("UniqueID:".Length);
            return embeddedId;
        }
        else
        {
            return "Failed to find ID";
        }
    }
    private static string[] GetFileNamesFromAllFolders()
    {// folderPath = C/stuff/music //NOT PATH TO A SINGLE FILE
        string[] allRegisteredFolders = ModifyAppSettings.ReadRegisteredMusicFolders();
        var allMusicNames = new List<string>();
        foreach (string folder in allRegisteredFolders)
        {
            string[] files = Directory.GetFiles(folder);

            for (int i = 0; i < files.Length; i++)
            {
                string fileName = Path.GetFileName(files[i]);
                allMusicNames.Add(fileName);
            }
        }

        return allMusicNames.ToArray();
    }
    public static string[,] GetSQLiteSongsData(SQLiteConnection connection)
    {
        int rowCount = 0;

        string countSyntax = $"SELECT COUNT(*) FROM allMusic";

        using (var countCmd = new SQLiteCommand(countSyntax, connection))
        {
            rowCount = Convert.ToInt32(countCmd.ExecuteScalar());
        }

        // Initialize the 3D array: [rows, 1, 3]
        string[,] resultArray = new string[rowCount, 4];

        int currentIndex = 0;
        string syntax = $"SELECT songID, name, uploadfile, absolutepath FROM allMusic";

        using (var command = new SQLiteCommand(syntax, connection))
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                // Store each value in the array
                resultArray[currentIndex, 0] = reader.IsDBNull(0) ? "" : reader.GetString(0); // songID
                resultArray[currentIndex, 1] = reader.IsDBNull(1) ? "" : reader.GetString(1); // name
                resultArray[currentIndex, 2] = reader.IsDBNull(2) ? "" : reader.GetInt16(2).ToString(); // uploadfile
                resultArray[currentIndex, 3] = reader.IsDBNull(1) ? "" : reader.GetString(3); // absolutepath

                currentIndex++;
            }
        }

        return resultArray;
    }
    public static SQLiteConnection OpenSQLiteConnection()
    {
        string databasePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\DeviceMusicData.db"));

        if (!File.Exists(databasePath))
        {
            var directory = Path.GetDirectoryName(databasePath);
            if (!Directory.Exists(directory) && directory != null)
            {
                Directory.CreateDirectory(directory);
            }
            SQLiteConnection.CreateFile(databasePath);
        }

        string path = $@"Data Source={databasePath}";
        var connection = new SQLiteConnection(path);
        connection.Open();
        return connection;
    }
}