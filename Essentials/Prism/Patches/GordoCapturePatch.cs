using System.Linq;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Starlight.Prism.Lib;
using Starlight.Storage;

namespace Starlight.Prism.Patches;

[PrismPatch()]
[HarmonyPatch(typeof(SnareModel),nameof(SnareModel.GetGordoIdForBait))]
internal class GordoCapturePatch
{
    public static void Postfix(SnareModel __instance, ref IdentifiableType __result)
    {
        var pair = PrismLibGordo.GordoBaitDict.FirstOrDefault(x => x.Key == __instance.baitTypeId.ReferenceId);
        
        if (pair.Value!=null)
            __result = pair.Value;
    }
}