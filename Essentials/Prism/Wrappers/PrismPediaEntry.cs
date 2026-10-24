using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Data.Pedia;
using Starlight.Prism.Lib;
using UnityEngine.Localization;

namespace Starlight.Prism.Wrappers;

public class PrismPediaEntry
{
    public static implicit operator PediaEntry(PrismPediaEntry prismPediaEntry)
    {
        return prismPediaEntry.GetPediaEntry();
    }
    
    internal PrismPediaEntry(PediaEntry pediaEntry, bool isNative)
    {
        this.PediaEntry = pediaEntry;
        this.IsNative = isNative;
    }
    internal PediaEntry PediaEntry;
    protected bool IsNative;
    
    public PediaEntry GetPediaEntry() => PediaEntry;
    public string GetPersistenceID() => PediaEntry.PersistenceId;
    public bool GetIsUnlockedInitially() => PediaEntry._isUnlockedInitially;
    public string GetName() => PediaEntry.name;
    public Sprite GetIcon() => PediaEntry.Icon;
    public LocalizedString GetTitle() => PediaEntry.Title;
    public LocalizedString GetDescription() => PediaEntry.Description;
    public int GetDetailCount() => PediaEntry._details.Count;
    public bool GetIsNative() => IsNative;

    public void AddDetail(PrismPediaDetail? detail)
    {
        if(detail!=null)
            PediaEntry._details = PediaEntry._details.AddToNew(detail.ConvertToNativeType());
    }
    public void RemoveAtDetail(int index)
    {
        if (index < 0) return;
        if (index >= PediaEntry._details.Count) return;
        PediaEntry._details = PediaEntry._details.RemoveAtToNew(index);
    }    
    public void ReplaceDetail(int index,PrismPediaDetail? detail)
    {
        if (index < 0) return;
        if (index >= PediaEntry._details.Count) return;
        if (detail == null) return;
        PediaEntry._details = PediaEntry._details.ReplaceToNew(detail.ConvertToNativeType(),index);
    }    
    public void InsertDetail(int index,PrismPediaDetail? detail)
    {
        if (index < 0) return;
        if (index >= PediaEntry._details.Count) return;
        if (detail == null) return;
        PediaEntry._details = PediaEntry._details.InsertToNew(detail.ConvertToNativeType(),index);
    }

    public void ClearDetails()
    {
        PediaEntry._details = new Il2CppReferenceArray<PediaEntryDetail>(0);
    }
    
    public void AddAdditionalFact(PrismPediaAdditionalFact? fact)
    {
        if (fact == null) return;
        if (!PrismLibPedia.AdditionalFactsMap.ContainsKey(PediaEntry))
            PrismLibPedia.AdditionalFactsMap[PediaEntry] = new List<PrismPediaAdditionalFact>();
        PrismLibPedia.AdditionalFactsMap[PediaEntry].Add(fact.Value);
    }
    public void RemoveAdditionalFact(PrismPediaAdditionalFact? fact)
    {
        if (fact == null) return;
        if (!PrismLibPedia.AdditionalFactsMap.ContainsKey(PediaEntry)) return;
        if (PrismLibPedia.AdditionalFactsMap[PediaEntry].Contains(fact.Value))
            PrismLibPedia.AdditionalFactsMap[PediaEntry].Remove(fact.Value);
    }
    public void RemoveAtAdditionalFact(int index)
    {
        if (index < 0) return;
        if (!PrismLibPedia.AdditionalFactsMap.ContainsKey(PediaEntry)) return;
        if (PrismLibPedia.AdditionalFactsMap[PediaEntry].Count>index)
            PrismLibPedia.AdditionalFactsMap[PediaEntry].RemoveAt(index);
    }

    public void ClearAdditionalFacts()
    {
        if (PrismLibPedia.AdditionalFactsMap.ContainsKey(PediaEntry)) return;
            PrismLibPedia.AdditionalFactsMap.Remove(PediaEntry);
    }
}