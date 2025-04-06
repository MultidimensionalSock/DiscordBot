using System.Data.SQLite;
using System;
using System.Runtime.CompilerServices;
using System.Data.Entity.Core.Metadata.Edm;

class Logger
{
    private static SQLiteConnection _connection;
    string DatabaseName = "DiscordBotDatabase";

    public Logger()
    {
        _connection = ConnectToDatabase(DatabaseName);
        CreateLogTable(); 
    }

    private SQLiteConnection ConnectToDatabase(string databaseName)
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

    private void CreateLogTable()
    {
        string LogTable = @"
            CREATE TABLE IF NOT EXISTS Logs (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            LogType INTEGER NOT NULL, 
            Source TEXT, 
            LogDescription TEXT NOT NULL,
            LogCreatedTime TEXT NOT NULL
        );";

        try
        {
            SQLiteCommand command = new SQLiteCommand(LogTable, _connection);
            command.ExecuteNonQuery();
            Console.WriteLine("Database table created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Log(LogType.Request, "database created");
    }

    //public static void Log(LogType logType, string source = "", string logDescription = "")
    //{
    //    string query = @"
    //        INSERT INTO Logs (LogType, Source, LogDescription, LogCreatedTime)
    //        VALUES (@LogType, @Source, @LogDescription, datetime());";

    //    using (SQLiteCommand command = new SQLiteCommand(query, _connection))
    //    {
    //        command.Parameters.AddWithValue("@LogType", (int)logType);
    //        command.Parameters.AddWithValue("@Source", source);
    //        command.Parameters.AddWithValue("@LogDescription", logDescription);

    //        try
    //        {
    //            command.ExecuteNonQuery();
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //        }
    //    }
    //}

    public static void Log(LogType logType, string logDescription, [CallerFilePath] string callerFilePath ="", [CallerMemberName] string source = "")
    {
        string query = @"
            INSERT INTO Logs (LogType, Source, LogDescription, LogCreatedTime)
            VALUES (@LogType, @Source, @LogDescription, datetime());";

        int index = callerFilePath.LastIndexOf("\\") + 1;
        callerFilePath = callerFilePath.Substring(index, callerFilePath.LastIndexOf(".") - index);

        using (SQLiteCommand command = new SQLiteCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@LogType", (int)logType);
            command.Parameters.AddWithValue("@Source", $"{callerFilePath}.{source}");
            command.Parameters.AddWithValue("@LogDescription", logDescription);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

public enum LogType
{
    Error = 0, 
    Request = 1, 
    Response = 2, 
    GatewayEvent = 3,
    BotAction = 4, 
    RateLimiting = 5
}

