using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.UI.ButtonBehavior;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu.Model;

namespace Starlight.Patches.MainMenu;

/// <summary>
/// A "New Save" entry at the top of the save selection list
/// </summary>
internal static class NewSaveRow
{
    /// <summary>
    /// Non-empty save slots :P
    /// </summary>
    internal static readonly HashSet<int> LockedSlots = new();

    /// <summary>
    /// Whether the entry is in the list
    /// </summary>
    internal static bool Present { get; set; }

    /// <summary>
    /// The slot number a model index refers to
    /// </summary>
    internal static int SlotOf(int index) => Present ? index : index + 1;

    /// <summary>
    /// Whether an index points at the entry rather than at a save slot
    /// </summary>
    internal static bool IsRow(int index) => Present && index == 0;
}

/// <summary>
/// Puts the entry at the front of the list
/// </summary>
[HarmonyPatch(typeof(SaveGamesRootUI), nameof(SaveGamesRootUI.CreateBehaviorModels))]
internal static class CreateBehaviorModelsPatch
{
    private static ButtonBehaviorDefinition definition;

    internal static void Postfix(SaveGamesRootUI __instance, ref Il2CppReferenceArray<ButtonBehaviorModel> __result)
    {
        NewSaveRow.Present = false;

        if (__result == null || __result.Length == 0) return;

        var target = FirstFree(__result);
        
        if (target == null) return;

        var row = Build(__instance, target);

        if (row == null) return;
        
        var expanded = new Il2CppReferenceArray<ButtonBehaviorModel>(__result.Length + 1);
        expanded[0] = row;

        for (var i = 0; i < __result.Length; i++)
            expanded[i + 1] = __result[i];

        __result = expanded;
        NewSaveRow.Present = true;
    }

    /// <summary>
    /// The first slot that is empty
    /// </summary>
    private static OpenDisplayBehaviorModel FirstFree(Il2CppReferenceArray<ButtonBehaviorModel> models)
    {
        for (var i = 0; i < models.Length; i++)
        {
            if (NewSaveRow.LockedSlots.Contains(i + 1)) continue;
            if (models[i] == null || models[i].TryCast<LoadGameBehaviorModel>() != null) continue;

            var free = models[i].TryCast<OpenDisplayBehaviorModel>();

            if (free != null) return free;
        }

        return null;
    }

    private static ButtonBehaviorModel Build(SaveGamesRootUI screen, OpenDisplayBehaviorModel target)
    {
        try
        {
            if (definition == null)
            {
                if (screen._emptySlotBehavior == null) return null;

                definition        = GameObject.Instantiate(screen._emptySlotBehavior);
                definition.name   = "StarlightNewSaveSlot";
                definition._label = LanguageEUtil.AddTranslationFromStarlight("messages.save.new.label");
            }

            var created = definition.CreateButtonBehaviorModel();

            if (created == null) return null;

            var row = created.TryCast<OpenDisplayBehaviorModel>();

            if (row == null) return null;
            
            row.OpenWithData = target.OpenWithData;

            return row;
        }
        catch (Exception exception)
        {
            LogWarning($"the new save row entry could not be built: {exception.Message}");
            return null;
        }
    }
}