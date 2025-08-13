using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


class EndPoints
{
    public static async Task<(string?, string?)> CreateDataBase()
    {
        try
        {
            var client = new HttpClient();
            var request = ParseHTTP.HTTPRequestFormat("GET", "/create-network");

            request.Headers.Add("UUID", ModifyAppSettings.GetUUID());

            var response = client.SendAsync(request).Result;

            var (headers, message) = await ParseHTTP.GetResponseHeadersAndMessage(response);

            headers.TryGetValue("X-passKey", out var passKey);
            headers.TryGetValue("X-GUID", out var GUID);

            if (GUID != null)
                ModifyAppSettings.RegisterGUID(GUID);

            return (passKey, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return (ex.Message, "Error: Likely failed to connect to server");
        }
    }
    public static string AddAllSongToClientDataBase()
    {
        string[] allFolders = ModifyAppSettings.ReadAllFolderLocations();
        if (allFolders[0] == null)
            return "Failed No folders are registerd to search";

        foreach (string musicFolder in allFolders)
            UserDatabase.AddAllMusicNotInDatabase(musicFolder);

        return "";
    }

    public static async Task<string> SendSQLToServer()
    {
        try
        {
            AddAllSongToClientDataBase();
            string data = UserDatabase.ConvertDatabaseToJSON();

            var client = new HttpClient();
            var request = ParseHTTP.HTTPRequestFormat("POST", "/compare");

            request.Headers.Add("UUID", ModifyAppSettings.GetUUID());
            request.Headers.Add("GUID", ModifyAppSettings.GetGUID());

            var content = new StringContent(data, Encoding.UTF8, "application/json");

            request.Content = content;

            var response = client.SendAsync(request).Result;

            var (headers, message) = await ParseHTTP.GetResponseHeadersAndMessage(response);


            return message;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return ex.Message;
        }
    }

    internal static async Task<(string? message, string? code)> RequestNewPassKey()
    {
        try
        {
            var client = new HttpClient();
            var request = ParseHTTP.HTTPRequestFormat("POST", "/generate-passkey");

            request.Headers.Add("UUID", ModifyAppSettings.GetUUID());
            request.Headers.Add("GUID", ModifyAppSettings.GetGUID());

            var response = client.SendAsync(request).Result;

            var (header, message) = await ParseHTTP.GetResponseHeadersAndMessage(response);

            header.TryGetValue("X-passKey", out var passKey);

            return (message, passKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return (ex.Message, "");
        }
    }
    internal static async Task<string> TryJoinNetWork(string passKey)
    {
        if (!string.IsNullOrEmpty(ModifyAppSettings.GetGUID()))
            return "Already apart of a network";

        var client = new HttpClient();
        var request = ParseHTTP.HTTPRequestFormat("POST", "/add-user");


        request.Headers.Add("UUID", passKey);
        request.Headers.Add("passKey", passKey);

        //request.Headers.Add();

        return "";
    }
}