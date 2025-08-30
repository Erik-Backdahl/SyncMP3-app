using System;
using System.Data.SQLite;
using System.IO;

class CreateEssentialFiles
{
    public static void CheckEssentialFiles()
    {
        try
        {
            TryCreateEmptyMessagesJson();
            TryCreateEmptyAppSettingsJson();
            TryCreateEmptySQLiteDatabase();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private static void TryCreateEmptyMessagesJson()
    {
        var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\Messages.json"));
    }
    private static void TryCreateEmptyAppSettingsJson()
    {
        var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\AppSettings.json"));
    }
    private static void TryCreateEmptySQLiteDatabase()
    {
        var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\DeviceMusicData.db"));

        var connection = UserDatabase.OpenSQLiteConnection();
        CreateDatabaseTables(connection);


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