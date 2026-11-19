using UnityEngine.Localization;

namespace Starlight.Prism.Wrappers;

public class PrismIdentifiableTypeGroup
{
    public static implicit operator IdentifiableTypeGroup(PrismIdentifiableTypeGroup group)
    {
        return group.GetIdentifiableTypeGroup();
    }
    
    public static implicit operator PrismIdentifiableTypeGroup(IdentifiableTypeGroup group)
    {
        return group.GetPrismIdentifiableGroup();
    }

    internal PrismIdentifiableTypeGroup(IdentifiableTypeGroup group, bool isNative)
    {
        this.Group = group;
        this._isNative = isNative;
    }

    internal readonly IdentifiableTypeGroup Group;
    private readonly bool _isNative;
    
    public IdentifiableTypeGroup GetIdentifiableTypeGroup() => Group;
    public string GetReferenceID() => Group.ReferenceId;
    public string GetName() => Group.name;
    public bool GetIsFood() => Group._isFood;
    public LocalizedString GetLocalized() => Group.LocalizedName;
    public bool GetIsNative() => _isNative;
    
    public void SetIsFood(bool isFood)
    {
        Group._isFood=isFood;
    }
}