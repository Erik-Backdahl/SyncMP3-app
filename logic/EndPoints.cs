using System;
using System.Net.Http;
using System.Text.Json;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;


class EndPoints
{
    public static async Task<(string?, string?)> CreateDataBase()
    {
        try
        {
            if (ModifyAppSettings.GetGUID() != string.Empty)
                return ("Already part of a Network", ""); 

            var client = new HttpClient();
            var request = ParseHTTP.HTTPRequestFormat("GET", "/create-network");

            request.Headers.Add("UUID", ModifyAppSettings.GetUUID());

            var response = client.SendAsync(request).Result;

            var (headers, message) = await ParseHTTP.GetResponseHeadersAndMessage(response);

            if (headers.TryGetValue("X-passKey", out var passKey))
                Console.WriteLine($"X-passKey: {passKey}");

            headers.TryGetValue("X-GUID", out var GUID);

            if (GUID != null)
                ModifyAppSettings.RegisterGUID(GUID);

            return (passKey, message);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }


    }
    public static string AddAllSongToDataBase()
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
}