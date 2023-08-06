using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Crossout.NewsParser.Domain;
public class ParsedNews
{
    public ParsedNews(string newsUri, string title, string content, params string[] images)
    {
        NewsUri = newsUri;
        Title = title;
        Content = content;
        Images = images;
    }

    public string NewsUri { get; set; }

    public string Title { get; set; }
    public string[] Images { get; set; }

    public string Content { get; set; }

    public override string ToString()
    {
        return $"Title: {Title}\nContent:\n{Content}\nImages:\n{string.Join('\n', Images)}";
    }
}
