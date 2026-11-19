// Originally taken from https://github.com/SlimeRancherModding/MelonSRML
// Original Code from Lionmeow
using System.Reflection;
using Il2CppSystem;
using Array = System.Array;
using Type = System.Type;

namespace Starlight.EnumPatcher;

[HarmonyPatch]
internal static class EnumInfoPatch
{
    static MethodBase TargetMethod()
    {
        return typeof(System.Enum).GetMethod("GetEnumInfo", AccessTools.all);
    }

    private static void FixEnum(object type, ref ulong[] oldValues, ref string[] oldNames)
    {
        var enumType = type as Type;
        if (EnumPatcher.TryGetRawPatch(enumType, out var patch))
        {
            var pairs = patch.GetPairs();
            var newValues = new List<ulong>(oldValues);
            var newNames = new List<string>(oldNames);

            foreach (var pair in pairs)
            {
                newValues.Add(pair.Key);
                newNames.Add(pair.Value);
            }

            oldValues = newValues.ToArray();
            oldNames = newNames.ToArray();

            Array.Sort(oldValues, oldNames, Comparer<ulong>.Default);
        }
    }

    [HarmonyPostfix]
    public static void Postfix(object enumType, ref object __result)
    {
        var enumInfoType = __result.GetType();
        var namesField = enumInfoType.GetField("Names", AccessTools.all);
        var valuesField = enumInfoType.GetField("Values", AccessTools.all);
        var names = (string[])namesField.GetValue(__result);
        var values = (ulong[])valuesField.GetValue(__result);
        FixEnum(enumType, ref values, ref names);
        namesField.SetValue(__result, names);
        valuesField.SetValue(__result, values);
    }
}

[HarmonyPatch(typeof(Il2CppSystem.Enum), nameof(Il2CppSystem.Enum.GetCachedValuesAndNames))]
internal static class EnumInfoIL2CPPPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Enum.ValuesAndNames __result, RuntimeType enumType)
    {

        ulong[] il2CppStructArray = __result.Values;
        string[] il2CppStringArray = __result.Names;
        var tryCast = enumType.TryCast<Il2CppSystem.Type>();
        if (tryCast is null) return;

        FixEnum(tryCast, ref il2CppStructArray, ref il2CppStringArray);
    }

    private static void FixEnum(Il2CppSystem.Type type, ref ulong[] oldValues, ref string[] oldNames)
    {
        if (EnumPatcher.TryGetRawPatchInIL2CPP(type, out var patch))
        {
            var pairs = patch.GetPairs();
            var newValues = new List<ulong>(oldValues);
            var newNames = new List<string>(oldNames);

            foreach (var pair in pairs)
            {
                newValues.Add(pair.Key);
                newNames.Add(pair.Value);
            }

            oldValues = newValues.ToArray();
            oldNames = newNames.ToArray();

            Array.Sort(oldValues, oldNames, Comparer<ulong>.Default);
        }
    }
}