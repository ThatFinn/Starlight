using System;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Lib;
using Starlight.Storage;

namespace Starlight.Prism.Patches.Actions;

/// <summary>
/// Invokes the Slimepedia custom unlock action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(PediaDirector), nameof(PediaDirector.Unlock), new Type[] { typeof(PediaEntry), typeof(bool) })]
internal static class PediaUnlockPatch
{
    public static void Postfix(PediaEntry entry, bool showPopup, bool __result)
    {
        if (!__result) return;

        try
        {
            PediaEvents.NotifyUnlock(entry, showPopup);
        }
        catch (Exception ex)
        {
            LogError("Prism: A pedia entry's custom unlock action failed", ex);
        }
    }
}