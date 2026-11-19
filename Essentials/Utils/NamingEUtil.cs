using Il2CppMonomiPark.SlimeRancher.Economy;
using Il2CppMonomiPark.SlimeRancher.Player.Component;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.Weather;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppMonomiPark.UnitPropertySystem;
using Starlight.Prism.Lib;
using UnityEngine.Localization;

namespace Starlight.Utils;

public static class NamingEUtil
{
    //ResourceGrowerDefinition
    public static string GetName(this ResourceGrowerDefinition definition, bool addQuotesIfSpaces = true) => _GNNonLocalized(definition,addQuotesIfSpaces);
    public static string GetCompactName(this ResourceGrowerDefinition definition) => _GCNNonLocalized(definition);
    public static string GetCompactUpperName(this ResourceGrowerDefinition definition) => _GCUNNonLocalized(definition);
    
    //SceneGroup
    public static string GetName(this SceneGroup sceneGroup, bool addQuotesIfSpaces = true) => _GNNonLocalized(sceneGroup,addQuotesIfSpaces);
    public static string GetCompactName(this SceneGroup sceneGroup) => _GCNNonLocalized(sceneGroup);
    public static string GetCompactUpperName(this SceneGroup sceneGroup) => _GCUNNonLocalized(sceneGroup);

    //StatusEffectDefinition
    public static string GetName(this StatusEffectDefinition definition, bool addQuotesIfSpaces = true) => _GNNonLocalized(definition,addQuotesIfSpaces);
    public static string GetCompactName(this StatusEffectDefinition definition) => _GCNNonLocalized(definition);
    public static string GetCompactUpperName(this StatusEffectDefinition definition) => _GCUNNonLocalized(definition);

    //UpgradeComponent
    public static string GetName(this UpgradeComponent definition, bool addQuotesIfSpaces = true) => _GNNonLocalized(definition,addQuotesIfSpaces);
    public static string GetCompactName(this UpgradeComponent definition) => _GCNNonLocalized(definition);
    public static string GetCompactUpperName(this UpgradeComponent definition) => _GCUNNonLocalized(definition);
    
    //ZoneDefinition
    public static string GetName(this ZoneDefinition definition, bool addQuotesIfSpaces = true) => _GN(definition, definition._localizedName,addQuotesIfSpaces);
    public static string GetCompactName(this ZoneDefinition definition) => _GCN(definition, definition._localizedName);
    public static string GetCompactUpperName(this ZoneDefinition definition) => _GCUN(definition, definition._localizedName);
    
    //IdentifiableTypeGroup
    public static string GetName(this IdentifiableTypeGroup definition, bool addQuotesIfSpaces = true) => _GN(definition, definition._localizedName,addQuotesIfSpaces);
    public static string GetCompactName(this IdentifiableTypeGroup definition) => _GCN(definition, definition._localizedName);
    public static string GetCompactUpperName(this IdentifiableTypeGroup definition) => _GCUN(definition, definition._localizedName);
    
    //CurrencyDefinition
    public static string GetName(this CurrencyDefinition definition, bool addQuotesIfSpaces = true) => _GN(definition, definition.DisplayName,addQuotesIfSpaces);
    public static string GetCompactName(this CurrencyDefinition definition) => _GCN(definition, definition.DisplayName);
    public static string GetCompactUpperName(this CurrencyDefinition definition) => _GCUN(definition, definition.DisplayName);
    
    //IdentifiableType
    public static string GetName(this IdentifiableType type, bool addQuotesIfSpaces = true) => _GN(type, type.LocalizedName,addQuotesIfSpaces);
    public static string GetCompactName(this IdentifiableType type) => ContentNames.CommandNameOf(type) ?? _GCN(type, type.LocalizedName);
    public static string GetCompactUpperName(this IdentifiableType type) => ContentNames.CommandNameOf(type)?.ToUpper() ?? _GCUN(type, type.LocalizedName);
    
    //Identifiable
    public static string GetName(this Identifiable identifiable, bool addQuotesIfSpaces = true) => _GN(identifiable.identType, identifiable.identType.LocalizedName, addQuotesIfSpaces);
    public static string GetCompactName(this Identifiable identifiable) => identifiable.identType.GetCompactName();
    public static string GetCompactUpperName(this Identifiable identifiable) => identifiable.identType.GetCompactUpperName();
    
    //WeatherPatternDefinition
    public static string GetName(this WeatherPatternDefinition definition, bool addQuotesIfSpaces = true) => _GNNonLocalized(definition, addQuotesIfSpaces);
    public static string GetCompactName(this WeatherPatternDefinition definition) => _GCNNonLocalized(definition);
    public static string GetCompactUpperName(this WeatherPatternDefinition definition) => _GCUNNonLocalized(definition);

    //WeatherStateDefinition
    public static string GetName(this WeatherStateDefinition definition, bool addQuotesIfSpaces = true) => _GNNonLocalized(definition, addQuotesIfSpaces);
    public static string GetCompactName(this WeatherStateDefinition definition) => _GCNNonLocalized(definition);
    public static string GetCompactUpperName(this WeatherStateDefinition definition) => _GCUNNonLocalized(definition);
    
    //LocalizedString
    public static string GetLocalized(this LocalizedString localizedString, bool addQuotesIfSpaces = true)
    {
        try
        {
            string itemName;
            string name = localizedString.GetLocalizedString();
            if (addQuotesIfSpaces && name.Contains(" ")) itemName = "'" + name + "'";
            else itemName = name;
            return itemName;
        }catch { return "<No Translation>"; }
    }
    public static string GetCompactLocalized(this LocalizedString localizedString)
    {
        try
        {
            return localizedString.GetLocalizedString().Replace(" ","").Replace("_","");
        } catch { return "<NoTranslation>"; }
    }
    public static string GetCompactUpperLocalized(this LocalizedString localizedString) {
        try
        {
            return localizedString.GetLocalizedString().Replace(" ","").Replace("_","").ToUpper();
        } catch { return "<NOTRANSLATION>"; }
    }

    //General Localized
    static string _GN(Object obj, LocalizedString localizedString, bool addQuotesIfSpaces = true)
    {
        if (obj == null) return null;
        try
        {
            string itemName;
            string name = localizedString.GetLocalizedString();
            if (addQuotesIfSpaces&&name.Contains(" ")) itemName = "'" + name + "'";
            else itemName = name;
            return itemName;
        }
        catch
        { return obj.name; }
    }
    static string _GCN(Object obj, LocalizedString localizedString)
    {
        if (obj == null) return null;
        try
        {
            string itemName = localizedString.GetLocalizedString().Replace(" ","").Replace("_","");
            return itemName;
        }
        catch
        { return obj.name.Replace(" ","").Replace("_",""); }
    }
    static string _GCUN(Object obj, LocalizedString localizedString)
    {
        if (obj == null) return null;
        try
        {
            string itemName = localizedString.GetLocalizedString().Replace(" ","").Replace("_","");
            return itemName.ToUpper();
        }
        catch
        { return obj.name.Replace(" ","").Replace("_","").ToUpper(); }
    }
    
    //General Non Localized
    static string _GNNonLocalized(Object obj, bool addQuotesIfSpaces = true)
    {
        try
        {
            if (addQuotesIfSpaces&&obj.name.Contains(" ")) return "'" + obj.name + "'";
            return obj.name;
        } catch { }

        return null;
    }
    static string _GCNNonLocalized(Object obj)
    {
        try { return obj.name.Replace(" ","").Replace("_",""); } catch { }

        return null;
    }
    static string _GCUNNonLocalized(Object obj)
    {
        try { return obj.name.Replace(" ","").Replace("_","").ToUpper(); } catch { }

        return null;
    }
}