using System;
using Il2CppMonomiPark.SlimeRancher.Slime;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(PlortRelationships), nameof(PlortRelationships.TryGetSlimeByPlort))]
internal static class MissingPlortRelationsPatch
{
    private static readonly (string Plort, string Slime)[] Missing =
    {
        ("IdentifiableType.HyperPlort", "SlimeDefinition.Hyper"),
    };

    private static readonly HashSet<IntPtr> Completed = new();

    private static bool filling;

    public static void Prefix(PlortRelationships __instance, IdentifiableType plort)
    {
        if (filling || __instance == null || plort == null || Completed.Contains(__instance.Pointer)) return;

        try
        {
            var map = __instance._slimeDefinitionsByPlort;
            if (map == null) return;

            if (map.Count == 0)
            {
                filling = true;
                try { __instance.TryGetSlimeByPlort(plort, out _); }
                finally { filling = false; }
            }

            if (map.Count == 0) return;

            var resolved = true;

            foreach (var (plortId, slimeId) in Missing)
            {
                var missingPlort = LookupEUtil.GetIdentifiableTypeByRefID(plortId);
                var slime = LookupEUtil.GetIdentifiableTypeByRefID(slimeId)?.TryCast<SlimeDefinition>();

                if (missingPlort == null || slime == null)
                {
                    resolved = false;
                    continue;
                }

                if (!map.ContainsKey(missingPlort)) map.Add(missingPlort, slime);
            }

            if (resolved) Completed.Add(__instance.Pointer);
        }
        catch (Exception ex)
        {
            Completed.Add(__instance.Pointer);
            LogError("Failed to add the missing plort relationships", ex);
        }
    }
}