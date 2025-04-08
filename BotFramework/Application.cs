using BotFramework.Commands;
using BotFramework.Structures;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;

namespace BotFramework
{
    class Application
    {
        public BotDetails? BotDetails { get; set; }
        public static Int64 Id;
        public static string? Token;
        public static Log Logger;

        public static Application? BotRef;
        public HTTPHandler? HttpHandler;
        public GatewayHandler? GatewayHandler;

        public static List<Command> BotCommands = new();

        public Application()
        {
            if (BotRef != null) return;
            string jsonString = File.ReadAllText("Data/ApplicationInfo.json");
            JsonDocument info = JsonDocument.Parse(jsonString);
            Int64.TryParse(info.RootElement.GetProperty("ClientId").ToString(), out Id);
            Token = info.RootElement.GetProperty("Token").GetString();


            BotRef = this;
            HttpClientHandler handler = new HttpClientHandler { AllowAutoRedirect = true };
            HttpClient httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
            ClientWebSocket webSocketClient = new ClientWebSocket();

            HttpHandler = new HTTPHandler(httpClient);
            GatewayHandler = new GatewayHandler(webSocketClient);

            GatewayHandler?.Connect();

            Logger = new Log();
        }

        public async Task AddCommands()
        {
            List<CommandData>? commandData = new List<CommandData>();
            HttpResponseMessage? response = null;

            try
            {
                response = await HTTPHandler.SendRequest(HttpMethod.Get,
                    $"https://discord.com/api/v10/applications/{Id}/commands", null);
                if (response.IsSuccessStatusCode)
                {
                    using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    commandData = JsonSerializer.Deserialize<List<CommandData>>(doc);
                }
            }
            catch (HttpRequestException e)
            {
                Log.Error(e.Message);
            }

            foreach (Command command in BotCommands)
            {
                if (commandData?.Count <= 0)
                {
                    response = await command.AddCommand(command);
                }
                else
                {
                    //this isnt comparing properly
                    CommandData data = commandData.Find(cd => String.Equals(cd.Name, command.Name));
                    if (data is null)
                    {
                        response = await command.AddCommand(command);
                    }
                    else if (data == command)
                    {
                        response = await Command.GetCommand(data.Id);
                    }
                    else
                    {
                        //await command.UpdateCommand(data.id);
                    }
                }
                if (response.IsSuccessStatusCode)
                {
                    using JsonDocument doc = JsonDocument.Parse(await response!.Content.ReadAsStringAsync());
                    command.Id = doc.RootElement.GetProperty("id").GetString();
                }
            }
        }

        public async Task InitializeCommands()
        {
            Type[] commands = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Command)) && !t.IsAbstract).ToArray();

            foreach (Type t in commands)
            {
                BotCommands.Add(Activator.CreateInstance(t) as Command);
            }

            await AddCommands();
        }
    }
}
