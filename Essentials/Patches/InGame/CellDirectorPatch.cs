namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(CellDirector), nameof(CellDirector.Start))]
internal class CellDirectorPatch
{
    internal static void Postfix(CellDirector __instance)
    {
        if (__instance.name == "cellConservatory")
        {
            var toFix = __instance.transform.Find("Sector/cellLabCave/Sector/FX/PortalCard - Cave (2)");
            toFix.position = new Vector3(toFix.position.x, 7, toFix.position.z);
        }
    }
}