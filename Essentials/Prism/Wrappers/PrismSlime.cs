using UnityEngine.Localization;

namespace Starlight.Prism.Wrappers;

public class PrismSlime
{
    public static implicit operator SlimeDefinition(PrismSlime prismSlime)
    {
        return prismSlime.GetSlimeDefinition();
    }
    
    internal PrismSlime(SlimeDefinition slimeDefinition, bool isNative)
    {
        this.SlimeDefinition = slimeDefinition;
        this.IsNative = isNative;
    }
    internal SlimeDefinition SlimeDefinition;
    protected bool IsNative;
    
    public SlimeDefinition GetSlimeDefinition() => SlimeDefinition;
    public string GetReferenceID() => SlimeDefinition.ReferenceId;
    public string GetName() => SlimeDefinition.name;
    public LocalizedString GetLocalized() => SlimeDefinition.LocalizedName;
    public Color32 GetColor() => SlimeDefinition.color;
    public GameObject GetPrefab() => SlimeDefinition.prefab;
    public SlimeAppearance GetSlimeAppearance() => SlimeDefinition.AppearancesDefault[0];
    
    public SlimeAppearance GetSlimeAppearanceRadiant() => SlimeDefinition.GetRadiantAppearance();

    public SlimeAppearance TryGetSlimeAppearanceRadiant()
    {
        try
        {
            var app = SlimeDefinition.GetRadiantAppearance();
            if (app != null)
                return app;
        }
        catch { }
        return GetSlimeAppearance();
    }
    public SlimeDiet GetSlimeDiet() => SlimeDefinition.Diet;
    public bool GetIsNative() => IsNative;
    
}