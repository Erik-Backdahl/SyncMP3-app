using System;
using System.IO;
using System.Net.Http;
using Avalonia.OpenGL;
class StartUpAction
{
    public static void AllStartUp()
    {
        TrySendPing();
    }
    private static string TrySendPing()
    {
        var client = new HttpClient();
        var request = SendHTTP.HTTPRequestFormat("GET", "/ping");
        try
        {
            request.Headers.Add("UUID", AppSettings.GetUUID());
           
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return $"{ex}";
        }

        var response = client.SendAsync(request).Result;
        string responseBody = response.Content.ReadAsStringAsync().Result;

        return responseBody;
    }
}