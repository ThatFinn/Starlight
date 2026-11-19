using System;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Starlight.Commands;

namespace Starlight.Patches.Saving.Fixer;

[HarmonyPriority(-99999999)]
[HarmonyPatch(typeof(GameModelPushHelpers), nameof(GameModelPushHelpers.PushRanch))]
internal static class SaveFixerPushRanch
{
    internal static void Prefix(GameModel gameModel, dynamic ranch, ILoadReferenceTranslation loadReferenceTranslation)
    {
        try
        {
            RanchCommand.LoadAutoComplete(ranch);
            if (!StarlightEntryPoint.disableFixSaves)
            {
                foreach (var plot in ranch.Plots.ToArray())
                {
                    //Remove invalid plot
                    if (plot == null)
                        ranch.Plots.Remove(plot);
                    if (!Enum.IsDefined<LandPlot.Id>(plot.TypeId))
                        ranch.Plots.Remove(plot);
                    else
                        foreach (LandPlot.Upgrade upgrade in plot.Upgrades.ToArray())
                            if (!Enum.IsDefined<LandPlot.Upgrade>(upgrade))
                                //Remove invalid upgrade
                                plot.Upgrades.Remove(upgrade);
                }
            }
        }
        catch (Exception e) { LogError(e); }
    }
}