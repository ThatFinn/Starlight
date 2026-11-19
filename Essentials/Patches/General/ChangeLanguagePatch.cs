using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using Il2CppSystem.Globalization;
using Starlight.Managers;

namespace Starlight.Patches.General;

[HarmonyPatch(typeof(LocalizationDirector), nameof(LocalizationDirector.SetLocale))]
internal class ChangeLanguagePatch
{
    internal static void Postfix(LocalizationDirector __instance, UnityEngine.Localization.Locale locale)
    {
        FixLanguage(__instance, locale);
        ExecuteInTicks((() => { FixLanguage(__instance, locale);}), 10);

        ExecuteInTicks(Prism.Creators.PrismLargoCreatorV02.RenameAll, 30);
    }

    static void FixLanguage(LocalizationDirector director, UnityEngine.Localization.Locale curLocale)
    {
        var code = curLocale.Formatter.Cast<CultureInfo>()._name;

        LoadLanguage(code);

        StarlightLanguageManager.StarlightToSRLanguage.Clear();
        StarlightLanguageManager.ForgetStarlightEntries();
        foreach (var str in StarlightLanguageManager.StarlightReplaceOnLanguageChange)
        {
            var localized = StarlightLanguageManager.AddStarlightEntry(Tr(str.Key), str.Value.Item1, str.Value.Item2);

            var original = str.Value.Item3;

            original.m_TableEntryReference = localized.TableEntryReference;
            original.m_TableReference = localized.TableReference;
        }
    }
}