using BotFramework.Structures;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Logging;
using BotFramework.Handlers;

namespace BotFramework.Commands
{
    class AddShakeResponse : Command
    {
        public new string? Id;
        public new bool GuildCommand { get; private set; }
        public new string Name = "addresponse";
        public new CommandType Type = CommandType.CHAT_INPUT;
        public new string Description = "add shake response";
        public new List<CommandOption> Options = new()
        {
            new CommandOption()
            {
                Type = CommandOptionType.STRING,
                Name = "response",
                Description = "new response to add",
                Required = true
            },
            new CommandOption()
            {
                Type = CommandOptionType.STRING,
                Name = "language",
                Description = "language of answer",
                Required = false
            }
        };
        public new string DefaultMemberPermissions = "0";
        public new bool DmPermissions = true;
        public new bool Nsfw = false;

        public override async Task CallCommand(InteractionObject interaction)
        {
            string language = "en";
            if (interaction.Data.Options.Length > 1 && interaction.Data.Options[1].Value.ToString() != "")
            {
                //this needs to be checked that if it isnt a valid option its not accepted. 
                language = interaction.Data.Options[1].Value.ToString();
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load("Data/ShakeAnswers.xml");
            }
            catch (Exception e)
            {
                Log.Warn("Shake answers cannot be loaded! " + e);
                return;
            }

            XElement element = doc.Descendants("Answers")
                .Where(a => (string)a.Attribute("language") == language)
                .First();

            if (element == null) return; //send message that it failed

            // the right element is found but this isnt adding it to the file
            element.Add(new XElement("Answer", interaction.Data.Options[0].Value.ToString()));

            element.Attribute("count").Value = (int.Parse(element.Attribute("count").Value) + 1).ToString();
            Log.Debug("Shake Answer Added");
            doc.Save("Data/ShakeAnswers.xml");

            using StringContent jsonContent = new(JsonSerializer.Serialize(new
            {
                type = (int)InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
                data = new
                {
                    content = "added to responses"
                }
            }), Encoding.UTF8, "application/json");

            await RespondToInteraction(interaction.Id, interaction.ContinuationToken, jsonContent);
        }

        public override async Task<HttpResponseMessage> AddCommand(Command command, string guildId = "-1")
        {
            string URL;
            using StringContent jsonContent = new(JsonSerializer.Serialize
                (
                new
                {
                    name = Name,
                    description = Description,
                    options = Options,
                    type = Type
                }
                ), Encoding.UTF8, "application/json");
            //change based on whether its a guild command or not
            if (guildId != "-1")
            {
                URL = $"https://discord.com/api/v10/applications/{Application.Id}/guilds/{guildId}/commands";
            }
            else
            {
                URL = $"https://discord.com/api/v10/applications/{Application.Id}/commands";
            }
            return await new HTTPRequest(HTTPRequest.CreateHTTPMessage(HttpMethod.Post, URL, jsonContent), 10, 2).SendHTTPMessage();
        }
    }
}