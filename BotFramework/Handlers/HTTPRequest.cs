using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotFramework.Handlers
{
    public class HTTPRequest
    {
        public Guid RequestID { get; private set; } 
        private HttpRequestMessage _request;
        public HttpResponseMessage Response = null;
        /// <summary> maximum time this request is alive for before it stops trying to resend </summary>
        public float MaxWaitTime;
        public int MaxRetryAttempts { get; private set; }
        private int _retryAttempts;
        public DateTime creationTime { get; private set; }
        private HTTPEnabled _owner;
        public bool Sent = false; 


        public HTTPRequest(HttpMethod httpMethod, string url, StringContent? body = null, Header[]? headers = null)
        {
            var request = new HttpRequestMessage(httpMethod, url);
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
            _request = request;
        }

        public async HttpResponseMessage SendHTTPMessage()
        {
            do
            {
                if (RateLimitHandler.Instance.GlobalRateLimitReached)
                    //wait 60 seconds if it doesnt put over max wait time return if so
                if (RateLimitHandler.Instance.EndpointRateLimitReached(_request.RequestUri)
                        //see when rate limit runs out and if it falls into max time limit return if so
                RateLimitHandler.Instance.AddRequest(this);
                Response = TrySend();
                if (Response.IsSuccessStatusCode)
                {
                    Log.Trace("Successful reuqest");
                    //add trace log here
                    return Response; 
                }
                switch(Response.StatusCode)
                {

                }
                //if successful return to sender, set sent to true
                //if not successful, check if can be resent, if not, break out of while
                //collect rate limiting info and send to rate limiting handler
            } while (!Sent && MaxRetryAttempts != _retryAttempts);
            return Response;
        }

        public HttpResponseMessage TrySend() { return null; }

        /// <summary>
        /// Creates a HTTPRequest
        /// </summary>
        /// <returns></returns>
        public static HttpRequestMessage CreateHTTPRequest()
        {
            return null;
        }

        /// <summary>
        /// Creates a repsonse message if none is reached in max wait time
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage CreateResponseMessage() { return null;  }

        /// <summary>
        /// kill reuqest immeditely
        /// </summary>
        public void Kill() { }
    }

    public class HTTPRequestException : Exception { }

    public interface HTTPEnabled
    {
        public void AcceptHTTPResponse(HttpResponseMessage response, HttpRequestMessage request) { }
    }

}
