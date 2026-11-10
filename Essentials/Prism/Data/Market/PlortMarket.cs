using System;
using CppList = Il2CppSystem.Collections.Generic;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Economy;
using Il2CppMonomiPark.SlimeRancher.Slime;

namespace Starlight.Prism.Data.Market;

/// <summary>
/// Makes created plorts sellable
/// </summary>
public static class PlortMarket
{
    private const float DefaultSaturation = 3f;

    private static readonly Dictionary<IdentifiableType, (float Value, float Saturation)> pricing = new();
    private static readonly Dictionary<IdentifiableType, SlimeDefinition> producedBy = new();

    /// <summary>
    /// Makes a plort sellable and lists it on the market board
    /// </summary>
    /// <param name="baseValue"> What one sells for before the market moves </param>
    /// <param name="fullSaturation"> Sales until the price drops down; zero or fewer uses the default </param>
    /// <returns> False if the price is not positive and therefore not sellable </returns>
    public static bool MakeSellable(IdentifiableType plort, float baseValue, float fullSaturation)
    {
        ArgumentNullException.ThrowIfNull(plort);

        if (baseValue <= 0f) return false;
        if (fullSaturation <= 0f) fullSaturation = DefaultSaturation;

        pricing[plort] = (baseValue, fullSaturation);
        PlortMarketListing.Add(plort);

        return true;
    }

    /// <summary>
    /// Records which slime produces which plort
    /// </summary>
    public static void ProducedBy(IdentifiableType plort, SlimeDefinition slime)
    {
        ArgumentNullException.ThrowIfNull(plort);
        ArgumentNullException.ThrowIfNull(slime);

        producedBy[plort] = slime;
    }

    private static void LinkToSlimes()
    {
        if (producedBy.Count == 0) return;

        var relationships = GetAny<PlortRelationships>();
        if (relationships?._slimeDefinitionsByPlort == null)
        {
            LogWarning("Prism: The slime-plort table is unavailable, created plorts stay locked on the market board");
            return;
        }

        foreach (var (plort, slime) in producedBy)
        {
            if (plort == null || slime == null) continue;
            if (relationships._slimeDefinitionsByPlort.ContainsKey(plort)) continue;

            relationships._slimeDefinitionsByPlort[plort] = slime;
        }
    }

    private static void AddToTable(PlortEconomySettings settings, IdentifiableType plort, float baseValue, float fullSaturation)
    {
        var table = settings.PlortsTable;
        var existing = table.Plorts;

        foreach (var entry in existing.AsEnumerable())
        {
            if (entry?.Type == null || entry.Type.Pointer != plort.Pointer) continue;

            entry.InitialValue = baseValue;
            entry.FullSaturation = fullSaturation;
            return;
        }

        table.Plorts = existing.AddToNew(new PlortValueConfiguration
        {
            Type           = plort,
            InitialValue   = baseValue,
            FullSaturation = fullSaturation
        });
        
        settings.PlortsTable = table;
    }

    internal static void CatchUp(PlortEconomyDirector director)
    {
        if (director?._currValueMap == null || pricing.Count == 0) return;

        var settings = director._settings;
        if (settings == null)
            LogWarning("Prism: The market has no settings, so created plorts sell at a flat price");

        foreach (var (plort, price) in pricing)
        {
            if (plort == null) continue;

            if (settings != null) AddToTable(settings, plort, price.Value, price.Saturation);

            if (!director._currValueMap.ContainsKey(plort))
                director._currValueMap[plort] = new PlortEconomyDirector.CurrValueEntry(
                    price.Value, price.Value, price.Value, price.Saturation);

            EnsureCounted(director, plort);
        }

        LinkToSlimes();
    }

    /// <summary>
    /// Fills in the saved market state of created plorts
    /// </summary>
    internal static void SeedWorldModel(WorldModel worldModel)
    {
        if (worldModel == null || pricing.Count == 0) return;

        foreach (var (plort, price) in pricing)
        {
            if (plort == null) continue;

            Seed(worldModel.marketSaturation, plort, 0f);
            Seed(worldModel.previousDayPlortValues, plort, price.Value);
            Seed(worldModel.currentDayPlortValues, plort, price.Value);
        }
    }

    private static void Seed(CppList.Dictionary<IdentifiableType, float>? map, IdentifiableType plort, float value)
    {
        if (map != null && !map.ContainsKey(plort)) map[plort] = value;
    }
    
    internal static void EnsureCounted(PlortEconomyDirector director, IdentifiableType? plort)
    {
        if (director == null || plort == null) return;

        var sold = director._sessionSoldCount;
        if (sold != null && !sold.ContainsKey(plort)) sold[plort] = 0;
    }
}

[HarmonyPatch(typeof(PlortEconomyDirector), nameof(PlortEconomyDirector.InitModel))]
internal static class PlortMarketCatchUpPatch
{
    public static void Postfix(PlortEconomyDirector __instance)
    {
        try
        {
            PlortMarket.CatchUp(__instance);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to price created plorts", ex);
        }
    }
}

[HarmonyPatch(typeof(PlortEconomyDirector), nameof(PlortEconomyDirector.ResetPrices))]
internal static class PlortMarketResetPatch
{
    public static void Prefix(WorldModel worldModel)
    {
        try
        {
            PlortMarket.SeedWorldModel(worldModel);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to prepare created plorts for a price reset", ex);
        }
    }
}

[HarmonyPatch(typeof(PlortEconomyDirector), nameof(PlortEconomyDirector.RegisterSold))]
internal static class PlortMarketSoldPatch
{
    public static void Prefix(PlortEconomyDirector __instance, IdentifiableType id)
    {
        try
        {
            PlortMarket.EnsureCounted(__instance, id);
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to prepare '{id?.name}' for sale", ex);
        }
    }
}