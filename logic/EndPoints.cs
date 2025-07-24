using System;
using System.Net.Http;
using System.Text.Json;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;


class EndPoints
{
    public static string CreateDataBase()
    {
        var client = new HttpClient();
        var request = SendHTTP.HTTPRequestFormat("GET", "/create-network");

        request.Headers.Add("UUID", AppSettings.GetUUID());

        var response = client.SendAsync(request).Result;

        string responseBody = response.Content.ReadAsStringAsync().Result;

        System.Console.WriteLine(responseBody);


        //Generate a UUID save in appsettings
        //check if connected to internet
        //ping server to see its online
        //request server to create family specific table in server
        //if successful it respondes with a GUID. 
        //GUID is saved in AppSettings.json ????? maybe 
        //give access to network tab in app.

        return "";
    }
    public static string AddAllSongToDataBase()
    {
        string[] allFolders = AppSettings.ReadAllFolderLocations();
        if (allFolders[0] == null)
            return "Failed No folders are registerd to search";

        foreach (string musicFolder in allFolders)
            UserDatabase.AddAllMusicNotInDatabase(musicFolder);
        return "";
    }

    public static string SendSQLToServer()
    {
        try
        {
            string data = UserDatabase.ConvertDatabaseToJSON();

            var client = new HttpClient();
            var request = SendHTTP.HTTPRequestFormat("POST", "/compare");

            request.Headers.Add("UUID", AppSettings.GetUUID());
            request.Headers.Add("GUID", AppSettings.GetGUID());

            var content = new StringContent(data, Encoding.UTF8, "application/json");

            request.Content = content;

            var response = client.SendAsync(request).Result;

            string lol = response.Content.ReadAsStringAsync().ToString();

            Console.WriteLine(lol);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return "";
    }
}