using Starlight.Prism.Data.Spawning;

namespace Starlight.Prism.Patches;

/// <summary>
/// Applies registered spawn rules to slime spawners
/// </summary>
[HarmonyPatch(typeof(DirectedActorSpawner), nameof(DirectedActorSpawner.Awake))]
internal static class SpawnerAwakePatch
{
    public static void Postfix(DirectedActorSpawner __instance)
    {
        if (!StarlightEntryPoint.prismEnableSpawning) return;

        try
        {
            SpawnRegistry.ApplyTo(__instance);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to apply spawn rules to spawner", ex);
        }
    }
}