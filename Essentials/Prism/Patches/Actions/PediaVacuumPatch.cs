using System;
using Starlight.Prism.Creators;
using Starlight.Storage;

namespace Starlight.Prism.Patches.Actions;

/// <summary>
/// Unlocks what is found by vacuuming
/// </summary>
/// <remarks>
/// Unlocks the entry Prism made for created content,
/// and for a largo the entries of both base slimes
/// </remarks>
[PrismPatch]
[HarmonyPatch(typeof(PediaDirector), nameof(PediaDirector.Unlock), new Type[] { typeof(IdentifiableType), typeof(bool) })]
internal static class PediaVacuumPatch
{
    public static void Postfix(PediaDirector __instance, IdentifiableType identifiableType, bool showPopup)
    {
        if (identifiableType == null) return;

        try
        {
            UnlockFor(__instance, identifiableType, showPopup);

            var slime = identifiableType.TryCast<SlimeDefinition>();
            if (slime == null || !slime.IsLargo || slime.BaseSlimes == null) return;

            foreach (var half in slime.BaseSlimes)
                if (half != null) UnlockFor(__instance, half, showPopup);
        }
        catch (Exception ex)
        {
            LogError("Prism: Couldn't unlock the pedia entry of something vacuumed", ex);
        }
    }

    /// <summary>
    /// Unlocks an entry
    /// </summary>
    private static void UnlockFor(PediaDirector director, IdentifiableType type, bool showPopup)
    {
        var entry = director.GetEntry(type) ?? PrismPediaCreatorV02.EntryFor(type);
        if (entry == null || director.IsUnlocked(entry)) return;

        director.Unlock(entry, showPopup);
    }
}