﻿using BotFramework.Commands;
using System.Text.Json.Serialization;

namespace BotFramework.Structures
{
    public class CommandData
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("type")] public int Type { get; set; }
        [JsonPropertyName("application_id")] public string? ApplicationId { get; set; }
        [JsonPropertyName("guild_id")] public string? GuildId { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("options")] public CommandOption[]? Options { get; set; }
        [JsonPropertyName("default_member_permissions")] public string? DefaultMemberPermissions { get; set; }
        [JsonPropertyName("dm_permission")] public bool DmPermissions { get; set; }
        [JsonPropertyName("nsfw")] public bool Nsfw { get; set; }
        [JsonPropertyName("version")] public string? CommandVersion { get; set; }

        public static bool operator ==(CommandData a, CommandData b)
        {
            if (a is null || b is null) return false;

            return (a.Type == b.Type &&
                a.Name == b.Name &&
                a.Description == b.Description &&
                a.Options == b.Options &&
                a.DefaultMemberPermissions == b.DefaultMemberPermissions &&
                a.DmPermissions == b.DmPermissions &&
                a.Nsfw == b.Nsfw);
        }

        public static bool operator ==(CommandData a, Command b)
        {
            if (a is null || b is null) return false;

            return (a.Type == (int)b.Type &&
                a.Name == b.Name &&
                a.Description == b.Description &&
                a.Options == b.Options.ToArray() &&
                a.DefaultMemberPermissions == b.DefaultMemberPermissions &&
                a.DmPermissions == b.DmPermissions &&
                a.Nsfw == b.Nsfw);
        }

        public static bool operator !=(CommandData a, Command b) { return !(a == b); }

        public static bool operator !=(CommandData a, CommandData b) { return !(a == b); }
    }
}
