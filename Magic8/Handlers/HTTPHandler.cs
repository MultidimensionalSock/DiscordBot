using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;

namespace Magic8
{
    public class HTTPHandler
    {
        public static HttpClient? HttpClient { get; private set; }

        public HTTPHandler(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public static async Task<HttpResponseMessage> SendRequest(HttpMethod httpMethod, string Url, StringContent? body = null, Header[]? headers = null)
        {
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
                //request.Content.Headers.ContentLength = Encoding.UTF8.GetByteCount(body);
            }
            HttpResponseMessage response = await HttpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Code: {response.StatusCode}");
            Console.WriteLine($"Response Body: {responseContent}");
            return response;
        }

        public static async Task<HttpResponseMessage> SendUnauthRequest(HttpMethod httpMethod, string Url, string? body = null, Header[]? headers = null)
        {
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
            return await HttpClient.SendAsync(request);
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
