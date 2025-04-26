using BotFramework.Structures;
using System.Text;
using System.Text.Json;
using Logging;
using BotFramework.Handlers;

namespace BotFramework.Commands
{
    public class Command
    {
        public string? Id;
        public bool GuildCommand { get; private set; }
        public string Name = "";
        public CommandType Type;
        public string Description = "";
        public List<CommandOption> Options = new();
        public string? DefaultMemberPermissions;
        public bool DmPermissions;
        public bool Nsfw;

        public virtual async Task CallCommand(InteractionObject interaction)
        {
            Log.Debug($"Command {Name} Called");
        }

        //TO DO:
        public static async Task<HttpResponseMessage> GetCommand(string commandId, string guildId = "-1")
        {
            string URL;
            if (guildId == "-1")
            {
                URL = $"https://discord.com/api/v10/applications/{Application.Id}/guilds/{guildId}/commands/{commandId}/permissions";
            }
            else
            {
                URL = $"https://discord.com/api/v10/applications/{Application.Id}/commands/{commandId}";
            }
            return await new HTTPRequest(HTTPRequest.CreateHTTPMessage(HttpMethod.Get, URL), 10, 2).SendHTTPMessage(); 

        }

        public virtual async Task<HttpResponseMessage> AddCommand(Command command, string guildId = "-1")
        {
            string URL;
            using StringContent jsonContent = new(JsonSerializer.Serialize(new
            {
                name = command.Name.ToString(),
                description = command.Description.ToString()
            }), Encoding.UTF8, "application/json");
            Log.Trace(jsonContent.ToString());

            //change based on whether its a guild command or not
            if (guildId == "-1")
            {
                URL = $"https://discord.com/api/v10/applications/{Application.Id}/guilds/{guildId}/commands";
            }
            else
            {
                URL = $"https://discord.com/api/v10/applications/{Application.Id}/commands";
            }
            return await new HTTPRequest(HTTPRequest.CreateHTTPMessage(HttpMethod.Post, URL), 10, 2).SendHTTPMessage();
        }

        /// <summary>
        /// https://discord.com/developers/docs/interactions/receiving-and-responding#responding-to-an-interaction
        /// </summary>
        public virtual async Task RespondToInteraction(string interactionId, string interactionToken, StringContent content)
        {
            string URL = $"https://discord.com/api/v10/interactions/{interactionId}/{interactionToken}/callback";
            HttpResponseMessage response = await new HTTPRequest(HTTPRequest.CreateHTTPMessage(HttpMethod.Post, URL, content), 30, 2).SendHTTPMessage();
            Console.WriteLine(response);
        }
    }
}