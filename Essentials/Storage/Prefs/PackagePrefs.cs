using System;
using System.IO;
using Newtonsoft.Json;

namespace Starlight.Storage.Prefs;

public class PackagePrefs
{
    public static PackagePrefs[] allPrefs =>_allPrefs.ToArray();
    internal static readonly List<PackagePrefs> _allPrefs = new();
    public delegate void OnPrefChangedEvent(string key, dynamic oldValue, dynamic newValue);
    public static event OnPrefChangedEvent OnPrefChanged;
    
    
    public string expansionID { get; }
    internal List<PackagePref> _entries = new();
    public PackagePref[] entries => _entries.ToArray();
    internal string path;
    
    internal PackagePrefs(string id, string filePath)
    {
        expansionID = id;
        path = filePath;
        _allPrefs.Add(this);
    }

    internal bool IsHidden()
    {
        if (_entries.Count == 0) return true;
        foreach (var pref in _entries)
            if (!pref.isHidden)
                return false;
        return true;
    }
    
    public void Save()
    {
        try
        {
            var dataToSave = new Dictionary<string, object>();
            foreach (var pref in _entries)
                dataToSave[pref.key] = pref.dynamicValue;
            
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string json = JsonConvert.SerializeObject(dataToSave, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        catch (Exception e) { LogError(e); }
    }
    private Dictionary<string, string> LoadRawData()
    {
        if (!File.Exists(path)) return new Dictionary<string, string>();
        try
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch (Exception e)
        {
            LogError(e);
            return new Dictionary<string, string>();
        }
    }
    public PackagePref<T> AddEntry<T>(string key, T defaultValue, string name, string description=null, bool isHidden=false, bool showWarningOnEdit=true, System.Action<T,T> onChange=null)
    {
        var type = typeof(T);
        if (!type.IsPrimitive && type != typeof(string) && !type.IsDefined(typeof(SerializableAttribute), true))
        {
            throw new InvalidOperationException($"The type '{type.Name}' must be have the [System.Serializable] attribute or must be a primitive to be used in PackagePrefs.");
        }
        if (HasEntry(key))
        {
            return GetEntry<T>(key);
        }
        var pref = new PackagePref<T>(this)
        {
            key = key,
            displayName = name,
            description = description,
            isHidden = isHidden,
            showWarningOnEdit = showWarningOnEdit,
            onChange = onChange,
            value = defaultValue,
            dynamicValue = defaultValue,
            defaultValue = defaultValue,
            defaultDynamicValue = defaultValue,
            stringifiedValue = defaultValue?.ToString()
        };
        var savedData = LoadRawData();
        if (savedData.TryGetValue(key, out var value))
        {
            try
            {
                var loadedValue = JsonConvert.DeserializeObject<T>(value);
                pref.value = loadedValue;
                pref.dynamicValue = loadedValue;
                pref.stringifiedValue = loadedValue?.ToString();
            }
            catch (Exception e)
            {
                LogError(new Exception($"Failed to load preference '{key}'. Falling back to default. Details: {e.Message}"));
            }
        }
        _entries.Add(pref);
        Save();
        return pref;
    }

    public bool HasEntry(string key)
    {
        foreach (var pref in _entries)
            if (pref.key == key)
                return true;
        return false;
    }
    public PackagePref<T> GetEntry<T>(string key)
    {
        foreach (var pref in _entries)
            if (pref.key == key)
                return (PackagePref<T>) pref;
        return null;
    }
    public PackagePref GetEntry(string key)
    {
        foreach (var pref in _entries)
            if (pref.key == key)
                return pref;
        return null;
    }
    public bool DeleteEntry(string key)
    {
        foreach (var pref in _entries.ToArray())
            if (pref.key == key)
            {
                _entries.Remove(pref);
                Save();
                return true;
            }
        return false;
    }

    public bool SetEntry<T>(string key, T newValue)
    {
        var pref = GetEntry<T>(key);
        if (pref == null) return false;

        dynamic oldValue = pref.value;
        pref.value=newValue;
        pref.dynamicValue = newValue;
        pref.stringifiedValue = newValue.ToString();
        Save();
        try
        {
            pref.onChange?.Invoke(oldValue, newValue);
        }
        catch (Exception e) { LogError(e); }
        try
        {
            OnPrefChanged?.Invoke(key, oldValue, newValue);
        }
        catch (Exception e) { LogError(e); }
        return true;
    }
}