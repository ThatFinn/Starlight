using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Data.Pedia;
using Starlight.Prism.Lib;
using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

[System.Obsolete("PrismRadiantSlimePediaEntryCreatorV01 does not work after version 1.3.0, use the V02 creators instead")]
public class PrismRadiantSlimePediaEntryCreatorV01
{
    PrismRadiantSlimePediaEntry _createdPediaEntry;
    public SlimeDefinition slimeDefinition;
    public PrismPediaCategoryType CategoryType;
    public PrismPediaAdditionalFact[] AdditionalFacts;
    public PrismPediaFactSetType FactSet;
    public LocalizedString DescriptionLocalized;
    public PrismPediaDetail[] Details;
    
    public PrismRadiantSlimePediaEntryCreatorV01(SlimeDefinition slimeDefinition, PrismPediaCategoryType categoryType, LocalizedString descriptionLocalized)
    {
        this.slimeDefinition = slimeDefinition;
        this.CategoryType = categoryType;
        this.DescriptionLocalized = descriptionLocalized;
    }
    
    public bool IsValid()
    {
        if (slimeDefinition==null) return false;
        if (slimeDefinition.IsLargo) return false;
        if (DescriptionLocalized==null) return false;
        return true;
    }

    public PrismRadiantSlimePediaEntry CreateRadiantSlimePediaEntry()
    {
        if (!IsValid()) return null;
        if (!SupportRadiant.HasFlag()) return null;
        if (_createdPediaEntry != null) return _createdPediaEntry;
        
        var entry = Object.Instantiate(PrismLibPedia.RadiantSlimePediaEntryPrefab);
        entry.hideFlags = HideFlags.DontUnloadUnusedAsset;

        entry._title = slimeDefinition.localizedName;
        entry._slimeDefinition = slimeDefinition;
        entry._description = DescriptionLocalized;
        entry.name = "Radiant"+slimeDefinition.name;
        entry._highlightSet = FactSet.GetPediaHighlightSet();
        
        var details = new List<PediaEntryDetail>();
        if(Details!=null)
            foreach (var detail in Details)
                details.Add(detail.ConvertToNativeType());
        entry._details = details.ToArray();
        PrismLibPedia.AddEntry(CategoryType, entry);
        
        var prismEntry = new PrismRadiantSlimePediaEntry(entry, false);

        _createdPediaEntry = prismEntry;
        PrismShortcuts.PrismRadiantSlimePediaEntries.Add(entry,prismEntry);
        
        if(AdditionalFacts!=null)
            foreach (var additionalFact in AdditionalFacts)
                prismEntry.AddAdditionalFact(additionalFact);
        
        if (PrismLibPedia.PediaCategories.ContainsKey(CategoryType))
            PrismLibPedia.PediaCategories[CategoryType].GetTypedRuntimeCategory();
        return prismEntry;
    }
}