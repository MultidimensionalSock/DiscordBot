using System.Net.Http.Headers;
using System.Collections;

namespace Magic8
{
    public class HTTPHandler
    {
        public static HttpClient? HttpClient { get; private set; }
        public static HTTPHandler HttpHandler;
        private static int requestsInLast60Seconds;

        public HTTPHandler(HttpClient httpClient)
        {
            HttpClient = httpClient;
            HttpHandler = this; 
        }

        public static async Task<HttpResponseMessage> SendRequest(HttpMethod httpMethod, string Url, StringContent? body = null, Header[]? headers = null)
        {
            if (requestsInLast60Seconds >= 60) { return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.TooManyRequests }; }
            var request = new HttpRequestMessage(httpMethod, Url);
            request.Headers.Add("User-Agent", $"DiscordBot (DiscordBot (https://discord.com/oauth2/authorize?client_id={Application.Id} 1.0.0), 1.0)");
            request.Headers.Add("Authorization", $"Bot {Application.Token}");
            if (headers != null)
            {
                foreach (Header header in headers)
                {
                    request.Headers.Add(header.Name, header.Value);
                }
            }
            if (body != null)
            {
                request.Content = body;
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            HttpResponseMessage response = await HttpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Code: {response.StatusCode}");
            Console.WriteLine($"Response Body: {responseContent}");
            HTTPHandler.HttpHandler.RequestCounter();
            return response;
        }

        public static async Task<HttpResponseMessage> SendUnauthRequest(HttpMethod httpMethod, string Url, string? body = null, Header[]? headers = null)
        {
            if (requestsInLast60Seconds >= 60) { return null; }
            var request = new HttpRequestMessage(httpMethod, Url);
            if (headers != null)
            {
                foreach (Header header in headers)
                {
                    request.Headers.Add(header.Name, header.Value);
                }
            }
            if (body != null)
            {
                request.Content = new StringContent(body);
            }
            HTTPHandler.HttpHandler.RequestCounter();
            return await HttpClient.SendAsync(request);
        }

        private async Task RequestCounter()
        {
            requestsInLast60Seconds++;
            await Task.Delay(60000);
            requestsInLast60Seconds--;
        }

    }

    public struct Header
    {
        public string Name;
        public string Value;

        public Header(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
