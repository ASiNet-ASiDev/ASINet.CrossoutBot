using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASiNet.Crossaut.DB;
using ASiNet.Crossaut.DB.Domain.Market;
using ASiNet.Crossout.Logger;
using ASiNet.CrossoutDB.API;

namespace ASiNet.Crossout.Core;
public class CrossoutDBCalculator
{
    public CrossoutDBCalculator(CrossoutDbAPI api)
    {
        _api = api;
    }

    private CrossoutDbAPI _api;


    public async Task Update()
    {
        await UpdateRarities();
        await UpdateFractions();
        await UpdateCategories();
        await UpdateItems();
        await UpdateMarketHistory();
    }

    public List<SellOffset> Calc(int chunc = 5)
    {
        var dt = DateTime.UtcNow - TimeSpan.FromHours(25);
        using var itemsContext = new CrossoutMarketDBContext();
        
        var result = new List<SellOffset>();

        foreach (var rarity in itemsContext.Rarities)
        {
            var offsets = new List<SellOffset>(250);
            if(rarity.Items.Count <= 0)
                continue;
            foreach (var item in rarity.Items.Where(x => x.Category is not null))
            {
                var historys = item.MarketHistory.Where(x => x.UtcDate > dt).ToArray();
                if(historys.Length < 2)
                    continue;
                var history = historys.Length == 2 ? historys : historys[^2..];
                offsets.Add(new(item.LocalizedName ?? item.Name, 
                    history[1].SellPrice - history[0].SellPrice, history[1].BuyPrice - history[0].BuyPrice, 
                    rarity.LocalizedName ?? rarity.Name,
                    item.Category?.LocalizedName ?? item.Category?.Name ?? string.Empty));
            }
            if(offsets.Count <= 1)
                continue;
            offsets.Sort((x, y) => Math.Abs(x.SellPriceOffset) == Math.Abs(y.SellPriceOffset) ? 0 : Math.Abs(x.SellPriceOffset) > Math.Abs(y.SellPriceOffset) ? -1 : 1);
            result.AddRange(offsets.GetRange(0, chunc > offsets.Count ? offsets.Count : chunc));
        }

        return result;
    }


    private async Task UpdateFractions()
    {
        Log.InfoLog("Core_Market_GUF: Updating...");
        try
        {
            using var context = new CrossoutMarketDBContext();

            var rarity = await _api.GetFactionsAsync();
            foreach (var r in rarity)
            {
                if (context.Fractions.FirstOrDefault(x => x.MarketId == r.Id) is null)
                    context.Fractions.Add(new()
                    {
                        MarketId = r.Id,
                        Name = r.Name,
                    });
            }
            context.SaveChanges();
            Log.InfoLog("Core_Market_GUF: Updating[OK]");
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Core_Market_GUF: {ex.Message}");
        }
    }

    private async Task UpdateCategories()
    {
        Log.InfoLog("Core_Market_GUC: Updating...");
        try
        {
            using var context = new CrossoutMarketDBContext();

            var rarity = await _api.GetCategoriesAsync();
            foreach (var r in rarity)
            {
                if (context.Categories.FirstOrDefault(x => x.MarketId == r.Id) is null)
                    context.Categories.Add(new()
                    {
                        MarketId = r.Id,
                        Name = r.Name,
                    });
            }
            context.SaveChanges();
            Log.InfoLog("Core_Market_GUC: Updating[OK]");
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Core_Market_GUC: {ex.Message}");
        }
    }

    private async Task UpdateRarities()
    {
        Log.InfoLog("Core_Market_GUR: Updating...");
        try
        {
            using var context = new CrossoutMarketDBContext();

            var categories = await _api.GetRaritiesAsync();
            foreach (var c in categories)
            {
                if (context.Rarities.FirstOrDefault(x => x.MarketId == c.Id) is null)
                    context.Rarities.Add(new()
                    {
                        MarketId = c.Id,
                        Name = c.Name,
                    });
            }
            context.SaveChanges();
            Log.InfoLog("Core_Market_GUR: Updating[OK]");
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Core_Market_GUR: {ex.Message}");
        }
    }

    private async Task UpdateItems()
    {
        Log.InfoLog("Core_Market_GUI: Updating...");
        try
        {
            using var context = new CrossoutMarketDBContext();

            var fractions = await _api.GetItemsAsync();
            foreach (var i in fractions)
            {
                if (context.Items.FirstOrDefault(x => x.MarketId == i.Id) is null)
                    context.Items.Add(new()
                    {
                        MarketId = i.Id,
                        Name = i.Name,
                        AvailableName = i.AvailableName,
                        Description = i.Description,
                        LocalizedName = i.LocalizedName,
                        Rarity = context.Rarities.FirstOrDefault(x => x.MarketId == i.RarityId),
                        Fraction = context.Fractions.FirstOrDefault(x => x.MarketId == i.FactionNumber),
                        Category = context.Categories.FirstOrDefault(x => x.MarketId == i.CategoryId),
                    });
            }
            context.SaveChanges();
            Log.InfoLog("Core_Market_GUI: Updating[OK]");
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Core_Market_GUI: {ex.Message}");
        }
    }

    public async Task UpdateMarketHistory()
    {
        Log.InfoLog("Core_Market_UMH: Updating...");
        try
        {
            var currentTime = DateTime.UtcNow;
            var offsetTime = currentTime - TimeSpan.FromMinutes(5);
            using var itemsContext = new CrossoutMarketDBContext();
            foreach (var item in itemsContext.Items.Where(x => x.Category != null && x.Category.MarketId != 5))
            {
                try
                {
                    var historyFrames = (await _api.GetMarketItemsAsync((int)item.MarketId, offsetTime, currentTime));
                    var historyFrame = historyFrames.FirstOrDefault();
                    if (historyFrame is null)
                        continue;

                    itemsContext.MarketHistory.Add(new()
                    {
                        MarketId = historyFrame.Id,
                        BuyOrders = historyFrame.BuyOrders,
                        BuyPrice = historyFrame.BuyPrice,
                        SellOffers = historyFrame.SellOffers,
                        SellPrice = historyFrame.SellPrice,
                        UtcDate = historyFrame.UtcDate,
                        Item = item,
                    });
                }
                catch (Exception ex)
                {
                    Log.ErrorLog($"Core_Market_UMH: [item.marketId: {item.MarketId}] {ex.Message}");
                }
            }
            itemsContext.SaveChanges();
            Log.InfoLog("Core_Market_UMH: Updating[OK]");
        }
        catch (Exception ex)
        {
            Log.ErrorLog($"Core_Market_UMH: {ex.Message}");
        }
    }
}


public record SellOffset(string ItemName, double SellPriceOffset, double BuyPriceOffset, string RarityName, string CategoryName);