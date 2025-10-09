using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using TagLib.Mpeg4;
using System.Collections.Generic;
using System.Threading;

class EndPoints
{
    public static async Task<(string?, string?)> CreateDataBase()
    {
        try
        {
            var client = new HttpClient();
            var request = ParseHTTP.HTTPRequestFormat("GET", "/create-network");

            request.Headers.Add("UUID", ModifyAppSettings.GetUUID());

            var response = await client.SendAsync(request);

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
    public static async Task<(Dictionary<string, string>, string)> SendSQLToServer()
    {
        await UserDatabase.AddAllMusicNotInDatabase();
        await UserDatabase.DeleteEmptyEntiresInDatabase();
        string data = UserDatabase.ConvertDatabaseToJSON();

        var client = new HttpClient();
        var request = ParseHTTP.HTTPRequestFormat("POST", "/compare");

        request.Headers.Add("UUID", ModifyAppSettings.GetUUID());
        request.Headers.Add("GUID", ModifyAppSettings.GetGUID());

        var content = new StringContent(data, Encoding.UTF8, "application/json");

        request.Content = content;

        var response = await client.SendAsync(request);

        var (headers, message) = await ParseHTTP.GetResponseHeadersAndMessage(response);

        return (headers, message);
    }
    public static async Task<(bool, string)> RequestAndReceiveMusic(string songID)
    {
        try
        {

            var client = new HttpClient();
            var request = ParseHTTP.HTTPRequestFormat("GET", "/get-music");

            request.Headers.Add("UUID", ModifyAppSettings.GetUUID());
            request.Headers.Add("GUID", ModifyAppSettings.GetGUID());
            request.Headers.Add("songID", songID);

            var response = await client.SendAsync(request);
            var (headers, songBytes) = await ParseHTTP.GetSongHeadersAndSongBytes(response);

            headers.TryGetValue("X-songName", out string? unescapedSongName);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return (false, "Error. Song not currently downloaded but a request has been scheduled");
            }
            else
            {
                string songName = Uri.UnescapeDataString(unescapedSongName ?? throw new Exception("Recivied song has no name"));
                string savePath = Path.Combine(ModifyAppSettings.ReadDownloadFolder(), songName);

                System.IO.File.WriteAllBytes(savePath, songBytes);

                return (true, $"success {songName} downloaded");


            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return (false, "Error" + ex.Message);
        }
    }
    public static async Task<string> SendMusicToServer(List<string> requestedMusicIDsList)
    {
        string[] requestedMusicIDs = requestedMusicIDsList.ToArray();
        try
        {
            var connection = UserDatabase.OpenSQLiteConnection();
            string[,] allMusicData = UserDatabase.GetSQLiteSongsData(connection);

            var client = new HttpClient();

            string fullAdress = ParseHTTP.baseAddress + "/upload";

            int uploadSuccess = 0;
            int failedUpload = 0;

            for (int k = 0; k < requestedMusicIDs.Length; k++)
            {
                for (int i = 0; i < allMusicData.GetLength(0); i++)
                {
                    if (allMusicData[i, 2] == "1" && allMusicData[i, 0] == requestedMusicIDs[k])
                    {
                        string filePath = Path.Combine(allMusicData[i, 3], allMusicData[i, 1]);

                        if (!System.IO.File.Exists(filePath))
                        {
                            Console.WriteLine($"File not found: {filePath}");
                            continue;
                        }

                        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                        using var content = new StreamContent(fileStream);

                        content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
                        content.Headers.Add("UUID", ModifyAppSettings.GetUUID());
                        content.Headers.Add("GUID", ModifyAppSettings.GetGUID());
                        content.Headers.Add("songID", allMusicData[i, 0]);

                        string songName = allMusicData[i, 1];
                        string encoded = Uri.EscapeDataString(songName);

                        content.Headers.Add("SongName", encoded);

                        var response = await client.PostAsync(fullAdress, content);
                        var (headers, message) = await ParseHTTP.GetResponseHeadersAndMessage(response);

                        if (response.IsSuccessStatusCode)
                        {
                            uploadSuccess++;
                        }
                        else
                        {
                            failedUpload++;
                            Console.WriteLine($"{requestedMusicIDs[k]} failed to upload");
                        }

                    }
                }
            }



        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        throw new NotImplementedException();
    }

    internal static async Task<(string? message, string? code)> RequestNewPassKey()
    {
        try
        {
            var client = new HttpClient();
            var request = ParseHTTP.HTTPRequestFormat("POST", "/generate-passkey");

            request.Headers.Add("UUID", ModifyAppSettings.GetUUID());
            request.Headers.Add("GUID", ModifyAppSettings.GetGUID());

            var response = await client.SendAsync(request);

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
    internal static async Task<string> TryJoinNetwork(string passKey)
    {
        if (!string.IsNullOrEmpty(ModifyAppSettings.GetGUID()))
            return "Already apart of a network";

        var client = new HttpClient();
        var request = ParseHTTP.HTTPRequestFormat("POST", "/add-user");

        request.Headers.Add("UUID", ModifyAppSettings.GetUUID());
        request.Headers.Add("passKey", passKey);

        var response = await client.SendAsync(request);

        var (header, message) = await ParseHTTP.GetResponseHeadersAndMessage(response);

        if (response.IsSuccessStatusCode && header.TryGetValue("X-GUID", out var GUID))
        {
            ModifyAppSettings.RegisterGUID(GUID);
            return message;
        }
        else
        {
            return message;
        }
    }
    public static async Task<(string, Dictionary<string, string>?)> TrySendPing()
    {
        var client = new HttpClient();
        var request = ParseHTTP.HTTPRequestFormat("GET", "/ping");
        try
        {
            request.Headers.Add("UUID", ModifyAppSettings.GetUUID());

            var response = await client.SendAsync(request);

            var (headers, message) = await ParseHTTP.GetResponseHeadersAndMessage(response);

            if (response.IsSuccessStatusCode)
            {
                string fullMessage = "connected. ";
                if (headers.Count > 0)
                {
                    fullMessage += headers.Count + "Message(s) ";
                    
                    return (fullMessage, headers);
                }
                else
                {
                    return (fullMessage + "No Messages ", headers);
                }
            }
            else
            {
                return ("ping failed", null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return ("ping failed", null);
        }

    }
}