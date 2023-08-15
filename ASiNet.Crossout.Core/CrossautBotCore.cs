using System.Text;
using ASiNet.Crossaut.DB;
using ASiNet.Crossaut.DB.Domain.News;
using ASiNet.Crossout.Logger;
using ASiNet.Crossout.NewsParser;
using ASiNet.Crossout.TelegramBot;
using ASiNet.CrossoutDB.API;

namespace ASiNet.Crossout.Core;

public class CrossautBotCore : IDisposable
{
    public CrossautBotCore(BotConfig config)
    {
        Log.InfoLog("Core: init...");
        _config = config;

        Log.InfoLog("Core: Load database....");
        using (var context = new CrossoutNewsDBContext())
        {
            context.Database.EnsureCreated();
        }
        Log.InfoLog("Core: Load database[OK]");

        Log.InfoLog("Core: Load market_database....");
        using (var context = new CrossoutMarketDBContext())
        {
            context.Database.EnsureCreated();
        }
        Log.InfoLog("Core: Load market_database[OK]");


        _crossoutDbApi = new();
        _cdbCalc = new(_crossoutDbApi);
        _parser = new();

        if(config.UseChatGPT)
            _compressor = new(config);

        _telegram = new(config.TelegramBotToken, config.TelegramChannelName, config.MaxTextLength);

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
    private ChatGPTTextCompressor? _compressor;
    private CrossoutDbAPI _crossoutDbApi;
    private CrossoutDBCalculator _cdbCalc;

    private void OnTimeUpdate(object? state = null)
    {
        Log.InfoLog("Core_OTU: Updating...");
        NewsUpdate();
        if(DateTime.UtcNow.Hour == 6)
            MarketUpdate();

        Log.InfoLog($"Core_OTU: Next Update: {DateTime.UtcNow.Add(_updateTime).ToString("G")}");
    }

    private async void MarketUpdate()
    {
        Log.InfoLog("Core_Market: Updating...");
        try
        {
            await GlobalUpdateDb();
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Core_MarketUpdate: {ex.Message}");
        }
    }

    private async void NewsUpdate()
    {
        Log.InfoLog("Core_NewsUpdate: Updating...");
        try
        {
            var pages = await _parser.ParseNewsPage();
            Log.InfoLog("Core_NewsUpdate: Getting pages[OK]");

            using var context = new CrossoutNewsDBContext();

            foreach (var page in pages)
            {
                if (context.News.FirstOrDefault(x => x.NewsUri == page) is not null)
                    continue;

                var news = await _parser.ParseNews(page);
                news.Content = news.Content.Trim('\n', '\t', ' ');
                news.Title = news.Title.Trim('\n', '\t', ' ');

                var compressedContent = _compressor is not null ? 
                    await _compressor.Compress(news.Content) : 
                    news.Content;

                _ = SendNewsTelegramMsg(news.Title, compressedContent, news.NewsUri, news.Images);

                context.News.Add(new(page, news.Title, news.Content, Array.ConvertAll(news.Images, x => new DbImageUri(x)))
                {
                    CreatedTime = DateTime.UtcNow,
                    CompressedContent = compressedContent,
                });
            }
            context.SaveChanges();
            Log.InfoLog("Core_NewsUpdate: Updating[OK]");
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Core_NewsUpdate: {ex.Message}");
        }
    }

    private async Task GlobalUpdateDb()
    {
        await _cdbCalc.Update();
        var offsets = _cdbCalc.Calc();
        if(offsets.Count == 0)
            return;
        await SendPriceTelegramMsg(offsets);
    }

    public async Task SendNewsTelegramMsg(string title, string content, string uri, params string[] images)
    {
        try
        {
            if (_config.DisableSendingPosts)
                return;
            var imageUri = images.FirstOrDefault();
            if (imageUri != null)
            {
                var image = await _parser.DownloadOrGetImage(imageUri);
                if (image != null)
                {
                    _ = _telegram.SendNewsPost(title, content, uri, image);
                    return;
                }
            }
            _ = _telegram.SendNewsPost(title, content, uri);
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"STM: {ex.Message}");
        }
    }


    public async Task SendPriceTelegramMsg(List<SellOffset> offsets)
    {
        try
        {
            if (_config.DisableSendingPosts)
                return;
            var sb = new StringBuilder();

            foreach (var offset in offsets.GroupBy(x => x.RarityName))
            {
                sb.AppendLine();
                sb.AppendLine($"<b>Редкость: <i>{offset.Key}</i></b>");

                var maxFirstL = offset.Max(x => x.CategoryName.Length);
                var maxSecondL = offset.Max(x => x.ItemName.Length);
                if(maxFirstL <= 12 && maxSecondL <= 18)
                {
                    maxFirstL = 12;
                    maxSecondL = 18;
                }

                foreach (var item in offset)
                {
                    sb.AppendLine();
                    sb.Append("<code>");
                    sb.Append($"* {item.CategoryName}");
                    if (maxFirstL - item.CategoryName.Length > 0)
                        sb.Append(' ', maxFirstL - item.CategoryName.Length);
                    sb.Append(" | ");
                    sb.Append(item.ItemName);
                    if (maxSecondL - item.ItemName.Length > 0)
                        sb.Append(' ', maxSecondL - item.ItemName.Length);
                    sb.Append("</code>");
                    sb.AppendLine();
                    var sp = $"Sell: {Math.Round(item.SellPriceOffset, 2)}";
                    var bp = $"Buy: {Math.Round(item.BuyPriceOffset, 2)}";

                    sb.Append("<code>");
                    sb.Append(sp);
                    if((maxFirstL - sp.Length) + 2 > 0)
                        sb.Append(' ', (maxFirstL - sp.Length) + 2);
                    sb.Append(" | ");
                    sb.Append(bp);
                    if ((maxFirstL - bp.Length) + 2 > 0)
                        sb.Append(' ', (maxSecondL - bp.Length) + 2);
                    sb.Append("</code>");
                }
            }

            await _telegram.SendPricesPost(sb.ToString());
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
