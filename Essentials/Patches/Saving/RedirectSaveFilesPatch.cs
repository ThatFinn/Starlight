using System.IO;
using Il2CppMonomiPark.SlimeRancher.Platform;
using Starlight.Storage;

namespace Starlight.Patches.Saving;

[HarmonyPatch(typeof(SystemContext), nameof(SystemContext.GetStorageProvider))]
internal static class RedirectSaveFilesPatch
{
    private static StorageProvider _provider = null;

    private static StorageProvider provider
    {
        get
        {
            if (_provider == null)
            {
                var savePath = Path.Combine(StarlightEntryPoint.dataPath, "redirectedSaves");
                Directory.CreateDirectory(savePath);
                var prov = new FileStorageProvider(savePath);
                prov.isInitialized = true;
                _provider = prov.TryCast<StorageProvider>();
            }
            return _provider;
        }
    }
    public static bool Prefix(SystemContext __instance, ref StorageProvider __result)
    {
        if (!RedirectSaveFiles.HasFlag()) return true;
        __result = provider;
        return false; 
    }
    
}
[HarmonyDontLogOnFail,HarmonyPatch(typeof(SystemContext), nameof(SystemContext.GetUserManager))]
internal static class RedirectSaveFilesPatch2
{
    private static EmptyUserManager _userManager = null;

    private static EmptyUserManager userManager
    {
        get
        {
            if (_userManager == null)
            {
                var manager = new EmptyUserManager();
                manager.userChanger = new EmptyUserChanger().TryCast<IUserChanger>();
                _userManager = manager;
            }
            return _userManager;
        }
    }
    public static bool Prefix(SystemContext __instance, ref IUserManager __result)
    {
        if (!RedirectSaveFiles.HasFlag()) return true;
        __result = userManager.TryCast<IUserManager>();
        return false; 
    }
}