namespace ASiNet.Crossout.Core;
public class BotConfig
{
    public string TelegramBotToken { get; set; } = null!;

    public string TelegramChannelName { get; set; } = null!;
    public string ChatGptToken { get; set; } = null!;
    public string ChatGptAddress { get; set; } = null!;

    public int UpdateTimeMinutes { get; set; }
    public int LogLevel { get; set; }

    public bool DisableSendingPosts { get; set; }

    public bool UseChatGPT { get; set; }
    public string ChatGPTCommand { get; set; } = null!;
    public string ChatGPTIterationCommand { get; set; } = null!;

    public int MaxTextLength { get; set; }

    public static BotConfig Default => new() 
    { 
        ChatGptAddress = string.Empty, 
        ChatGptToken = string.Empty, 
        TelegramBotToken = string.Empty, 
        TelegramChannelName = string.Empty, 
        DisableSendingPosts = false,
        UpdateTimeMinutes = 30, 
        LogLevel = 3,
        UseChatGPT = true, 
        ChatGPTCommand = "Сократи текст на русском языке: $text",
        ChatGPTIterationCommand = "Сократи ещё", 
        MaxTextLength = 500,
    };
}
