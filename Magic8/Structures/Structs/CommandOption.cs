using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Magic8.Structures
{
    public struct CommandOption
    {
        [JsonPropertyName("type")] public CommandOptionType Type { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("required")] public bool Required { get; set; }
        [JsonPropertyName("options")] public CommandOption[] Options { get; set; }
        //[JsonPropertyName("channel_types")] public CommandOptionType ChannelType { get; set; }
    }
}
