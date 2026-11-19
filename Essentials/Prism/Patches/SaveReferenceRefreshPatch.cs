using Il2CppMonomiPark.SlimeRancher;
using Starlight.Prism.Creators;
using Starlight.Prism.Lib;

namespace Starlight.Prism.Patches;

/// <summary>
/// Re-registers created content right before the game resolves saved references
/// </summary>
[HarmonyPatch(typeof(GameModelPullHelpers), nameof(GameModelPullHelpers.PullRanch))]
internal static class SaveReferenceRefreshPatch
{
    public static void Prefix()
    {
        try
        {
            SaveRegistry.RefreshAll();

            PrismGardenCreatorV02.RefreshGrowers();

            PrismMusicCreatorV02.Refresh();
            PrismVacPaletteCreatorV02.Refresh();
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to refresh save references before loading", ex);
        }
    }
}