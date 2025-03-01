using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Magic8.Structures
{
    /// <summary>
    /// https://discord.com/developers/docs/interactions/receiving-and-responding#interaction-object-interaction-structure
    /// </summary>
    public struct InteractionObject
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("application_id")] public string ApplicationId { get; set; }
        [JsonPropertyName("type")] public InteractionType Type { get; set; }
        [JsonPropertyName("data")] public ApplicationCommandData Data { get; set; }
        [JsonPropertyName("guild_id")] public string GuildId { get; set; }
        [JsonPropertyName("channel_id")] public string ChannelId { get; set; }
        [JsonPropertyName("token")] public string ContinuationToken { get; set; }
    }

    /// <summary>
    /// https://discord.com/developers/docs/interactions/receiving-and-responding#interaction-object-application-command-data-structure
    /// </summary>
    public struct ApplicationCommandData
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("type")] public CommandType Type { get; set; }
        //resolved data
        [JsonPropertyName("options")] public InteractionDataOption[] Options { get; set; }
        [JsonPropertyName("guild_id")] public string CommandGuildId { get; set; }
        [JsonPropertyName("target_id")] public string CommandTargetId { get; set; }
    }


    /// <summary>
    /// https://discord.com/developers/docs/interactions/receiving-and-responding#interaction-object-application-command-interaction-data-option-structure
    /// </summary>
    public struct InteractionDataOption
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("type")] public CommandOptionType Type { get; set; }
        [JsonPropertyName("value")] public JsonElement Value { get; set; }
        [JsonPropertyName("options")] public InteractionDataOption[] Options { get; set; }
        [JsonPropertyName("focused")] public bool FocusedForAutocomplete { get; set; }
    }

    
}
