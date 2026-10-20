using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.ScriptedValue;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.Options;
using Il2CppMonomiPark.SlimeRancher.Platform;
using Starlight.Buttons.Definitions;
using Starlight.Enums;
using Starlight.Managers;
using UnityEngine.Localization;
using QualityLevel = Il2CppMonomiPark.ScriptedValue.QualityLevel;

namespace Starlight.Buttons;

public class CustomOptionsButtonValues : CustomAbstractOptionsButton
{
    public LocalizedString label;
    public LocalizedString detailsText;
    public bool applyImmediately = true;
    public bool wrapAround = false;
    public bool requireConfirmation = false;
    public int defaultValueIndex = 0;
    public string saveid = null;
    public Action<int> onModify;
    public LocalizedString[] values;
    internal string refID;
    private static ScriptedValuePresetOptionDefinition info;
    protected override OptionsItemDefinition GenerateOptionsItemDefinition()
    {
        if(!string.IsNullOrWhiteSpace(saveid))
            StarlightOptionsButtonManager.InitializeValuesButton(Type,saveid, defaultValueIndex);
        if(info==null) info = GetAny<ScriptedValuePresetOptionDefinition>();
        var instance = ScriptableObject.CreateInstance<CustomOptionsValuesDefinition>();
        
        instance.name = label.GetLocalizedString().Replace(" ", "");
        instance._wrapAround = wrapAround;
        var partID = "starlightexclude"+MiscEUtil.GetRandomString(20);
        while (true)
        {
            if (UsedIds.Contains(partID))
                partID = "starlightexclude" + MiscEUtil.GetRandomString(20);
            else break;
        }

        refID = "setting." + partID;
        instance._referenceId = refID;
        instance._label = label;
        instance._detailsText = detailsText;
        instance._applyImmediately = applyImmediately;
        instance._requireConfirmation = requireConfirmation;
        instance._defaultValueIndex = defaultValueIndex;
        if(!string.IsNullOrWhiteSpace(saveid))
            instance.SetTempPresetIndex(StarlightOptionsButtonManager.GetValuesButton(Type,saveid, defaultValueIndex));
        instance._optionsItemModels = new Il2CppSystem.Collections.Generic.List<PresetOptionsItemModel>();
        instance.SupportedInputDeviceAssets = new Il2CppSystem.Collections.Generic.List<InputDeviceAsset>();
        instance.SupportedPlatforms = new Il2CppSystem.Collections.Generic.List<StoreAndPlatform>();
        instance._isProfileSetting = true;
        instance.button = this;
        instance._showTutorialDisclaimer = false;
        instance._controlPrefab = info._controlPrefab;
        instance._confirmationPopupConfig = info._confirmationPopupConfig;
        var presets = new Il2CppReferenceArray<ScriptedValuePresetOptionDefinition.ScriptedValuePreset>(values.Length);
        var i = 0;
        foreach (var entry in values)
        {
            var preset = new CustomOptionsValuesPreset();
            preset._presetLabel = entry;
            preset._referenceId = partID+i;
            preset._scriptedBoolSettings = new Il2CppReferenceArray<ScriptedValuePresetOptionDefinition.ScriptedValueSetting<ScriptedBool, bool>>(0);
            preset._scriptedFloatSettings = new Il2CppReferenceArray<ScriptedValuePresetOptionDefinition.ScriptedValueSetting<ScriptedFloat, float>>(0);
            preset._scriptedIntSettings = new Il2CppReferenceArray<ScriptedValuePresetOptionDefinition.ScriptedValueSetting<ScriptedInt, int>>(0);
            preset._scriptedQualitySettings = new Il2CppSystem.Collections.Generic.List<ScriptedValuePresetOptionDefinition.ScriptedValueSetting<ScriptedQuality, QualityLevel>>();
            presets[i]=preset;
            i++;
        }
        instance._optionsPresets = presets;
        instance._referenceId = refID;
        try { instance.CreateOptionItemModel(); }catch { }
        instance._referenceId = refID;
        gameContext.OptionsDirector._optionsModel.optionsById.Add(refID, instance._optionsItemModels[0].TryCast<OptionsItemModel>());
        return instance;
    }

    
    
    
    public CustomOptionsButtonValues(LocalizedString label, Action<int> onModify, params LocalizedString[] values)
    {
        this.label = label;
        this.label = label;
        this.onModify = onModify;
        this.values = values;
    }
    public CustomOptionsButtonValues(LocalizedString label, LocalizedString detailsText, Action<int> onModify, params LocalizedString[] values)
    {
        this.label = label;
        this.detailsText = detailsText;
        this.label = label;
        this.onModify = onModify;
        this.values = values;
    }
    public CustomOptionsButtonValues(LocalizedString label, LocalizedString detailsText, string saveid, Action<int> onModify, params LocalizedString[] values)
    {
        this.saveid = saveid;
        this.label = label;
        this.detailsText = detailsText;
        this.label = label;
        this.onModify = onModify;
        this.values = values;
    }
    public CustomOptionsButtonValues(
        LocalizedString label, LocalizedString detailsText,
        int defaultValueIndex, bool applyImmediately,
        bool wrapAround, bool requireConfirmation, Action<int> onModify, 
        params LocalizedString[] values)
    {
        this.saveid = saveid;
        this.label = label;
        this.detailsText = detailsText;
        this.label = label;
        this.defaultValueIndex = defaultValueIndex;
        this.applyImmediately = applyImmediately;
        this.wrapAround = wrapAround;
        this.requireConfirmation = requireConfirmation;
        this.onModify = onModify;
        this.values = values;
    }
    public CustomOptionsButtonValues(
        LocalizedString label, LocalizedString detailsText,
        string saveid, int defaultValueIndex, bool applyImmediately,
        bool wrapAround, bool requireConfirmation, Action<int> onModify, 
        params LocalizedString[] values)
    {
        this.saveid = saveid;
        this.label = label;
        this.detailsText = detailsText;
        this.label = label;
        this.defaultValueIndex = defaultValueIndex;
        this.applyImmediately = applyImmediately;
        this.wrapAround = wrapAround;
        this.requireConfirmation = requireConfirmation;
        this.onModify = onModify;
        this.values = values;
    }
    public CustomOptionsButtonValues(
        LocalizedString label, LocalizedString detailsText,
        string saveid, int defaultValueIndex, bool applyImmediately,
        bool wrapAround, bool requireConfirmation, Action<int> onModify, OptionsButtonType type,
        params LocalizedString[] values)
    {
        this.saveid = saveid;
        this.label = label;
        this.detailsText = detailsText;
        this.label = label;
        this.defaultValueIndex = defaultValueIndex;
        this.applyImmediately = applyImmediately;
        this.wrapAround = wrapAround;
        this.requireConfirmation = requireConfirmation;
        this.onModify = onModify;
        this.Type = type;
        this.values = values;
    }
    
}