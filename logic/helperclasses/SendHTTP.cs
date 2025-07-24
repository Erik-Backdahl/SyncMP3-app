
using System.Net.Http;

class SendHTTP
{
    public static string baseAddress = "http://localhost:4221";
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