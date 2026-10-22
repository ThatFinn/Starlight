using System;
using System.Reflection;

namespace Starlight.Patches.InGame;

internal static class WorldPopulatorErrorPatch
{
    // IEnumerators get turned into classes and have weird suffixes like _d_5_
    // In the interest of not fixing this every version and in between Store versions, I made this
    // We love Il2CPP
    
    // Shark:
    // Since 1.3.0 the populate steps are plain static methods that take the fail callback as a parameter
    // I love IL2CPP
    private static readonly string[] PatchableMethods =
        { "PopulateRanch", "PopulateActors", "PopulateDrones", "PopulateGadgets" };

    public static void Apply(HarmonyLib.Harmony harmony)
    {
        var prefixMethod = typeof(WorldPopulatorErrorPatch).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.Public);
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (var methodName in PatchableMethods)
        {
            var method = typeof(WorldPopulator).GetMethod(methodName, flags);
            if (method == null)
            {
                LogError($"Could not find WorldPopulator.{methodName}");
                continue;
            }
            harmony.Patch(method, new HarmonyMethod(prefixMethod));
        }
    }

    public static void Prefix(MethodBase __originalMethod, ref WorldPopulator.OnFailDelegate onFail)
    {
        if (!ShowWorldPopulatorErrors.HasFlag()) return;
        
        var coroutineName = __originalMethod.Name;
        WorldPopulator.OnFailDelegate newAction = new Action<Il2CppSystem.Exception>(exception =>
            LogError($"Coroutine exception in {coroutineName}:\n{exception.ToString()}"));
        
        if (IgnoreWorldPopulatorErrors.HasFlag() || onFail == null)
            onFail = newAction;
        else
            onFail += newAction;
    }
}