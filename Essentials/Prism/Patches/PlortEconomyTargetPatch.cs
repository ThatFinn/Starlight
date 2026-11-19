using System;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Economy;
using Starlight.Storage;

namespace Starlight.Prism.Patches;

[PrismPatch()]
[HarmonyPatch(typeof(PlortEconomyDirector),nameof(PlortEconomyDirector.GetTargetValue))]
internal class PlortEconomyGetTargetValuePatch
{
    public static bool Prefix(WorldModel worldModel, IdentifiableType id, float baseValue, float fullSaturation, float day, ref float __result)
    {
        foreach (var pair in PrismShortcuts.MarketData)
            if (pair.Key == id && pair.Value.UseSaturationAsRangeForValue)
            {
                var rng = new System.Random((int)worldModel.econSeed);

                for (int i = 0; i < ((int)day); i++)
                    rng.NextDouble();
                
                var min = baseValue - fullSaturation;
                var max = baseValue + fullSaturation;

                var randomPercent = (float)rng.NextDouble();

                var finalValue = min + (randomPercent * (max - min));

                __result = Math.Max(1, finalValue);
                return false;
            }
        return true;
    }
}