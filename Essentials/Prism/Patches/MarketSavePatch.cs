using Il2CppMonomiPark.SlimeRancher;
using Starlight.Prism.Data.Market;

namespace Starlight.Prism.Patches;

/// <summary>
/// Keeps the extra market screen out of save files
/// </summary>
[HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.SaveGameImpl))]
internal static class MarketSavePatch
{
    public static void Prefix() => SetVisible(false);

    public static void Finalizer() => SetVisible(true);

    private static void SetVisible(bool visible)
    {
        try
        {
            PlortMarketScreenObject.SetVisible(visible);
            PlortMarketScreenUi.SetVisible(visible);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to toggle the extra market screen while saving", ex);
        }
    }
}