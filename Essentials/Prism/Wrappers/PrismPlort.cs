using Starlight.Prism.Data.Native;
using UnityEngine.Localization;

namespace Starlight.Prism.Wrappers;

public class PrismPlort
{
    public static implicit operator IdentifiableType(PrismPlort prismPlort)
    {
        return prismPlort.GetIdentifiableType();
    }
    
    public static implicit operator PrismPlort(PrismNativePlort nativePlort)
    {
        return nativePlort.GetPrismPlort();
    }

    internal readonly IdentifiableType IdentifiableType;
    private readonly bool _isNative;
    public IdentifiableType GetIdentifiableType() => IdentifiableType;
    public string GetReferenceID() => IdentifiableType.ReferenceId;
    public string GetName() => IdentifiableType.name;
    public Sprite GetIcon() => IdentifiableType.icon;
    public LocalizedString GetLocalized() => IdentifiableType.LocalizedName;
    public Color32 GetVacColor() => IdentifiableType.color;
    public GameObject GetPrefab() => IdentifiableType.prefab;
    public bool GetIsNative() => _isNative;
    
    
    public void SetIcon(Sprite newIcon)
    {
        IdentifiableType.icon = newIcon;
    }
    public void SetVacColor(Color32 newColor)
    {
        IdentifiableType.color = newColor;
    }
    
    internal PrismPlort(IdentifiableType identifiableType, bool isNative)
    {
        this.IdentifiableType = identifiableType;
        this._isNative = isNative;
    }
}