using System;
using System.Text.RegularExpressions;
using Il2CppMonomiPark.ScriptedValue;
using Il2CppMonomiPark.SlimeRancher.Options;
using Starlight.Buttons.Definitions;
using Starlight.Enums;
using Starlight.Managers;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Starlight.Buttons;

/// <summary>
/// A row for a whole number between a minimum and a maximum, stepped with arrows.
/// </summary>
/// <remarks>
/// The game's rows are integer steppers,
/// a fractional setting is shown as a scaled whole number (150 for 1.5)
/// </remarks>
public class CustomOptionsButtonSlider : CustomAbstractOptionsButton
{
    public LocalizedString label;
    public LocalizedString detailsText;
    public bool applyImmediately = true;
    public bool requireConfirmation = false;
    public int defaultValue = 0;
    public int minValue = 0;
    public int maxValue = 100;
    public int stepSize = 1;
    public string saveid = null;

    /// <summary>
    /// Text shown for the value,
    /// with <c>{0}</c> where the number goes, like <c>{0}%</c>.
    /// Null keeps the game's own format.
    /// </summary>
    public string displayFormat = null;
    public Action<int> onModify;
    internal string refID;
    private static ScriptedFloatOptionDefinition info;

    protected override OptionsItemDefinition GenerateOptionsItemDefinition()
    {
        if (!string.IsNullOrWhiteSpace(saveid))
            StarlightOptionsButtonManager.InitializeValuesButton(Type, saveid, defaultValue);

        if (info == null) info = GetAny<ScriptedFloatOptionDefinition>();

        var instance = ScriptableObject.CreateInstance<CustomOptionsSliderDefinition>();

        instance.name = label.GetLocalizedString().Replace(" ", "");

        var partID = "starlightexclude" + MiscEUtil.GetRandomString(20);
        while (UsedIds.Contains(partID)) partID = "starlightexclude" + MiscEUtil.GetRandomString(20);

        UsedIds.Add(partID);

        refID = "setting." + partID;
        instance._referenceId = refID;
        instance._label = label;
        instance._detailsText = detailsText;
        instance._applyImmediately = applyImmediately;
        instance._requireConfirmation = requireConfirmation;
        instance._isProfileSetting = true;
        instance.button = this;
        instance._showTutorialDisclaimer = false;
        instance._controlPrefab = info._controlPrefab;
        instance._confirmationPopupConfig = info._confirmationPopupConfig;
        instance._displayFormat = FormatFor();
        instance._minValue = minValue;
        instance._maxValue = Math.Max(maxValue, minValue);
        instance._stepSize = Math.Max(1, stepSize);

        var start = defaultValue;
        if (!string.IsNullOrWhiteSpace(saveid))
            start = StarlightOptionsButtonManager.GetValuesButton(Type, saveid, defaultValue);

        instance._targetValue = ScriptableObject.CreateInstance<ScriptedFloat>();
        instance._targetValue.Value = Math.Clamp(start, instance._minValue, instance._maxValue);
        instance._targetValue.OnValueChanged += (Action<float>)(value => Changed(Mathf.RoundToInt(value)));

        try
        {
            var model = instance.CreateOptionItemModel();
            var byId = gameContext.OptionsDirector._optionsModel.optionsById;
            if (model != null && !byId.ContainsKey(refID)) byId.Add(refID, model);
        }
        catch (Exception e) { LogError(e); }

        return instance;
    }

    private void Changed(int value)
    {
        if (!string.IsNullOrWhiteSpace(saveid))
            StarlightOptionsButtonManager.SetValuesButton(Type, saveid, value);

        try { onModify?.Invoke(value); }
        catch (Exception e) { LogError(e); }
    }
    
    private LocalizedString FormatFor()
    {
        if (string.IsNullOrWhiteSpace(displayFormat)) return info._displayFormat;

        try
        {
            var reference = info._displayFormat;
            var table = LocalizationSettings.StringDatabase.GetTable(reference.TableReference);
            var raw = table.GetEntryFromReference(reference.TableEntryReference).Value;
            var name = Regex.Match(raw, @"\{([A-Za-z_][A-Za-z0-9_]*)").Groups[1].Value;

            if (name.Length > 0)
                return AddTranslation(displayFormat.Replace("{0}", "{" + name + "}"), "starlight.slider." + MiscEUtil.GetRandomString(12), "UI");
        }
        catch (Exception e) { LogError(e); }

        return info._displayFormat;
    }

    public CustomOptionsButtonSlider(LocalizedString label, LocalizedString detailsText, int defaultValue, int minValue, int maxValue, int stepSize, string displayFormat, Action<int> onModify)
    {
        this.label = label;
        this.detailsText = detailsText;
        this.defaultValue = defaultValue;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.stepSize = stepSize;
        this.displayFormat = displayFormat;
        this.onModify = onModify;
    }

    public CustomOptionsButtonSlider(LocalizedString label, LocalizedString detailsText, string saveid, int defaultValue, int minValue, int maxValue, int stepSize,
        string displayFormat, bool applyImmediately, bool requireConfirmation, Action<int> onModify, OptionsButtonType type)
    {
        this.saveid = saveid;
        this.label = label;
        this.detailsText = detailsText;
        this.defaultValue = defaultValue;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.stepSize = stepSize;
        this.displayFormat = displayFormat;
        this.applyImmediately = applyImmediately;
        this.requireConfirmation = requireConfirmation;
        this.onModify = onModify;
        this.Type = type;
    }
}