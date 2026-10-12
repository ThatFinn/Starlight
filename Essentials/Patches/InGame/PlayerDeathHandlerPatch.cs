using Starlight.Managers;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(PlayerDeathHandler), nameof(PlayerDeathHandler.ResetPlayer))]
internal static class PlayerDeathHandlerPatch
{
    internal static void Prefix(ref bool clearOutAmmo)
    {
        if (StarlightSaveManager.inGameData?.KeepInventoryActive == true)
            clearOutAmmo = false;
    }
}