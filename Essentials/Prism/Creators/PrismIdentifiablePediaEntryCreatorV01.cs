using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Data.Pedia;
using Starlight.Prism.Lib;
using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

[System.Obsolete("PrismIdentifiablePediaEntryCreatorV01 does not work after version 1.3.0, use the V02 creators instead")]
public class PrismIdentifiablePediaEntryCreatorV01
{
    PrismIdentifiablePediaEntry _createdPediaEntry;
    public IdentifiableType IdentifiableType;
    public PrismPediaCategoryType CategoryType;
    public PrismPediaAdditionalFact[] AdditionalFacts;
    public PrismPediaFactSetType FactSet;
    public LocalizedString DescriptionLocalized;
    public PrismPediaDetail[] Details;
    
    public PrismIdentifiablePediaEntryCreatorV01(IdentifiableType identifiableType, PrismPediaCategoryType categoryType, LocalizedString descriptionLocalized)
    {
        this.IdentifiableType = identifiableType;
        this.CategoryType = categoryType;
        this.DescriptionLocalized = descriptionLocalized;
    }
    
    public bool IsValid()
    {
        if (IdentifiableType==null) return false;
        if (DescriptionLocalized==null) return false;
        return true;
    }

    
    public PrismIdentifiablePediaEntry CreateIdentifiablePediaEntry()
    {
        if (!IsValid()) return null;
        if (_createdPediaEntry != null) return _createdPediaEntry;
        
        var entry = Object.Instantiate(PrismLibPedia.IdentifiablePediaEntryPrefab);
        entry.hideFlags = HideFlags.DontUnloadUnusedAsset;

        entry._title = IdentifiableType.localizedName;
        entry._identifiableType = IdentifiableType;
        entry._description = DescriptionLocalized;
        entry.name = IdentifiableType.name;
        entry._highlightSet = FactSet.GetPediaHighlightSet();
        var details = new List<PediaEntryDetail>();
        if(Details!=null)
            foreach (var detail in Details)
                details.Add(detail.ConvertToNativeType());
        entry._details = details.ToArray();
        PrismLibPedia.AddEntry(CategoryType, entry);
        
        var prismEntry = new PrismIdentifiablePediaEntry(entry, false);

        _createdPediaEntry = prismEntry;
        PrismShortcuts.PrismIdentifiablePediaEntries.Add(entry,prismEntry);
        
        if(AdditionalFacts!=null)
            foreach (var additionalFact in AdditionalFacts)
                prismEntry.AddAdditionalFact(additionalFact);
        
        if (PrismLibPedia.PediaCategories.ContainsKey(CategoryType))
            PrismLibPedia.PediaCategories[CategoryType].GetTypedRuntimeCategory();
        return prismEntry;
    }
}