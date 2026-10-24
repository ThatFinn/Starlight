using Il2CppMonomiPark.SlimeRancher.Economy;
using Il2CppMonomiPark.SlimeRancher.UI;
using Starlight.Prism.Data.Market;

namespace Starlight.Prism.Lib;
/// <summary>
/// A library of helper functions for dealing with the market
/// </summary>
public static class PrismLibMarket
{
    /// <summary>
    /// Makes an identifiable type sellable in the plort market
    /// </summary>
    /// <param name="ident">The identifiable type to make sellable</param>
    /// <param name="prismMarketData">The market data for the identifiable type</param>
    public static void MakeSellable(IdentifiableType ident, PrismMarketData prismMarketData)
    {
        if (ident == null) return;
        if (ident.IsPlayer) return;
        if (ident.IsGadget()) return;
        PrismShortcuts.MarketData.Remove(ident);

        if (PrismShortcuts.RemoveMarketPlortEntries.Contains(ident))
            PrismShortcuts.RemoveMarketPlortEntries.Remove(ident);
        PrismShortcuts.MarketPlortEntries.Add(new PlortEntry
            {
                IdentType = ident
            },
            prismMarketData.HideInMarketUI);
        PrismShortcuts.MarketData.Add(ident, prismMarketData);
        TryRefreshMarketData();
    }

    internal static void TryRefreshMarketData(PlortEconomySettings settings = null)
    {
        try
        {
            if (settings == null) settings = Get<PlortEconomySettings>("PlortEconomy");
            if (settings == null) return;
            List<PlortValueConfiguration> entries = new List<PlortValueConfiguration>();
            entries.AddRange(settings.PlortsTable.Plorts);
            foreach (var entry in PrismShortcuts.MarketData)
            {
                foreach (var existingEntry in settings.PlortsTable.Plorts)
                {
                    if (existingEntry.Type.ReferenceId == entry.Key.ReferenceId)
                    {
                        entries.Remove(existingEntry);
                        break;
                    }
                }

                var defaultValues = new PlortValueConfiguration()
                {
                    FullSaturation = entry.Value.Saturation,
                    Type = entry.Key,
                    InitialValue = entry.Value.Value,
                };

                entries.Add(defaultValues);
            }

            var newTable = new PlortValueConfigurationTable
            {
                Plorts = entries.ToArray()
            };
            settings.PlortsTable = newTable;
        } catch { }
    }

    /// <summary>
    /// Checks if an identifiable type is sellable in the plort market
    /// </summary>
    /// <param name="ident">The identifiable type to check</param>
    /// <returns>Whether or not the identifiable type is sellable</returns>
    public static bool IsSellable(IdentifiableType ident)
    {
        if (ident == null) return false;
        if (ident.IsPlayer) return false;
        if (ident.IsGadget()) return false;
        try
        {
            var settings = Get<PlortEconomySettings>("PlortEconomy");
            if (settings != null)
                foreach (var entry in settings.PlortsTable.Plorts)
                    if (entry.Type == ident)
                        return true;
        } catch { }


        if (PrismShortcuts.MarketData.ContainsKey(ident)) return true;
        foreach (var keyPair in PrismShortcuts.MarketPlortEntries)
        {
            PlortEntry entry = keyPair.Key;
            if (entry.IdentType == ident)
                return true;
        }
        
        if(ident.ReferenceId!="IdentifiableType.UnstablePlort")
            foreach (var pair in PrismLibLookup.RefIDTranslationPrismNativeBaseSlime)
                if (pair.Value == ident.ReferenceId)
                {
                    bool returnBool = true;
                    foreach (IdentifiableType removed in PrismShortcuts.RemoveMarketPlortEntries)
                        if (ident == removed)
                        {
                            returnBool = false; 
                            break;
                        }
                    return returnBool;
                }

        return false;
    }

    /// <summary>
    /// Makes an identifiable type not sellable in the plort market
    /// </summary>
    /// <param name="ident">The identifiable type to make not sellable</param>
    public static void MakeNotSellable(IdentifiableType ident)
    {
        if (ident == null) return;
        if (ident.IsPlayer) return;
        if (ident.IsGadget()) return;
        if(!PrismShortcuts.RemoveMarketPlortEntries.Contains(ident))
         PrismShortcuts.RemoveMarketPlortEntries.Add(ident);
        foreach (var keyPair in PrismShortcuts.MarketPlortEntries)
        {
            PlortEntry entry = keyPair.Key;
            if (entry.IdentType == ident)
            {
                PrismShortcuts.MarketPlortEntries.Remove(entry);
                break;
            }
        }

        PrismShortcuts.MarketData.Remove(ident);
    }
}