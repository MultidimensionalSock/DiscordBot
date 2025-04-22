using System.Data.SQLite;
using System.Runtime.CompilerServices;

class Log
{
    private static SQLiteConnection _connection;
    private const string _databaseName = "DiscordBotDatabase";
    private EmailSender _emailSender;

    public Log()
    {
        _connection = ConnectToDatabase(_databaseName);
        CreateLogTable();
        _emailSender = new EmailSender();
        _emailSender.SendEmail("test", "testifthisisworking", "dbotham16@outlook.com");
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
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Level INTEGER NOT NULL,
            Class TEXT NOT NULL, 
            Function TEXT NOT NULL,
            Message TEXT NOT NULL, 
            Payload TEXT, 
            Time TEXT NOT NULL
        );";

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

    public static void Fatal(string message, string payload = "", [CallerFilePath] string filePath = "", [CallerMemberName] string function = "") =>
     CreateLog(LogType.Fatal, filePath, function, message, payload);


    public static void Error(string message, string payload = "", [CallerFilePath] string filePath = "", [CallerMemberName] string function = "") =>
         CreateLog(LogType.Error, filePath, function, message, payload);

    public static void Warn(string message, string payload = "", [CallerFilePath] string filePath = "", [CallerMemberName] string function = "") =>
         CreateLog(LogType.Warn, filePath, function, message, payload);


    public static void Info(string message, string payload = "", [CallerFilePath] string filePath = "", [CallerMemberName] string function = "") =>
         CreateLog(LogType.Info, filePath, function, message, payload);


    public static void Debug(string message, string payload = "", [CallerFilePath] string filePath = "", [CallerMemberName] string function = "") =>
         CreateLog(LogType.Debug, filePath, function, message, payload);


    public static void Trace(string message, string payload = "", [CallerFilePath] string filePath = "", [CallerMemberName] string function = "") =>
         CreateLog(LogType.Trace, filePath, function, message, payload);

    private static string GetClassName(string filePath)
    {
        int index = filePath.LastIndexOf("\\") + 1;
        return filePath.Substring(index, filePath.LastIndexOf(".") - index);
    }

    public static void CreateLog(LogType logType, string classFilepath, string function, string message, string payload = "")
    {
        string query = @"
            INSERT INTO Logs (Level, Class, Function, Message, Payload, Time)
            VALUES (@Level, @Class, @Function, @Message, @Payload, datetime());";

        using (SQLiteCommand command = new SQLiteCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@Level", (int)logType);
            command.Parameters.AddWithValue("@Class", GetClassName(classFilepath));
            command.Parameters.AddWithValue("@Function", function);
            command.Parameters.AddWithValue("@Message", message);
            command.Parameters.AddWithValue("@Payload", payload);

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

    public static SQLiteDataReader ReadSQL(string query, params (string paramName, object value)[] parameters)
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


}

public enum LogType
{
    Fatal = 0,
    Error = 1,
    Warn = 2,
    Info = 3,
    Debug = 4,
    Trace = 5
}

