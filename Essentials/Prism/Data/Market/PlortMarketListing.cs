using System;
using System.Linq;
using Il2CppMonomiPark.SlimeRancher.UI;
using Starlight.Prism.Lib;

namespace Starlight.Prism.Data.Market;

/// <summary>
/// Lists created plorts on the Plort Market
/// </summary>
public static class PlortMarketListing
{
    private readonly record struct ListedPlort(IdentifiableType Plort, string Mod);

    private static readonly List<ListedPlort> Listed = new();
    
    private static readonly HashSet<IntPtr> Excluded = new();

    /// <summary>
    /// Adds a plort to the board
    /// </summary>
    public static void Add(IdentifiableType plort)
    {
        ArgumentNullException.ThrowIfNull(plort);

        if (Listed.Any(listed => listed.Plort.Pointer == plort.Pointer)) return;

        Listed.Add(new ListedPlort(plort, ContentNames.SourceModId ?? string.Empty));
        Listed.Sort((first, second) =>
        {
            var byMod = string.CompareOrdinal(first.Mod, second.Mod);
            return byMod != 0 ? byMod : string.CompareOrdinal(first.Plort.ReferenceId, second.Plort.ReferenceId);
        });
    }

    /// <summary>
    /// Keeps the board from filling a configuration that something else manages
    /// </summary>
    internal static void Exclude(MarketUIConfiguration config) => Excluded.Add(config.Pointer);

    /// <summary>
    /// How many plorts one board can show
    /// </summary>
    public static int CapacityOf(MarketUI board) =>
        board.pricesPanels.AsEnumerable()
            .Where(panel => panel != null)
            .Sum(panel => panel.entryCount);

    internal static void ApplyTo(MarketUI board)
    {
        if (!StarlightEntryPoint.prismShowModdedPlorts || Listed.Count == 0) return;

        if (board == null || board._config == null || board.pricesPanels == null)
        {
            LogWarning("Prism: The market board has no configuration or panels");
            return;
        }

        if (Excluded.Contains(board._config.Pointer)) return;
        
        var screens = StarlightEntryPoint.prismSecondMarketScreen ? 2 : 1;
        var room = CapacityOf(board) * screens - (board._config._plorts?.Length ?? 0);

        AddToConfiguration(board._config, Math.Max(0, room));
    }

    private static void AddToConfiguration(MarketUIConfiguration config, int room)
    {
        var added = 0;

        foreach (var (plort, _) in Listed)
        {
            if (added >= room) break;
            if (plort == null || config.FindIndex(plort) >= 0) continue;

            config._plorts = config._plorts.AddToNew(new PlortEntry { IdentType = plort });
            added++;
        }
    }
}

[HarmonyPatch(typeof(MarketUI), nameof(MarketUI.Start))]
internal static class PlortMarketListingPatch
{
    public static void Prefix(MarketUI __instance)
    {
        try
        {
            PlortMarketListing.ApplyTo(__instance);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to add created plorts to the market board", ex);
        }
    }
}
