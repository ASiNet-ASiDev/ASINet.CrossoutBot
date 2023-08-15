using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ASiNet.Crossout.Logger;

namespace ASiNet.Crossout.Core;
public static class ConfigLoader
{

    public static string CNF_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bot_config.json"); 

    public static bool CreateEmptyConfig()
    {
        try
        {
            if(File.Exists(CNF_PATH))
                File.Delete(CNF_PATH);
            File.WriteAllBytes(CNF_PATH, JsonSerializer.SerializeToUtf8Bytes(BotConfig.Default, options: new JsonSerializerOptions() 
            {
                WriteIndented = true
            }));
            return true;
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Cnf: {ex.Message}");
            return false;
        }
    }

    public static BotConfig? ReadConfig()
    {
        try
        {
            if (!File.Exists(CNF_PATH))
                return null;

            var result = JsonSerializer.Deserialize<BotConfig>(File.ReadAllBytes(CNF_PATH), options: new JsonSerializerOptions()
            {
                WriteIndented = true
            });

            if (result == null)
                throw new Exception("Config is corrupted!");

            if(result.UseChatGPT)
            {
                if (result.ChatGptAddress == string.Empty)
                    throw new Exception($"[{nameof(BotConfig.ChatGptAddress)}]: Is not set!");

                if (result.ChatGptToken == string.Empty)
                    throw new Exception($"[{nameof(BotConfig.ChatGptToken)}]: Is not set!");
            }

            if (result.TelegramChannelName == string.Empty || result.TelegramChannelName.FirstOrDefault() != '@')
                throw new Exception($"[{nameof(BotConfig.TelegramChannelName)}]: Is not set or incorrect format[correct: @channel_name]!");

            if (result.UpdateTimeMinutes <= 0)
                throw new Exception($"[{nameof(BotConfig.UpdateTimeMinutes)}]: Incorrect value[correct: value >= 1]!");

            if (result.LogLevel < 0 || result.LogLevel > 3)
                throw new Exception($"[{nameof(BotConfig.LogLevel)}]: Incorrect value[correct: 0 - disable logs, or - 1 error logs, or - 2 error and warning logs, or - 3 all logs]!");

            return result;
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Cnf: {ex.Message}");
            return null;
        }
    }
}
