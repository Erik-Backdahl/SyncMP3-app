using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
class StartUpAction
{
    public static async Task AllStartUp()
    {
        await TrySendPing();
    }
    public static async Task<string> TrySendPing()
    {
        var client = new HttpClient();
        var request = ParseHTTP.HTTPRequestFormat("GET", "/ping");
        try
        {
            request.Headers.Add("UUID", ModifyAppSettings.GetUUID());

            var response = client.SendAsync(request).Result;

            var (headers, message) = await ParseHTTP.GetResponseHeadersAndMessage(response);

            string fullMessage = "connected. messages:\n";

            if (message.StartsWith("success"))
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    fullMessage += header + ";\n";
                }
                return fullMessage;

            }
            else
            {
                return fullMessage;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "ping failed";
        }

    }
}