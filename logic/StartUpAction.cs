using System.Net.Http;
class StartUpAction
{
    public static void AllStartUp()
    {
        TrySendPing();
    }
    public static string TrySendPing()
    {
        var client = new HttpClient();
        var request = SendHTTP.HTTPRequestFormat("GET", "/ping");

        var response = client.SendAsync(request).Result;
        string responseBody = response.Content.ReadAsStringAsync().Result;

        return responseBody;
    }
}