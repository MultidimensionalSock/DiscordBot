﻿using System.Net.WebSockets;
using Magic8.Structures;
using Magic8.Commands;
using System.Text.Json;
using System.Reflection;

namespace Magic8
{
    class Application
    {
        public BotDetails? BotDetails { get; set; }
        public static Int64 Id;
        public static string Token;

        public static Application? BotRef;
        public HTTPHandler HttpHandler;
        public GatewayHandler GatewayHandler;

        public static List<Command> BotCommands = new(); 

        public Application()
        {
            if (BotRef != null) return;
            string jsonString = File.ReadAllText("ApplicationInfo.json");
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
            
        }

        // add commands isnt working correctly 
        public async Task AddCommands()
        {
            List<CommandData> commandData = new List<CommandData>();
            HttpResponseMessage response = null;

            try
            {
                Console.WriteLine("making html reuqest");
                response = await HTTPHandler.SendRequest(HttpMethod.Get, 
                    $"https://discord.com/api/v10/applications/{Id}/commands", null);
                Console.WriteLine(response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.Content);
                    //parsing is not working correctly 
                    using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    //using JsonDocument doc = JsonDocument.Parse(response.Content.ToString());
                    commandData = JsonSerializer.Deserialize<List<CommandData>>(doc);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
            }

            foreach (Command command in BotCommands) 
            {
                if (commandData.Count <= 0)
                {
                    response = await command.AddCommand(command);
                }
                else
                {
                    //this is being set as null
                    CommandData data = commandData.Find(cd => cd.Name == command.Name);
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
                using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                command.Id = doc.RootElement.GetProperty("id").GetString();
            }
        }

        public void InitializeCommands()
        {
            Type[] commands = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Command))).ToArray();

            foreach (Type t in commands)
            {
                BotCommands.Add(Activator.CreateInstance(t) as Command);
            }

            AddCommands();
        }

        public void RunCommand()
        {
            //needs to be done from events
        }
    }
}
