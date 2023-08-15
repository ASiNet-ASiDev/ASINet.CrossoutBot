using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ASiNet.Crossaut.DB.Domain.Market;
public class Item
{
    [Key]
    public long Id { get; set; }

    public long MarketId { get; set; }

    public string Name { get; set; } = null!;
    public string? LocalizedName { get; set; } = null!;
    public string? AvailableName { get; set; } = null!;
    public string? Description { get; set; } = null!;

    public virtual Category? Category { get; set; } = null!;
    public virtual Fraction? Fraction { get; set; } = null!;

    public virtual Rarity? Rarity { get; set; } = null!;

    public virtual ICollection<MarketHistoryFrame> MarketHistory { get; set; } = null!;
}

public class Category
{
    [Key]
    public long Id { get; set; }
    public long MarketId { get; set; }
    public string Name { get; set; } = null!;
    public string? LocalizedName { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = null!;
}

public class Rarity
{
    [Key]
    public long Id { get; set; }

    public long MarketId { get; set; }
    public string? LocalizedName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = null!;
}

public class Fraction
{
    [Key]
    public long Id { get; set; }

    public long MarketId { get; set; }

    public string Name { get; set; } = null!;
    public string? LocalizedName { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = null!;
}

public class MarketHistoryFrame
{
    [Key]
    public long Id { get; set; }
    public long MarketId { get; set; }
    public double SellPrice { get; set; }

    public double BuyPrice { get; set; }

    public int SellOffers { get; set; }

    public int BuyOrders { get; set; }

    public DateTime UtcDate { get; set; }

    public virtual Item Item { get; set; } = null!;
}
