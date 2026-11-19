/*using Il2CppMonomiPark.SlimeRancher.Options;
using Il2CppMonomiPark.SlimeRancher.Persist;
// Not working yet :(
namespace Starlight.Patches.Options;

[HarmonyPatch(typeof(OptionsDirector), nameof(OptionsDirector.PushSettings))]
internal static class OptionsFixer
{
    internal static void Prefix(OptionsDirector __instance, OptionsV02 optionsData)
    {
        try
        {
            var allowedIds = new List<string>();
            foreach (var def in __instance._optionsItemDefinitions._profileBasedOptionItems._items)
                allowedIds.Add(def.ReferenceId);
            var modifiedList = new List<OptionItemDataV01>();
            foreach (var data in optionsData.OptionItems._items)
            {
                try
                {
                    if(allowedIds.Contains(data.PersistenceKey))
                        modifiedList.Add(data);
                }
                catch
                {
                    if(data==null||string.IsNullOrEmpty(data.PersistenceKey)) Log("OptionsFixer: Removed invalid entry!");
                    else Log("OptionsFixer: Removed invalid entry: "+data.PersistenceKey+"!");
                }
            }
            optionsData.OptionItems = modifiedList.ToIl2CppList();
        }
        catch (Exception e) { LogError(e); }
    }

    
}*/