using System;
using Il2CppMonomiPark.SlimeRancher.Options;
using Il2CppMonomiPark.SlimeRancher.UI.Options;
using Starlight.Buttons.Definitions;
using Starlight.Managers;

namespace Starlight.Patches.Options;

[HarmonyPatch(typeof(OptionsUIRoot), nameof(OptionsUIRoot.SwapCategory))]
internal static class OptionsUIRootSwapCategoryPatch
{
    public static void Prefix(OptionsUIRoot __instance, int categoryIndex)
    {
        if (__instance.optionsItemModels == null) __instance.optionsItemModels = new Il2CppSystem.Collections.Generic.List<IOptionsItemModel>();
        if (!InjectOptionsButtons.HasFlag()) return;
        var c = __instance.categories[categoryIndex];
        OptionsItemCategory category = null;
        if (c.TryCast<OptionsItemCategory>() != null)
            category = c.Cast<OptionsItemCategory>();
        if (category == null) return;
        foreach (var def in category.items.ToNetList())
        {
            if (def is CustomOptionsSliderDefinition sliderDef)
            {
                IOptionsItemModel sliderModel = null;
                try { sliderModel = sliderDef.CreateOptionItemModel()?.TryCast<IOptionsItemModel>(); }
                catch (Exception e) { LogError(e); }

                if (sliderModel != null && !__instance.optionsItemModels.Contains(sliderModel))
                    __instance.optionsItemModels.Insert(Math.Clamp(sliderDef.button.InsertIndex, 0, __instance.optionsItemModels.Count), sliderModel);

                continue;
            }
            if (def!=null)
                if (def is CustomOptionsValuesDefinition customDef && def.ReferenceId.StartsWithAny("setting.sr2eexclude","setting.starlightexclude"))
                {
                    IOptionsItemModel model = null;
                    try
                    {
                        dynamic modelTMP = customDef.CreateOptionItemModel();
                        if(modelTMP!=null) model=modelTMP.TryCast<IOptionsItemModel>();
                    }catch (Exception e) { LogError(e); }
                    
                    if (!string.IsNullOrWhiteSpace(customDef.button.saveid))
                    {
                        var value = StarlightOptionsButtonManager.GetValuesButton(customDef.button.Type,customDef.button.saveid, customDef.button.defaultValueIndex);
                        try
                        {
                            customDef.SetTempPresetIndex(value);
                        } catch {}
                        try
                        {
                            if (model != null)
                                model.ApplyTemporaryValue();
                        } catch {}
                    }
                    if(model!=null)
                        if (!__instance.optionsItemModels.Contains(model))
                        {
                            __instance.optionsItemModels.Insert(Math.Clamp(customDef.button.InsertIndex,0,__instance.optionsItemModels.Count),model);
                        }
                }
        }
    }
}