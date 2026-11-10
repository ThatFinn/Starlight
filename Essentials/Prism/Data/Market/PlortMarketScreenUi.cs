using System;
using UObject = UnityEngine.Object;
using Il2CppMonomiPark.SlimeRancher.UI;
using UnityEngine.UI;

namespace Starlight.Prism.Data.Market;

/// <summary>
/// Adds a second market price screen UI
/// </summary>
public static class PlortMarketScreenUi
{
    private const string CopySuffix = " (upper)";

    private static GameObject? copy;

    public static bool IsCopy(MarketUI board) =>
        board != null && board.gameObject.name.EndsWith(CopySuffix, StringComparison.Ordinal);

    /// <summary>
    /// Copies the display onto the second screen
    /// and displays the plorts that did not fit on the original market screen
    /// </summary>
    public static void Place(MarketUI board)
    {
        if (!StarlightEntryPoint.prismSecondMarketScreen || board == null || IsCopy(board)) return;
        if (!PlortMarketScreenObject.Apply() || PlortMarketScreenObject.UpperScreenOffset is not { } offset) return;

        if (copy != null) return;

        try
        {
            var source = board.gameObject;
            var created = UObject.Instantiate(source, source.transform.parent);
            created.name = source.name + CopySuffix;

            if (created.transform.parent?.GetComponent<LayoutGroup>() != null)
                (created.GetComponent<LayoutElement>() ?? created.AddComponent<LayoutElement>()).ignoreLayout = true;

            created.transform.position   = source.transform.position + offset;
            created.transform.rotation   = source.transform.rotation;
            created.transform.localScale = source.transform.localScale;

            DisplayOtherPlorts(board, created);

            created.SetActive(true);
            copy = created;
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to place the second market screen UI", ex);
        }
    }

    /// <summary>
    /// Changes the visibility of the second market screen UI
    /// </summary>
    public static void SetVisible(bool visible)
    {
        if (copy != null) copy.SetActive(visible);
    }

    private static void DisplayOtherPlorts(MarketUI board, GameObject created)
    {
        var display = created.GetComponent<MarketUI>();
        var capacity = PlortMarketListing.CapacityOf(board);
        if (display == null || board._config == null || capacity <= 0) return;

        var all = board._config._plorts;

        var cfg = ScriptableObject.CreateInstance<MarketUIConfiguration>();
        cfg.hideFlags = HideFlags.DontUnloadUnusedAsset;
        cfg._plorts = all.SkipToNew(capacity);

        PlortMarketListing.Exclude(cfg);
        display._config = cfg;
        
        if (all.Length > capacity)
            board._config._plorts = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PlortEntry>(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(all, capacity)));
    }
}

[HarmonyPatch(typeof(MarketUI), nameof(MarketUI.Start))]
[HarmonyPriority(Priority.Low)]
internal static class PlortMarketScreenUiPatch
{
    public static void Prefix(MarketUI __instance)
    {
        try
        {
            PlortMarketScreenUi.Place(__instance);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to place the second market screen UI", ex);
        }
    }
}