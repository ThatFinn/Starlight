using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Persist;

namespace Starlight.Patches.Options.Fixer;

[HarmonyPatch(typeof(OptionsModel), nameof(OptionsModel.Pull))]
internal static class OptionsModelPullPatch
{
    internal static void Postfix(OptionsV02 persistence) => Prefix(persistence);
    internal static void Prefix(OptionsV02 persistence)
    {
        try
        {
            foreach (var entry in persistence.OptionItems.ToNetList())
            {
                if(entry!=null)
                    if (entry.PersistenceKey.StartsWithAny("setting.sr2eexclude","setting.starlightexclude"))
                        if(persistence.OptionItems.Contains(entry))
                            persistence.OptionItems.Remove(entry);
            }
        }
        catch (Exception e) { LogError(e); }
    }
}