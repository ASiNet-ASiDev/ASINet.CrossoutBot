using ASiNet.Crossaut.DB;
using ASiNet.Crossaut.DB.Domain.News;
using ASiNet.Crossout.Logger;
using ASiNet.Crossout.NewsParser;
using ASiNet.Crossout.TelegramBot;

namespace ASiNet.Crossout.Core;

public class CrossautBotCore : IDisposable
{
    public CrossautBotCore(BotConfig config)
    {
        Log.InfoLog("Core: init...");
        _config = config;

        _parser = new();

        _compressor = new(config.ChatGptToken, config.ChatGptAddress);

        Log.InfoLog("Core: Load database....");
        using (var context = new CrossautNewsDBContext())
        {
            context.Database.EnsureCreated();
        }
        Log.InfoLog("Core: Load database[OK]");

        _telegram = new(config.TelegramBotToken, config.TelegramChannelName);

        _updateTime = TimeSpan.FromMinutes(config.UpdateTimeMinutes);
        _timer = new Timer(OnTimeUpdate, null, TimeSpan.FromSeconds(0), UpdateTime);

        Log.InfoLog("Core: init[OK]");
    }

    public TimeSpan UpdateTime
    { 
        get => _updateTime;
        set
        {
            _updateTime = value;
            _timer.Change(TimeSpan.FromSeconds(0), _updateTime);
        }
    }

    private TimeSpan _updateTime = TimeSpan.FromMinutes(30);

    private BotConfig _config;

    private Timer _timer;
    private CrossautNewsParser _parser;
    private TelegramAPI _telegram;
    private ChatGPTTextCompressor _compressor;
    private async void OnTimeUpdate(object? state = null)
    {
        try
        {
            Log.InfoLog("Core_OTU: Updating...");

            var pages = await _parser.ParseNewsPage();
            Log.InfoLog("Core_OTU: Getting pages[OK]");

            using var context = new CrossautNewsDBContext();

            foreach (var page in pages)
            {
                if (context.News.FirstOrDefault(x => x.NewsUri == page) is not null)
                    continue;

                var news = await _parser.ParseNews(page);
                news.Content = news.Content.Trim('\n', '\t', ' ');
                news.Title = news.Title.Trim('\n', '\t', ' ');

                var compressedContent = await _compressor.Compress(news.Content);

                _ = SendTelegramMsg(news.Title, compressedContent, news.NewsUri, news.Images);

                context.News.Add(new(page, news.Title, news.Content, Array.ConvertAll(news.Images, x => new DbImageUri(x)))
                {
                    CreatedTime = DateTime.UtcNow, CompressedContent = compressedContent,
                });
            }
            context.SaveChanges();
            Log.InfoLog("Core_OTU: Updating[OK]");
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Core_OTU: {ex.Message}");
        }
        finally
        {
            Log.InfoLog($"Core_OTU: Next Update: {DateTime.UtcNow.Add(_updateTime).ToString("G")}");
        }
    }

    public async Task SendTelegramMsg(string title, string content, string uri, params string[] images)
    {
        try
        {
            var imageUri = images.FirstOrDefault();
            if (imageUri != null)
            {
                var image = await _parser.DownloadOrGetImage(imageUri);
                if (image != null)
                {
                    _ = _telegram.SendPost(title, content, uri, image);
                    return;
                }
            }
            _ = _telegram.SendPost(title, content, uri);
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"STM: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Log.InfoLog("Core: disposing...");

        _timer?.Dispose();
        _parser?.Dispose();

        Log.InfoLog("Core: dispose[OK]");
    }
}
