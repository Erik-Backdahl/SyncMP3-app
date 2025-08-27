
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

class ParseHTTP
{
    public static string baseAddress = "http://192.168.0.33:4221";
    public static HttpRequestMessage HTTPRequestFormat(string requestType, string path)
    {
        HttpMethod HttpType;

        //Determine request type
        if (requestType == "GET")
            HttpType = HttpMethod.Get;
        else if (requestType == "POST")
            HttpType = HttpMethod.Post;
        else
            HttpType = HttpMethod.Get;

        HttpRequestMessage request = new(HttpType, $"{baseAddress}" + $"{path}");

        return request;
    }
    public static async Task<(Dictionary<string, string>, string message)> GetResponseHeadersAndMessage(HttpResponseMessage serverResponse)
    {
        var headers = new Dictionary<string, string>();
        string message = "";

        foreach (var header in serverResponse.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        if (serverResponse.Content != null)
        {
            message = await serverResponse.Content.ReadAsStringAsync();
        }

        return (headers, message);
    }
}