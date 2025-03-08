using Magic8.Structures;
using System.Text;
using System.Text.Json;

namespace Magic8.Commands
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
        private List<string> Answers = new()
        {
            "It is certain", "It is decidedly so", "Without a doubt",
            "Yes definitely", "You may rely on it", "As I see it, yes",
            "Most likely", "Outlook good", "Yes", "Signs point to yes",
            "Reply hazy, try again", "Ask again later", "Better not tell you now",
            "Cannot predict now", "Concentrate and ask again",
            "Don't count on it", "My reply is no", "My sources say no",
            "Outlook not so good", "Very doubtful"
        };

        public override async Task CallCommand(InteractionObject interaction)
        {
            Console.WriteLine("Shake Command Called");
            Random random = new();
            string answer = "";


            if (interaction.Data.Options is not null && interaction.Data.Options[0].Value.ToString() != "")
            {
                answer = $"**Question:** {interaction.Data.Options[0].Value.ToString()}\n\n**Answer:** {Answers[random.Next(0, Answers.Count)]}";
            }
            else
            {
                answer = Answers[random.Next(0, Answers.Count)];
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
}
