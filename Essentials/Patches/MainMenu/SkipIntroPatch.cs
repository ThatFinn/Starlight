using System;
using Il2CppMonomiPark.SlimeRancher.Cutscene;
using Il2CppMonomiPark.SlimeRancher.Options;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using Starlight.Buttons;

namespace Starlight.Patches.MainMenu;

/// <summary>
/// A "Skip Animation" setting on the new game screen
/// </summary>
internal static class SkipIntro
{
    internal static bool Enabled;
}

/// <summary>
/// Ends the intro immediately instead of playing it
/// </summary>
[HarmonyPatch(typeof(CutsceneDirector), nameof(CutsceneDirector.SpawnIntroSequence))]
internal static class SpawnIntroSequencePatch
{
    internal static bool Prefix(CutsceneDirector __instance)
    {
        if (!SkipIntro.Enabled) return true;
        
        SkipIntro.Enabled = false;
        
        return !Finish(__instance);
    }

    private static bool Finish(CutsceneDirector director)
    {
        try
        {
            if (director == null || director._uiRoot == null) return false;

            var root = director._uiRoot.gameObject;

            root.SetActive(true);
            GameObject.Destroy(root);

            return true;
        }
        catch (Exception exception)
        {
            LogWarning($"the new game intro could not be skipped: {exception.Message}");
            return false;
        }
    }
}

/// <summary>
/// Adds the setting at the top of the new game screen's options list
/// </summary>
[HarmonyPatch(typeof(NewGameOptionsUIRoot), nameof(NewGameOptionsUIRoot.BindItemCategory))]
internal static class SkipIntroOptionPatch
{
    private static CustomOptionsButtonValues option;

    internal static void Prefix(OptionsItemCategory category)
    {
        try
        {
            if (category == null || category.items == null) return;

            var definition = Definition();

            if (definition == null || category.items.Contains(definition)) return;

            category.items.Insert(0, definition);
        }
        catch (Exception exception)
        {
            LogWarning($"the skip animation setting could not be added: {exception.Message}");
        }
    }

    /// <summary>The definition, built once and kept - a second one would register its id twice.</summary>
    private static OptionsItemDefinition Definition()
    {
        option ??= new CustomOptionsButtonValues(
            LanguageEUtil.AddTranslationFromStarlight("options.skipintro.label"),
            LanguageEUtil.AddTranslationFromStarlight("options.skipintro.details"),
            new Action<int>(index => SkipIntro.Enabled = index == 1),
            LanguageEUtil.AddTranslationFromStarlight("options.skipintro.off"),
            LanguageEUtil.AddTranslationFromStarlight("options.skipintro.on"));

        return option.GetOptionsItemDef();
    }
}