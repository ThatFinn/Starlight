using MelonLoader;
using Starlight.Expansion;
using Starlight.Patches.Context;

namespace Starlight.Managers;

public static class StarlightCounterGateManager
{
    private static readonly List<object> UseOcclusionCullingList = new ();
    private static readonly List<object> DisableCheatsList = new ();
    private static readonly List<object> LockPackages = new ();
    // ReSharper disable once InconsistentNaming
    private static readonly List<object> SRLEActive = new ();
    public static bool playerCameraUseOcclusionCulling => UseOcclusionCullingList.Count == 0;
    public static bool disableCheats => DisableCheatsList.Count != 0;
    public static bool packagesLocked => LockPackages.Count != 0;
    public static bool srleActive => SRLEActive.Count != 0;

    internal static void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        RefreshOcclusionCulling();
    }
    static void RefreshDisableCheats()
    {
        try
        {
            if(GameContextPatch.CheatMenuButton!=null)
                if(disableCheats)
                    GameContextPatch.CheatMenuButton.Remove();
                else
                    GameContextPatch.CheatMenuButton.AddAgain();
        } catch { }
    }
    static void RefreshOcclusionCulling()
    {
        try
        {
            foreach (var cam in Camera.allCameras)
                if(cam.name.Contains("Player")||cam.name.Contains("SRLECamera"))
                    cam.useOcclusionCulling = playerCameraUseOcclusionCulling;
        } catch { }
    }
    public static void RegisterFor_PlayerCameraDisableUseOcclusionCulling(this StarlightExpansionVXX expansion)
    {
        if (!UseOcclusionCullingList.Contains(expansion)) UseOcclusionCullingList.Add(expansion);
        RefreshOcclusionCulling();
    }
    public static void DeregisterFor_PlayerCameraDisableUseOcclusionCulling(this StarlightExpansionVXX expansion)
    {
        if (!UseOcclusionCullingList.Contains(expansion)) UseOcclusionCullingList.Remove(expansion);
        RefreshOcclusionCulling();
    }
    public static void RegisterFor_PlayerCameraDisableUseOcclusionCulling(this MelonBase melon)
    {
        if (!UseOcclusionCullingList.Contains(melon)) UseOcclusionCullingList.Add(melon);
        RefreshOcclusionCulling();
    }
    public static void DeregisterFor_PlayerCameraDisableUseOcclusionCulling(this MelonBase melon)
    {
        if (!UseOcclusionCullingList.Contains(melon)) UseOcclusionCullingList.Remove(melon);
        RefreshOcclusionCulling();
    }
    
    public static void RegisterFor_DisableCheats(this StarlightExpansionVXX expansion)
    {
        if (!DisableCheatsList.Contains(expansion)) DisableCheatsList.Add(expansion);
        RefreshDisableCheats();
    }
    public static void DeregisterFor_DisableCheats(this StarlightExpansionVXX expansion)
    {
        if (!DisableCheatsList.Contains(expansion)) DisableCheatsList.Remove(expansion);
        RefreshDisableCheats();
    }
    public static void RegisterFor_DisableCheats(this MelonBase melon)
    {
        if (!DisableCheatsList.Contains(melon)) DisableCheatsList.Add(melon);
        RefreshDisableCheats();
    }
    public static void DeregisterFor_DisableCheats(this MelonBase melon)
    {
        if (!DisableCheatsList.Contains(melon)) DisableCheatsList.Remove(melon);
        RefreshDisableCheats();
    }
    
    public static void RegisterFor_LockPackages(this StarlightExpansionVXX expansion)
    {
        if (!LockPackages.Contains(expansion)) LockPackages.Add(expansion);
    }
    public static void DeregisterFor_LockPackages(this StarlightExpansionVXX expansion)
    {
        if (!LockPackages.Contains(expansion)) LockPackages.Remove(expansion);
    }
    public static void RegisterFor_LockPackages(this MelonBase melon)
    {
        if (!LockPackages.Contains(melon)) LockPackages.Add(melon);
    }
    public static void DeregisterFor_LockPackages(this MelonBase melon)
    {
        if (!LockPackages.Contains(melon)) LockPackages.Remove(melon);
    }
    
    public static void RegisterFor_SRLEActive(this StarlightExpansionVXX expansion)
    {
        if (!SRLEActive.Contains(expansion)) SRLEActive.Add(expansion);
    }
    public static void DeregisterFor_SRLEActive(this StarlightExpansionVXX expansion)
    {
        if (!SRLEActive.Contains(expansion)) SRLEActive.Remove(expansion);
    }
    public static void RegisterFor_SRLEActive(this MelonBase melon)
    {
        if (!SRLEActive.Contains(melon)) SRLEActive.Add(melon);
    }
    public static void DeregisterFor_SRLEActive(this MelonBase melon)
    {
        if (!SRLEActive.Contains(melon)) SRLEActive.Remove(melon);
    }
}