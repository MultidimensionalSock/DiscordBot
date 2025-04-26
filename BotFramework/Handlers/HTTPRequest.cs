using Logging;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace BotFramework.Handlers
{
    public class HTTPRequest
    {
        private static HttpClient client;
        // Object data 
        public Guid RequestID { get; private set; }
        /// <summary> maximum time this request is alive for before it stops trying to resend, stored in seconds </summary>
        public float MaxWaitTime { get; private set; }
        public int MaxRetryAttempts { get; private set; }
        private int _retryAttempts = 0;
        public DateTime creationTime { get; private set; }
        private DateTime _timeout;
        private string _filePath = "";
        private string _function = "";

        // HTTP Request Data 
        private HttpRequestMessage _request;
        public HttpResponseMessage Response = null;
        public bool Sent = false; //remove from handler tracker at this point? 

        private double timeRemaining => MaxWaitTime - (DateTime.Now - creationTime).TotalSeconds; 

        public HTTPRequest(HttpRequestMessage request, float maxWaitTime, int maxRetryAttempts, [CallerFilePath] string filePath = "", [CallerMemberName] string function = "")
        {
            RequestID = Guid.NewGuid();
            MaxWaitTime = maxWaitTime;
            MaxRetryAttempts = maxRetryAttempts;
            creationTime = DateTime.Now;
            _timeout = DateTime.Now.AddSeconds(maxWaitTime);
            _filePath = filePath;
            _function = function;

            _request = request;

            if (client == null)
                client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
        }

        public static HttpRequestMessage CreateHTTPMessage(HttpMethod method, string url, StringContent? body = null, Header[]? headers = null)
        {
            var request = new HttpRequestMessage(method, url);
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
            return request;
        }

        /// <summary> loop to handle sending of request until max send limit or timeout </summary>
        public async Task<HttpResponseMessage> SendHTTPMessage()
        {
            do
            {
                if (RateLimitHandler.Instance.GlobalRateLimitReached)
                {
                    if (timeRemaining > 60)
                    {
                        Task.Delay(60000); // wait 60 seconds until global rate limit reset, if within time limit
                    }
                    else
                    {
                        Log.Warn("Global Rate Limit reached, request cancelled" + _request.RequestUri.ToString(), "", _filePath, _function);
                        return CreateResponseMessage();
                    }
                }

                (bool rateLimitReached, DateTime resetTime) rateLimitInfo = RateLimitHandler.Instance.EndpointRateLimitReached(_request);
                if (rateLimitInfo.rateLimitReached)
                {
                    if (DateTime.Compare(_timeout, rateLimitInfo.resetTime) < 0)
                    {
                        Task.Delay((int)(rateLimitInfo.resetTime - DateTime.Now).TotalMilliseconds); //wait until the endpoint limit reset
                    }
                    else
                    {
                        Log.Warn("Endpoint Rate Limit reached, request cancelled" + _request.RequestUri.ToString(), "", _filePath, _function);
                        return CreateResponseMessage();
                    }
                }

                Response = await TrySend();

                if (Response.IsSuccessStatusCode) //range 200-299
                {
                    Log.Trace("Successful request", Response.Content.ReadAsStringAsync().ToString(), _filePath, _function);
                    return Response; 
                }

                //decides what to do if the response code is not successful
                switch(Response.StatusCode)
                {
                    //do not retry - request is incorrect
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.MethodNotAllowed:
                    case HttpStatusCode.Conflict:
                    case HttpStatusCode.UnprocessableEntity:
                    case HttpStatusCode.PreconditionFailed:
                    case HttpStatusCode.RequestUriTooLong:
                    case HttpStatusCode.UnsupportedMediaType:
                        Log.Error("Issue with request", Response.Content.ReadAsStringAsync().ToString(), _filePath, _function);
                        Sent = true;
                        return Response;

                    //retry
                    case HttpStatusCode.RequestTimeout:
                    case HttpStatusCode.TooManyRequests:
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.GatewayTimeout:
                    case HttpStatusCode.LengthRequired:
                        Log.Warn("Retry Initiated", Response.Content.ReadAsStringAsync().ToString(), _filePath, _function);
                        break;
                    

                    //redirect 
                    case HttpStatusCode.MovedPermanently:
                    case HttpStatusCode.PermanentRedirect:
                    case HttpStatusCode.Found:
                    case HttpStatusCode.SeeOther:
                    case HttpStatusCode.TemporaryRedirect:
                        Log.Warn("Redirect Initiated", Response.Content.ReadAsStringAsync().ToString(), _filePath, _function);
                        Redirect();
                        break;

                    default:
                        Sent = true; 
                        return Response;
                }
            } while (!Sent && MaxRetryAttempts != _retryAttempts && timeRemaining > 0);
            
            if (MaxRetryAttempts >= _retryAttempts) 
                Log.Warn("Max Attempts Reached", Response.ToString(), _filePath, _function);
            else if (timeRemaining > 0) 
                Log.Warn("Timeout Reached", Response.ToString(), _filePath, _function);

            return Response;
        }

        private void Redirect()
        {
            //if URI change 
            _request.RequestUri = Response.Headers.Location;
            //TODO: length required - Server rejected the request because the Content-Length header field is not defined and the server requires it.
        }

        private async Task<HttpResponseMessage> TrySend() 
        {
            RateLimitHandler.Instance.AddRequest(this);
            _retryAttempts++;
            return await client.SendAsync(_request);
        }

        /// <summary>
        /// Creates a repsonse message if none is reached in max wait time
        /// </summary>
        public HttpResponseMessage CreateResponseMessage() 
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.TooManyRequests;
            return response;  
        }

        /// <summary> kill reuqest immeditely </summary>
        public void Kill() { }
    }

    public class HTTPRequestException : Exception { }

    public interface HTTPEnabled
    {
        public void AcceptHTTPResponse(HttpResponseMessage response, HttpRequestMessage request) { }
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
