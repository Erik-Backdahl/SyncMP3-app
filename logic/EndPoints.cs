using System.Linq;

class EndPoints
{
    
    public static string AddAllSongToDataBase()
    {
        string[] allFolders = ReadAppSettings.ReadAllFolderLocations();
        if (allFolders[0] == null)
            return "Failed No folders are registerd to search";

        foreach (string musicFolder in allFolders)
                UserDatabase.AddAllMusicNotInDatabase(musicFolder);
        return "";
    }

}