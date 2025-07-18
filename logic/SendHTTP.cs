
using System.Net.Http;

class SendHTTP
{
    public static string baseAddress = "http://localhost:4221";
    public static string SendMusicData()
    {
        var client = new HttpClient();
        var request = HTTPRequestFormat("POST", "/Request-History");

        request.Headers.Add("UUID", "1ebbed86-48fc-4e5a-b253-d73a4970869c");
        request.Headers.Add("lastRequestID", "100");

        var response = client.SendAsync(request).Result;
        string responseBody = response.Content.ReadAsStringAsync().Result;

        return "";
    }

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
    
}