using System;
using ASiNet.Crossout.Logger;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ASiNet.Crossout.TelegramBot;

public class TelegramAPI
{
    public TelegramAPI(string token, string chennelName, int maxTextLength)
    {
        _channelName = chennelName;
        _client = new(token);
        MaxTextLength = maxTextLength;
    }

    private TelegramBotClient _client;

    private string _channelName;

    public int MaxTextLength { get; init; }

    public async Task SendNewsPost(string title, string text, string newsUri, params string[] images)
    {
        try
        {
            var content = $"<b>{title}</b>\n\n{SplitText(text)}\n\n<a href = \"{$"https://crossout.net{newsUri}"}\"><b>Читать полностью на оффициальном сайте Crossout.</b></a>";
            var chat = new ChatId(_channelName);
            if (images.Length == 1)
            {
                using var fs = new FileStream(images.First(), FileMode.Open, FileAccess.Read);
                var msg = await _client.SendPhotoAsync(chat, 
                    InputFile.FromStream(fs),
                    caption: content,
                    parseMode: ParseMode.Html);
            }
            else
            {
                var msg = await _client.SendTextMessageAsync(chat,
                    content,
                    parseMode: ParseMode.Html);
            }
        }
        catch (Exception ex)
        {
            Log.ErrorLog(ex.Message);
        }
    }

    public async Task SendPricesPost(string text, params string[] images)
    {
        try
        {
            var content = $"<b>Изменение рыночных цен за сутки, от {DateTime.UtcNow.ToString("d")}:</b>\n\n{text}\n\n<a href = \"{$"https://crossoutdb.com/"}\"><b>Данные взяты с сайта CrossoutDB.</b></a>";
            var chat = new ChatId(_channelName);
            if (images.Length == 1)
            {
                using var fs = new FileStream(images.First(), FileMode.Open, FileAccess.Read);
                var msg = await _client.SendPhotoAsync(chat,
                    InputFile.FromStream(fs),
                    caption: content,
                    parseMode: ParseMode.Html);
            }
            else
            {
                var msg = await _client.SendTextMessageAsync(chat,
                    content,
                    parseMode: ParseMode.Html);
            }
        }
        catch (Exception ex)
        {
            Log.ErrorLog(ex.Message);
        }
    }

    private string SplitText(string text)
    {
        if(text.Length > MaxTextLength)
        {
            for (var i = MaxTextLength; i < text.Length; i++)
            {
                if (text[i] == ' ')
                    return $"{text[0..i]}...";
            }
            return $"{text[0..MaxTextLength]}...";
        }
        else
            return text;
    }
}
