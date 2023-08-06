using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Crossaut.DB.Domain.News;
public class DbNews
{
    public DbNews() { }
    public DbNews(string uri, string title, string content, params DbImageUri[] imsages)
    {
        NewsUri = uri;
        Title = title;
        RawContent = content;
        Images = imsages;
    }

    public long Id { get; set; }

    public string NewsUri { get; set; } = null!;
    public string Title { get; set; } = null!;

    public string RawContent { get; set; } = null!;

    public string CompressedContent { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public virtual ICollection<DbImageUri> Images { get; set; } = null!;

}
