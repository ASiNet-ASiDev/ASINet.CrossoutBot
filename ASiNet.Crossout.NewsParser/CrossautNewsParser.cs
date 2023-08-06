using System.Text.RegularExpressions;
using ASiNet.Crossout.Logger;
using ASiNet.Crossout.NewsParser.Domain;

namespace ASiNet.Crossout.NewsParser;

public partial class CrossautNewsParser : IDisposable
{
    public CrossautNewsParser(string baseAddres = @"https://crossout.net")
    {
        Log.InfoLog($"Parser starting...");
        _httpClient = new HttpClient() 
        { 
            BaseAddress = new(baseAddres),    
        };

        Log.InfoLog($"Parser starting[OK] Base addres: {baseAddres}");
    }

    private HttpClient _httpClient;


    public async Task<string[]> ParseNewsPage(int pageIndex = -1)
    {
        Log.InfoLog($"Parser_PARSE_PAGE[{(pageIndex == -1 ? "FIRST" : $"{pageIndex}")}]: Wait...");
        using HttpResponseMessage response = await _httpClient.GetAsync(pageIndex == -1 ? "/ru/news" : $"/ru/news/page/{pageIndex}/");

        Log.InfoLog("Parser_PP: Response - [OK]");

        var data = await response.Content.ReadAsStringAsync();

        Log.InfoLog("Parser_PP: Reading data - [OK]");

        var result = new List<string>(10);

        foreach (Match match in ParseNewsUris().Matches(data))
        {
            var group = match.Groups["uri"];
            result.Add(group.Value);
        }
        result.Reverse();
        Log.InfoLog("Parser_PP: Parse data - [OK]");

        Log.InfoLog($"Parser_PARSE_PAGE[{(pageIndex == -1 ? "FIRST" : $"{pageIndex}")}]: [OK] Pages:\n\t{string.Join("\n\t", result)}");

        return result.ToArray();
    }

    public async Task<ParsedNews> ParseNews(string newsUri)
    {
        Log.InfoLog($"Parser_PARSE_NEWS[{newsUri}]: Wait...");

        using HttpResponseMessage response = await _httpClient.GetAsync(newsUri);

        Log.InfoLog("Parser_PN: Response - [OK]");

        var data = await response.Content.ReadAsStringAsync();
        Log.InfoLog("Parser_PN: Reading response data - [OK]");


        var title = ParseNewsTitle().Match(data).Groups["title"].Value;
        var content = ParseNewsContent().Match(data).Groups["news_content"].Value;
        var images = ParseNewsImageUris().Matches(data).Select(x => x.Groups["img_uri"].Value).ToArray();
        Log.InfoLog("Parser_PN: Parsing data - [OK]");

        var noTegsContent = ReplaceAllTegs().Replace(content, string.Empty);
        var normalizeContent = ReplaceNewLine().Replace(noTegsContent, "\n");
        Log.InfoLog("Parser_PN: Normalize data - [OK]");

        Log.InfoLog($"Parser_PARSE_NEWS[{newsUri}]: [OK]");
        return new(newsUri, title, normalizeContent, images);
    }

    public async Task<string?> DownloadOrGetImage(string uri)
    {
        try
        {
            Log.InfoLog($"Parser_DOWNLOAD_IMAGE[{uri}]: Wait...");
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database", "images");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var fileName = Path.Combine(dirPath, Path.GetFileName(uri));

            if (File.Exists(fileName))
                return fileName;

            using (var s = await _httpClient.GetStreamAsync(uri))
            {
                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    s.CopyTo(fs);
                }
            }
            Log.InfoLog($"Parser_DOWNLOAD_IMAGE[{uri}]: [OK]");
            return fileName;
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Parser_DOWNLOAD_IMAGE[{uri}]: {ex.Message}");
            return null;
        }
    }

    [GeneratedRegex("<a href=\"(?<uri>.+)\" class=\"news-item__thumb\">")]
    private static partial Regex ParseNewsUris();

    [GeneratedRegex("<div class=\"content__title block-two\"><h1>(?<title>.+)</h1></div>")]
    private static partial Regex ParseNewsTitle();

    [GeneratedRegex("<news class=\".+?\">(?<news_content>.+)</news>", RegexOptions.Singleline)]
    private static partial Regex ParseNewsContent();

    [GeneratedRegex("<img alt=\"(?<alt_img_uri>.*)\" src=\"(?<img_uri>.+)\" style=\".*\".*/>")]
    private static partial Regex ParseNewsImageUris();

    [GeneratedRegex("<.+?\\/?>|&.+?;|[ ]{2,}?|\\t", RegexOptions.Singleline)]
    private static partial Regex ReplaceAllTegs();

    [GeneratedRegex("[\n\r]+", RegexOptions.Singleline)]
    private static partial Regex ReplaceNewLine();

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}