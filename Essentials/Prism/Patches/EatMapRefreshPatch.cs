using Starlight.Prism.Creators;

namespace Starlight.Prism.Patches;

/// <summary>
/// Restores Prism's diet entries
/// </summary>
[HarmonyPatch(typeof(SlimeDiet), nameof(SlimeDiet.RefreshEatMap))]
internal static class EatMapRefreshPatch
{
    public static void Postfix(SlimeDefinition definition)
    {
        if (definition == null) return;

        try
        {
            PrismSlimeDietCreatorV02.ReapplyAfterRefresh(definition);
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to restore diet entries for '{definition.name}'", ex);
        }
    }
}