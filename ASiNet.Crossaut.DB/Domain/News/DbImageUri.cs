using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Crossaut.DB.Domain.News;
public class DbImageUri
{
    public DbImageUri() { }
    public DbImageUri(string imageUri)
    {
        ImageUri = imageUri;
    }
    public long Id { get; set; }

    public string ImageUri { get; set; } = null!;
}
