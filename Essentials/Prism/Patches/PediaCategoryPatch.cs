using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Creators;

namespace Starlight.Prism.Patches;

/// <summary>
/// Adds created pedia entries to their category
/// </summary>
[HarmonyPatch(typeof(PediaCategory), nameof(PediaCategory.GetTypedRuntimeCategory))]
internal static class PediaCategoryPatch
{
    public static void Postfix(PediaCategory __instance, ref PediaRuntimeCategory __result)
    {
        try
        {
            var kind = PrismPediaCreatorV02.CategoryFromName(__instance.name);
            if (kind == null) return;

            if (!PrismPediaCreatorV02.PendingEntries.TryGetValue(kind.Value, out var entries)) return;

            foreach (var entry in entries)
            {
                if (!__instance._items.Contains(entry))
                    __instance._items = __instance._items.AddToNew(entry);

                if (__result != null && !__result._items.Contains(entry))
                    __result._items.Add(entry);
            }

        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to add pedia entries to '{__instance.name}'", ex);
        }
    }
}