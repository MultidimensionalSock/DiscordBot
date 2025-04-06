using BotFramework.Structures;
using System.Diagnostics.SymbolStore;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace BotFramework.Commands
{
    public class ShakeCommand : Command
    {
        public new string? Id;
        public new bool GuildCommand { get; private set; }
        public new string Name = "shake";
        public new CommandType Type = CommandType.CHAT_INPUT;
        public new string Description = "Shake the magic ball";
        public new List<CommandOption> Options = new()
        {
            new CommandOption()
            {
                Type = CommandOptionType.STRING,
                Name = "question",
                Description = "question being asked",
                Required = false
            }
        };
        public new string DefaultMemberPermissions = "0";
        public new bool DmPermissions = true;
        public new bool Nsfw = false;

        public string GetAnswer(string language = "en")
        {
            XDocument doc = XDocument.Load("Data/ShakeAnswers.xml");
            Random random = new();

            XElement element = doc.Descendants("Answers")
                .Where(a => (string)a.Attribute("language") == language)
                .First();
            int index = random.Next(0, (int)element.Attribute("count"));

            return element.Elements("Answer").ElementAt(index)?.Value.ToString();
        }

        public override async Task CallCommand(InteractionObject interaction)
        {
            Logger.Log(LogType.Response, "", "Shake Command Called");
            Random random = new();
            string answer = "";


            if (interaction.Data.Options is not null && interaction.Data.Options[0].Value.ToString() != "")
            {
                answer = $"**Question:** {interaction.Data.Options[0].Value.ToString()}\n\n**Answer:** {GetAnswer()}";
            }
            else
            {
                answer = GetAnswer();
            }
            using StringContent jsonContent = new(JsonSerializer.Serialize(new
            {
                type = (int)InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
                data = new
                {
                    content = answer
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
            return await HTTPHandler.SendRequest(HttpMethod.Post, URL, jsonContent);
        }
    }

    public struct AnswerData
    {
        public string[] answers { get; set; }
    }
}
