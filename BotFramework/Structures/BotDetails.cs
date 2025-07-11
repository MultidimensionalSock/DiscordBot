﻿using System.Text.Json.Serialization;

namespace BotFramework.Structures
{
    public struct BotDetails
    {
        [JsonPropertyName("v")] public int APIVersion { get; set; }
        //[JsonPropertyName("user")] public UserObject User { get; set; }
        [JsonPropertyName("guilds")] public Guild[]? Guilds { get; set; }
        [JsonPropertyName("session_id")] public string SessionId { get; set; }
        [JsonPropertyName("shard")] public int[]? Shard { get; set; }
    }

    public struct AvatarDecorationDataObject
    {
        public string asset { get; set; }
        public string sku_id { get; set; }
    }

    public class Guild
    {
        public string? id { get; set; }
        public bool unavailable { get; set; }
    }
}
