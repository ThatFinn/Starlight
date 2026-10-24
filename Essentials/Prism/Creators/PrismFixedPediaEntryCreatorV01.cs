using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Data.Pedia;
using Starlight.Prism.Lib;
using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

public class PrismFixedPediaEntryCreatorV01
{
    PrismFixedPediaEntry _createdPediaEntry;
    public string Name;
    public Sprite Icon;
    public PrismPediaCategoryType CategoryType;
    public PrismPediaFactSetType FactSet;
    public PrismPediaAdditionalFact[] AdditionalFacts;
    public LocalizedString DescriptionLocalized;
    public LocalizedString TitleLocalized;
    public PrismPediaDetail[] Details;

    public string CustomPersistenceSuffix = null;
    public PrismFixedPediaEntryCreatorV01(string name, PrismPediaCategoryType categoryType, LocalizedString titleLocalized, LocalizedString descriptionLocalized)
    {
        this.Name = name;
        this.CategoryType = categoryType;
        this.DescriptionLocalized = descriptionLocalized;
        this.TitleLocalized = titleLocalized;
    }
    
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        for (int i = 0; i < Name.Length; i++)
            if (!((Name[i] >= 'A' && Name[i] <= 'Z') || (Name[i] >= 'a' && Name[i] <= 'z')))
                return false;
        if (DescriptionLocalized==null) return false;
        if (TitleLocalized==null) return false;
        if (CustomPersistenceSuffix!=null)
            for (int i = 0; i < CustomPersistenceSuffix.Length; i++)
                if (!((CustomPersistenceSuffix[i] >= 'A' && CustomPersistenceSuffix[i] <= 'Z') || (CustomPersistenceSuffix[i] >= 'a' && CustomPersistenceSuffix[i] <= 'z')))
                    return false;
        return true;
    }

    
    public PrismFixedPediaEntry CreateFixedPediaEntry()
    {
        if (!IsValid()) return null;
        if (_createdPediaEntry != null) return _createdPediaEntry;
        
        var entry = Object.Instantiate(PrismLibPedia.FixedPediaEntryPrefab);
        entry.hideFlags = HideFlags.DontUnloadUnusedAsset;
        entry._title = TitleLocalized;
        entry._icon = Icon ?? PrismShortcuts.UnavailableIcon;
        entry._description = DescriptionLocalized;
        entry.name = Name;
        entry._highlightSet = FactSet.GetPediaHighlightSet();
        if (CustomPersistenceSuffix == null)
            entry._persistenceSuffix = entry.name.ToLower();
        else entry._persistenceSuffix = CustomPersistenceSuffix;
        var _details = new List<PediaEntryDetail>();
        if(Details!=null)
            foreach (var detail in Details)
                _details.Add(detail.ConvertToNativeType());
        entry._details = _details.ToArray();
        PrismLibPedia.AddEntry(CategoryType, entry);
        
        var prismEntry = new PrismFixedPediaEntry(entry, false);

        _createdPediaEntry = prismEntry;
        PrismShortcuts.PrismFixedPediaEntries.Add(entry,prismEntry);
        
        if(AdditionalFacts!=null)
            foreach (var additionalFact in AdditionalFacts)
                prismEntry.AddAdditionalFact(additionalFact);
        
        if (PrismLibPedia.PediaCategories.ContainsKey(CategoryType))
            PrismLibPedia.PediaCategories[CategoryType].GetTypedRuntimeCategory();
        return prismEntry;
    }
}