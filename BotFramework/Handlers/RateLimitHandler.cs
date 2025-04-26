using BotFramework.Handlers;
using Logging;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotFramework
{
    public class RateLimitHandler : DatabaseTable
    {
        private static RateLimitHandler _instance = null; 
        public static RateLimitHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new();
                }
                return Instance;
            }
        }

        private SQLiteConnection _connection;
        public bool GlobalRateLimitReached => requestsInLast60Seconds > 60;
        private int requestsInLast60Seconds;
        private List<HTTPRequest> _activeRequests;
        

        public void AddRequest(HTTPRequest request)
        {
            _activeRequests.Add(request);
            RequestCounter();
        }
        /// <summary> Called to run start up proceedures for the class handling logging. </summary>
        private RateLimitHandler()
        {
            _connection = ConnectToDatabase("BotDatabase");
            CreateTable();
        }

        public void LimitExceeded(HttpResponseMessage response, HttpRequestMessage requestInfo)
        {

        }

        public bool EndpointRateLimitReached(Uri requestUri)
        {
            //check rate limit logs 
            //check if endpoint has been called before and get bucket info 
            return true;
        }

        public void ReadRateLimitInformation()
        {

        }

        private async Task RequestCounter()
        {
            requestsInLast60Seconds++;
            await Task.Delay(60000);
            requestsInLast60Seconds--;
        }

        public void CreateTable()
        {
            string LogTable = @"
                    CREATE TABLE IF NOT EXISTS RateLimits (
                    Limit INTEGER NOT NULL,
                    Remaining INTEGER NOT NULL, 
                    Reset TEXT NOT NULL,
                    ResetAfter INT NOT NULL, 
                    Bucket TEXT NOT NULL
                    Global BOOL, 
                    Scope TEXT NOT NULL
                );"
            ;

            try
            {
                SQLiteCommand command = new SQLiteCommand(LogTable, _connection);
                command.ExecuteNonQuery();
                Log.Debug("Database Created");
                Console.WriteLine("Database table created");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
