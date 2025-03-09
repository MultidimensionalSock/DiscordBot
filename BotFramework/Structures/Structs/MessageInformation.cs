namespace BotFramework.Structures.Structs
{
    public struct MessageInformation
    {
        public string? content; //text content of message (max 2000)
        public bool? tts; //if true sends a text to speech message
        public Embed[] embeds;
        public object[] components; //message components (buttons, select menus, etc. 
        public int[] sticker_ids;
    }

    public struct Embed
    {
        public string title;
        public string description;
        public int color;
    }
}
