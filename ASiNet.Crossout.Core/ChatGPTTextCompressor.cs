using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASiNet.Crossout.Logger;
using OpenAI_API;

namespace ASiNet.Crossout.Core;
public class ChatGPTTextCompressor
{
    public ChatGPTTextCompressor(string token, string apiUri)
    {
        _api = new OpenAIAPI(token) { ApiUrlFormat = apiUri };
    }

    public int MaxOutputLength { get; set; } = 500; 

    private OpenAIAPI _api;
    public async Task<string> Compress(string input)
    {
        Log.InfoLog("Text_Compressor_GPT: Compress...");
        var output = input;
        try
        {
            var chat = _api.Chat.CreateConversation();

            var sb = new StringBuilder();
            chat.AppendUserInput($"Сократи текст в 4 раза: {output}");

            await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
                sb.Append(res);
            output = sb.ToString();

            Log.InfoLog($"Text_Compressor_GPT: Compress iteration[0], [il={input.Length}, ol={output.Length}]");

            var i = 1;

            while (i <= 5 && (output.Length > MaxOutputLength))
            {
                sb = new StringBuilder();
                chat.AppendUserInput($"Сократи ещё");

                await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
                    sb.Append(res);
                output = sb.ToString();

                Log.WarningLog($"Text_Compressor_GPT: Compress iteration[{i}], [il={input.Length}, ol={output.Length}]");
                i++;
            }

            return output;
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Text_Compressor_GPT: {ex.Message}");
            return output;
        }
    }

}
