class ResponseHTTP
{
    public static string FormatResponse(string requestOperation, string message, string requestContentType = "text/plain")
    {
        string responseCode = FormatCodeOperation(requestOperation);
        string responseContentType = FormatContentType(requestContentType);

        string responseTemplate =
        $"{responseCode}" +
        $"{responseContentType}" +
        $"Content-Length: {message.Length}\r\n\r\n" +
        $"{message}";

        return responseTemplate;

    }
    public static string FormatEasyResponse(string requestOperation)
    {
        string responseCode; //make this a switch case dumbfuck

        if (requestOperation == "200")
            responseCode = "HTTP/1.1 200 OK\r\n\r\n";
        else if (requestOperation == "201")
            responseCode = "HTTP/1.1 201 Created\r\n\r\n";
        else if (requestOperation == "400")
            responseCode = "HTTP/1.1 400 Bad Request\r\n\r\n";
        else if (requestOperation == "404")
            responseCode = "HTTP/1.1 404 Not Found\r\n\r\n";
        else if (requestOperation == "500")
            responseCode = "HTTPS/1.1 500 Internal Server Error\r\n\r\n";
        else
            responseCode = "HTTP/1.1 404 Not Found\r\n\r\n";

        return responseCode;
    }
    public static string FormatCodeOperation(string requestOperation)
    {
        string responseCode; //make this a switch case dumbfuck

        if (requestOperation == "200")
            responseCode = "HTTP/1.1 200 OK\r\n";
        else if (requestOperation == "201")
            responseCode = "HTTP/1.1 201 Created\r\n\r\n";
        else if (requestOperation == "400")
            responseCode = "HTTP/1.1 400 Bad Request\r\n";
        else if (requestOperation == "500")
            responseCode = "HTTP/1.1 500 Internal Server Error";
        else
            responseCode = "HTTP/1.1 404 Not Found\r\n";

        return responseCode;
    }
    public static string FormatContentType(string contentType)
    {
        string responseContentType; //make this a switch case dumbfuck
        if (contentType == "text/plain")
            responseContentType = "Content-Type: text/plain\r\n";
        else if (contentType == "audio/mpeg")
        {
            responseContentType = "Content-Type: audio/mpeg\r\n";
        }
        else
            responseContentType = "Content-Type: application/octet-stream\r\n";

        return responseContentType;
    }

}