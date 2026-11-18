using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

[System.Obsolete("PrismIdentifiableTypeGroupCreatorV01 does not work after version 1.3.0, use the V02 creators instead")]
public class PrismIdentifiableTypeGroupCreatorV01
{
    
    PrismIdentifiableTypeGroup _createdGroup;
    public string Name;
    public LocalizedString Localized;
    public List<IdentifiableType> MemberTypes;
    public List<IdentifiableTypeGroup> MemberGroupes;
    public bool IsFood = false;
    public Sprite Icon;
    public PrismIdentifiableTypeGroupCreatorV01(string name, LocalizedString localized)
    {
        this.Name = name;
        this.Localized = localized;
    }
    
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        for (int i = 0; i < Name.Length; i++)
            if (!((Name[i] >= 'A' && Name[i] <= 'Z') || (Name[i] >= 'a' && Name[i] <= 'z')))
                return false;
        return true;
    }

    
    public PrismIdentifiableTypeGroup CreateIdentifiableTypeGroup()
    {
        if (!IsValid()) return null;
        if (_createdGroup != null) return _createdGroup;

        var group = ScriptableObject.CreateInstance<IdentifiableTypeGroup>();
        group.hideFlags = HideFlags.DontUnloadUnusedAsset;

        group._memberTypes = new Il2CppSystem.Collections.Generic.List<IdentifiableType>();
        if(MemberTypes!=null)
            foreach (var type in MemberTypes)
                group._memberTypes.Add(type);

        group._memberGroups = new Il2CppSystem.Collections.Generic.List<IdentifiableTypeGroup>();
        if(MemberGroupes!=null)
            foreach (var subGroup in MemberGroupes)
                group._memberGroups.Add(subGroup);

        group._isFood = IsFood;
        
        if (Localized != null) group._localizedName = Localized;
        else group._localizedName = PrismShortcuts.EmptyTranslation;
        group.name = Name;

        group._icon = Icon ?? PrismShortcuts.UnavailableIcon;
        group.AllowedCategories = new Il2CppSystem.Collections.Generic.List<IdentifiableCategory>();

        group._runtimeObject = new IdentifiableTypeGroupRuntimeObject(group);

            
        if(!LookupEUtil.IdentifiableTypeGroupList.items.Contains(group))
            LookupEUtil.IdentifiableTypeGroupList.items.Add(group);
        gameContext.LookupDirector.RegisterIdentifiableTypeGroup(group);
        if(!gameContext.LookupDirector._allIdentifiableTypeGroups.items.Contains(group))
            gameContext.LookupDirector._allIdentifiableTypeGroups.items.Add(group);

        var prismGroup = new PrismIdentifiableTypeGroup(group, false);
            
        PrismShortcuts.PrismIdentifiableTypeGroups.Add(group,prismGroup);

        _createdGroup = prismGroup;
        return _createdGroup;
    }
}