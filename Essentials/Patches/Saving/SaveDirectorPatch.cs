using Il2CppMonomiPark.SlimeRancher;
using Starlight.Enums;
using Starlight.Expansion;
using Starlight.Managers;
using Starlight.Prism;
using Starlight.Prism.Lib;

namespace Starlight.Patches.Saving;

[HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.Awake))]
internal static class SaveDirectorPatch
{
    internal static void Prefix(AutoSaveDirector __instance)
    {
        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
            try { expansion.BeforeSaveDirector(__instance); }
            catch (Exception e) { LogError(e); }
        StarlightCallEventManager.ExecuteWithArgs(CallEvent.BeforeSaveDirectorLoad,("saveDirector",__instance));
        
    }
    internal static void Postfix(AutoSaveDirector __instance)
    {
        LookupEUtil.IdentifiableTypeGroupList = Get<IdentifiableTypeGroupList>("All Type Groups List");

        if (StarlightEntryPoint.isPrismInUse)
            DispatchContentCreation();

        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
            try { expansion.AfterSaveDirector(__instance);
            } catch (Exception e) { LogError(e); }
        StarlightCallEventManager.ExecuteWithArgs(CallEvent.AfterSaveDirectorLoad,("saveDirector",__instance));

    }
    
    private static void DispatchContentCreation()
    {
        PrismShortcuts.EmptyTranslation = AddTranslation("");
        PrismShortcuts.UnavailableIcon = Get<Sprite>("unavailableIcon");
        PrismLibPedia.PediaDetailTypesInitialize();

        ForEachExpansion(expansion => expansion.OnPrismCreateAdditions());
        ForEachExpansion(expansion => expansion.AfterPrismCreateAdditions());
        
        foreach (var largoAction in PrismShortcuts.CreateLargoActions)
            try { largoAction.Invoke(); }
            catch (Exception e) { LogError("Prism: A largo action failed", e); }
        
        Starlight.Prism.Creators.PrismLargoCreatorV02.CommitPairings();
        ForEachExpansion(expansion => expansion.AfterPrismLargosCreated());
    }
    
    private static void ForEachExpansion(System.Action<StarlightExpansionV01> step)
    {
        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
        {
            ContentNames.SourceModName = expansion.GetPackageInfoFromExpansion()?.Name;

            try { step(expansion); }
            catch (Exception e) { LogError(e); }
        }

        ContentNames.SourceModName = null;
    }
}
