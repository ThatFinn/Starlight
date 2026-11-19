namespace Starlight.Storage.Prefs;

public abstract class PackagePref
{
    public PackagePrefs prefs { get; internal set; }
    public string key { get; internal set; }
    public bool isHidden { get; internal set; } = false;
    public bool showWarningOnEdit { get; internal set; } = true;
    
    public string description { get; internal set; } = null;
    public string displayName { get; internal set; } = null;
    
    public string stringifiedValue { get; internal set; } = null;
    public dynamic dynamicValue { get; internal set; } = null;
    public dynamic defaultDynamicValue { get; internal set; } = null;
    
    internal PackagePref(PackagePrefs prefs)
    {
        this.prefs = prefs;
    }

    public void SetPref<T>(T value) => prefs.SetEntry<T>(key, value);
    
}
public class PackagePref<T> : PackagePref
{
    public T value { get; internal set; } = default(T);
    public T defaultValue { get; internal set; } = default(T);
    public System.Action<T,T> onChange { get; internal set; } = null;
    public PackagePref(PackagePrefs prefs) : base(prefs)
    {
        this.prefs = prefs;
    }
}