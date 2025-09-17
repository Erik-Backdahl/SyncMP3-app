using System;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Text.Json;
using System.Data;
using Tmds.DBus.Protocol;
class UserDatabase
{
    public static string ConvertDatabaseToJSON()
    {
        var connection = UserDatabase.OpenSQLiteConnection();

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
    public static string[,] AddAllMusicNotInDatabase(string folder)
    {
        using var connection = OpenSQLiteConnection();

        string[] musicFolder = GetFileNamesFromFolder(folder);

        string[,] musicAddedToDatabase = new string[musicFolder.Length, 2];

        for (int i = 0; i < musicFolder.Length; i++)
        {
            string songTag = ReadTag(folder, musicFolder[i]);

            if (songTag.StartsWith("Failed to find ID"))
            {//does not have ID give it one
                songTag = TagFile(folder, musicFolder[i]);
            }

            if (!FoundSongInDataBase(songTag, connection))
            {
                musicAddedToDatabase[i, 0] = musicFolder[i];
                musicAddedToDatabase[i, 1] = songTag;
                SaveInSQLite(folder, musicFolder[i], songTag, connection);
            }
            else
            {
                //VerifySQLiteEntry(musicFolder[i], folder, songTag);
                //TODO verify each song in the db eg: folderlocation has changed or name has changed 
            }
        }

        return musicAddedToDatabase;
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
    public static string TagFile(string folder, string fileName)
    {
        string path = $@"{folder}{fileName}";
        try
        {
            var file = TagLib.File.Create(path);

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
    public static string ReadTag(string folder, string fileName)
    {
        string filePath = folder + fileName;

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
    private static string[] GetFileNamesFromFolder(string folderPath)
    {// folderPath = C/stuff/music //NOT PATH TO A SINGLE FILE
        string[] files = Directory.GetFiles(folderPath);

        string[] fileData = new string[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            string fileName = Path.GetFileName(files[i]);
            fileData[i] = fileName;
        }

        return fileData;
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