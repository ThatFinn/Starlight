namespace Starlight.Patches.InGame;
/*
 
   BROKEN SINCE 1.0.0!
   Needs fixing
   
[HarmonyPatch(typeof(AccessDoor), nameof(AccessDoor.Awake))]
internal class AccessDoorPatch
{
    internal static void Postfix(AccessDoor __instance)
    {
        RanchCommand.accessDoors.Add(__instance);
        RanchCommand.accessDoors.RemoveAll(item => item == null);
        try
        {
            __instance.CurrState = autoSaveDirector.SavedGame.GameState.Ranch.AccessDoorStates[__instance.Id];
        }
        catch {}
        
    }
}
*/