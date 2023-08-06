namespace ASiNet.Crossout.Core;
public class BotConfig
{
    public string TelegramBotToken { get; set; } = null!;

    public string TelegramChannelName { get; set; } = null!;
    public string ChatGptToken { get; set; } = null!;
    public string ChatGptAddress { get; set; } = null!;

    public int UpdateTimeMinutes { get; set; }
    public int LogLevel { get; set; }

    public static BotConfig Default => new() 
    { 
        ChatGptAddress = string.Empty, 
        ChatGptToken = string.Empty, 
        TelegramBotToken = string.Empty, 
        TelegramChannelName = string.Empty, 
        UpdateTimeMinutes = 30, 
        LogLevel = 3
    };
}
