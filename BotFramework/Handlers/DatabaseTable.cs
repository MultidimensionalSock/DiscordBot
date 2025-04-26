using Logging;
using Mysqlx.Notice;
using System.Data.SQLite;

namespace BotFramework.Handlers
{
    public class DatabaseTable
    {
        protected static SQLiteConnection _connection;
        /// <summary>
        /// Called to run start up proceedures for the class handling logging. 
        /// </summary>
        public void DatbaseTable()
        {
            _connection = ConnectToDatabase("BotDatabase");
            CreateTable();
        }

        protected SQLiteConnection ConnectToDatabase(string databaseName)
        {
            SQLiteConnection connection = new SQLiteConnection($"Data Source={databaseName}");

            try
            {
                connection.Open();
                Console.WriteLine("Database connection Successful");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return null;
        }

        protected virtual void CreateTable() { Log.Warn("Create table has not been implemented for this class"); }


        /// <summary>
        /// Search for logs
        /// </summary>
        public object Search(string query, params (string paramName, object value)[] parameters)
        {
            SQLiteDataReader reader = null;
            using (SQLiteCommand command = new SQLiteCommand(query, _connection))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.paramName, param.value);
                    }
                }
                try
                {
                    reader = command.ExecuteReader();
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message);
                }
            }
            return reader;
        }

        /// <summary>
        /// delete log type/s within a set time frame
        /// </summary>
        public void Delete(DateTime timeFrame, params LogType[] logType) { }
    }
}

