using System.IO;
using Il2CppMonomiPark.SlimeRancher.Script.Util;
using Microsoft.VisualBasic.FileIO;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Starlight.Managers;

internal static class StarlightLanguageManager
{
    internal static readonly Dictionary<string, Dictionary<string, string>> AddedTranslations = new ();
    internal static readonly Dictionary<string, LocalizedString> StarlightToSRLanguage = new ();
    internal static readonly Dictionary<string, (string, string, LocalizedString)> StarlightReplaceOnLanguageChange = new ();

    /// <summary>
    /// The entries made from Starlight's own translations
    /// </summary>
    private static readonly HashSet<(string Table, string Key)> StarlightEntries = new ();

    private static readonly Dictionary<string, List<Dictionary<string, string>>> Languages = new ();
    private static Dictionary<string, string> _loadedLanguage = new ();
    private static Dictionary<string, string> _defaultLang;
    
    
    internal static string Translation(string key)
    {
        if (string.IsNullOrEmpty(key) || !_loadedLanguage.ContainsKey(key)) return key;
        return _loadedLanguage[key];
    }
    
    internal static string Translation(string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key) || !_loadedLanguage.ContainsKey(key)) return key;
        int i = 1;
        string translatedRaw = _loadedLanguage[key];
        foreach (object obj in args)
        {
            translatedRaw = translatedRaw.Replace($"${i}", obj.ToString());
            i++;
        }
    
        return translatedRaw;
    }

    internal static void AddLanguages(string csvText)
    {
        var newLanguages = new Dictionary<string, Dictionary<string, string>>();
        var codeIndexes = new List<string>(){};
        MemoryStream stream = new MemoryStream();
        var cvsBytes = System.Text.Encoding.Default.GetBytes(csvText);
        stream.Write(cvsBytes,0,cvsBytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
        using (TextFieldParser csvParser = new TextFieldParser(stream))
        {
            csvParser.CommentTokens = new string[] { "#" };
            csvParser.SetDelimiters(new string[] { "," });
            csvParser.HasFieldsEnclosedInQuotes = true;
            
            bool firstLine = true;
            while (!csvParser.EndOfData)
            {
                string[] parts = csvParser.ReadFields();
                if (firstLine)
                {
                    firstLine = false;
                    if (parts == null) return; if (parts.Length < 1) return;
                    bool isKeys = true;
                    foreach (string code in parts)
                        if (isKeys) isKeys = false;
                        else
                        {
                            if (!newLanguages.ContainsKey(code)) newLanguages[code] = new Dictionary<string, string>();
                            codeIndexes.Add(code);
                        }
                }
                else
                {
                    if (parts == null) continue; if (parts.Length < 1) continue;
                    bool isKey = true;
                    string key = parts[0];
                    int i = 0;
                    foreach (string translation in parts)
                        if (isKey) isKey = false;
                        else
                        {
                            if(codeIndexes.Count>i) newLanguages[codeIndexes[i]][key] = translation.Replace("\\n", "\n");
                            i++;
                        }
                }
            }
        }
        foreach (var newLanguage in newLanguages)
        {
            var langCode=newLanguage.Key;
            if (!Languages.ContainsKey(langCode)) Languages.Add(langCode, new List<Dictionary<string, string>>());
            Languages[langCode].Add(newLanguage.Value);
        }
        StarlightEntryPoint.CheckFallBackFont();
    }
    internal static void LoadLanguage(string code)
    {
        _loadedLanguage = new Dictionary<string, string>();
        if (_defaultLang == null)
        {
            _defaultLang = new Dictionary<string, string>();
            foreach (var languageDicts in Languages[DEFAULT_LANGUAGECODE.Get()])
                foreach (var translation in languageDicts) 
                    _defaultLang[translation.Key] = translation.Value;
        }
        _loadedLanguage = new Dictionary<string, string>(_defaultLang);
        if (code != DEFAULT_LANGUAGECODE.Get()) if (Languages.ContainsKey(code))
            foreach (var languageDicts in Languages[code]) 
                foreach (var translation in languageDicts)
                    _loadedLanguage[translation.Key] = translation.Value;
        StarlightEntryPoint.CheckFallBackFont();
    }
    
    
    
    internal static LocalizedString AddTranslation(string localized, string key = null, string table = "Actor") =>
        AddTranslation(localized, key, table, out _);

    private static LocalizedString AddTranslation(string localized, string key, string table, out string usedKey)
    {
        usedKey = null;
        if (!InjectTranslations.HasFlag())
        { 
            var tutorial = LocalizationUtil.GetTable("Tutorial");
            foreach (var pair in tutorial.m_TableEntries) return new LocalizedString(tutorial.SharedData.TableCollectionName, pair.Value.SharedEntry.Id);
        }
        StringTable table2 = LocalizationUtil.GetTable(table);


        StringTableEntry existing = null;
        if (string.IsNullOrWhiteSpace(key))
        {
            while (true)
            {
                key = "starlighrandom."+MiscEUtil.GetRandomString(20);
                StringTableEntry curr = null;
                try
                {
                    curr = table2.GetEntry(key);
                } catch { }
                if (curr == null) break;
            }
        }
        usedKey = key;
        try { existing = table2.GetEntry(key); } catch { }
        if (existing != null) return new LocalizedString(table2.SharedData.TableCollectionName, existing.SharedEntry.Id);
        System.Collections.Generic.Dictionary<string, string> dictionary;
        if (!AddedTranslations.TryGetValue(table, out dictionary))
        {
            dictionary = new System.Collections.Generic.Dictionary<string, string>();

            AddedTranslations.Add(table, dictionary);
        }

        dictionary[key] = localized;
        StringTableEntry stringTableEntry = table2.AddEntry(key, localized);
        return new LocalizedString(table2.SharedData.TableCollectionName, stringTableEntry.SharedEntry.Id);
        }
    /// <summary>
    /// Adds an entry made from one of Starlight's own translations
    /// </summary>
    internal static LocalizedString AddStarlightEntry(string localized, string key, string table)
    {
        var localizedString = AddTranslation(localized, key, table, out var usedKey);
        if (usedKey != null) StarlightEntries.Add((table, usedKey));

        return localizedString;
    }

    internal static LocalizedString AddTranslationFromStarlight(string starlightTranslationID, string key = null, string table = "Actor")
    {
        var localizedString = AddStarlightEntry(Translation(starlightTranslationID), key, table);

        StarlightToSRLanguage.TryAdd(starlightTranslationID,localizedString);
        StarlightReplaceOnLanguageChange.TryAdd(starlightTranslationID, (key, table, localizedString));
            
        return localizedString;
    }
        
    /// <summary>
    /// Forgets the entries made from Starlight's own translations
    /// </summary>
    internal static void ForgetStarlightEntries()
    {
        foreach (var (table, key) in StarlightEntries)
            if (AddedTranslations.TryGetValue(table, out var entries))
                entries.Remove(key);

        StarlightEntries.Clear();
    }

    /// <summary>
    /// Changes the text of an added entry
    /// </summary>
    internal static void UpdateTranslation(string localized, string key, string table = "Actor")
    {
        if (!InjectTranslations.HasFlag() || string.IsNullOrWhiteSpace(key)) return;

        if (AddedTranslations.TryGetValue(table, out var entries)) entries[key] = localized;

        var table2 = LocalizationUtil.GetTable(table);
        StringTableEntry entry = null;
        try { entry = table2.GetEntry(key); } catch { }

        if (entry != null) entry.Value = localized;
        else table2.AddEntry(key, localized);
    }

    internal static void SetTranslation(string localized, string key, string table)
    {
        if (!InjectTranslations.HasFlag()) return;
            
        StringTable table2 = LocalizationUtil.GetTable(table);
            
        table2.GetEntry(key).Value = localized;
    }
    internal static void SetTranslationFromStarlight(string starlightTranslationID, string key, string table) => SetTranslation(Translation(starlightTranslationID), key, table);
}