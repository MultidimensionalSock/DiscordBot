using System.Data.SQLite;
using System;

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
        Log(LogType.Request, "database created", "database check");
    }

    public static void Log(LogType logType, string source, string logDescription)
    {
        string query = @"
            INSERT INTO Logs (LogType, Source, LogDescription, LogCreatedTime)
            VALUES (@LogType, @Source, @LogDescription, datetime());";

        using (SQLiteCommand command = new SQLiteCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@LogType", (int)logType);
            command.Parameters.AddWithValue("@Source", source);
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
        

        string readAllLogs = "Select * from Logs;";
        var command2 = new SQLiteCommand(readAllLogs, _connection);
        Console.WriteLine(command2.ExecuteReader().ToString());

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

