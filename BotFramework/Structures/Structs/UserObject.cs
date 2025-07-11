﻿namespace BotFramework.Structures
{
    public class UserObject
    {
        public required string? id { get; set; }
        public required string? username { get; set; }
        public required string? discriminator { get; set; }
        public string? avatar { get; set; }
        public bool? bot { get; set; }
        public bool? system { get; set; }
        public bool? mfa_enabled { get; set; }
        public string? banner { get; set; }
        public int? accent_colour { get; set; }
        public string? locale { get; set; }
        public bool? verified { get; set; }
        public string? email { get; set; }
        public int? flags { get; set; }
        public int? premium_type { get; set; }
        public int? public_flags { get; set; }
        public AvatarDecorationDataObject? avatar_decoration_data { get; set; }

    }
}
