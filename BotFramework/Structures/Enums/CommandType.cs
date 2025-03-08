namespace Magic8.Structures
{
    public enum CommandType
    {
        CHAT_INPUT = 1, //Slash commands; a text-based command that shows up when a user types /
        USER = 2, //A UI-based command that shows up when you right click or tap on a user
        MESSAGE = 3 //A UI-based command that shows up when you right click or tap on a message
    }
}
