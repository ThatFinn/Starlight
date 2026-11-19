using Starlight.Prism.Colliders;
using Starlight.Prism.Lib;
using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

//WIP
[System.Obsolete("PrismToyCreatorV01 does not work after version 1.3.0, use the V02 creators instead")]
internal class PrismToyCreatorV01
{
    private PrismToy _createdToy;

    public string Name;
    public GameObject VisualModelPrefab;
    public ColliderData ColliderData;
    public Sprite Icon;
    public LocalizedString Localized;
    private string referenceID => "ToyDefinition.Modded" + Name + "Toy";

    public Color VacColor = new Color32(0,0,0,255);
    public GameObject CustomBasePrefab = null;

    public float BaseAgitationReductionFactor = 0.15f;
    public float FavoriteAgitationReductionFactor = 0.3f;
    
    public PrismToyCreatorV01(string name, GameObject visualModelPrefab, ColliderData colliderData, Sprite icon, LocalizedString localized)
    {
        this.Name = name;
        this.VisualModelPrefab = visualModelPrefab;
        this.ColliderData = colliderData;
        this.Icon = icon;
        this.Localized = localized;
    }
    
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        for (int i = 0; i < Name.Length; i++)
            if (!((Name[i] >= 'A' && Name[i] <= 'Z') || (Name[i] >= 'a' && Name[i] <= 'z')))
                return false;
        if (Localized==null) return false;
        if (CustomBasePrefab != null)
        {
            if (!CustomBasePrefab.HasComponent<IdentifiableActor>()) return false;
        }
        return true;
    }

    public PrismToy CreateToy()
    {
        if (!IsValid()) return null;
        if (_createdToy != null) return _createdToy;
        var toy = ScriptableObject.CreateInstance<ToyDefinition>();
        toy.hideFlags = HideFlags.DontUnloadUnusedAsset;
        toy.name = Name + "Toy";
        toy.color = VacColor;
        toy.icon = Icon ?? PrismShortcuts.UnavailableIcon;
        try
        {
            dynamic dynamicIdent = toy;
            dynamicIdent._requiresFullArt = false;
            dynamicIdent._fullArt = null;
        }
        catch { }
        
        toy.BaseAgitationReductionFactor = BaseAgitationReductionFactor;
        toy.FavoriteAgitationReductionFactor = FavoriteAgitationReductionFactor;
        
        toy.localizedName = Localized;
        toy._pediaPersistenceSuffix = "modded"+Name.ToLower()+"_toy";
        
        toy.Prism_AddToGroup("ToyGroup");
        
        var basePrefab = CustomBasePrefab;
        if (basePrefab == null) basePrefab = LookupEUtil.toyTypes.GetEntryByRefID("ToyDefinition.BeachBall").prefab; //PrismNativeToy.BeachBall.GetPrismToy().GetPrefab();
        toy.prefab = CreatePrefab(Name, basePrefab);
        toy.prefab.GetComponent<IdentifiableActor>().identType = toy;
        while (toy.prefab.HasComponent<Collider>())
            toy.prefab.RemoveComponent<Collider>();
        
        if (CustomBasePrefab == null)
            Object.DestroyImmediate(toy.prefab.GetObjectRecursively<GameObject>("beachBall_ld"));
        var modelHolder = new GameObject("StarlightModelHolder");
        modelHolder.transform.SetParent(toy.prefab.transform);
        modelHolder.transform.position = Vector3.zero;
        modelHolder.transform.rotation = Quaternion.identity;
        modelHolder.transform.localScale = Vector3.zero;

        var modelInstance = Object.Instantiate(VisualModelPrefab);
        modelHolder.transform.SetParent(modelInstance.transform);
        modelHolder.transform.position = Vector3.zero;
        
        ColliderData.AddToGameObject(toy.prefab);
        
        // I highly doubt anyone will make a toy snare-able
        //toy.gordoSnareBaitPrefab=CreatePrefab(Name+"SnareBait", VisualModelPrefab);
        //toy.gordoSnareBaitPrefab.transform.position = Vector3.zero;
        
        PrismLibSaving.SetupForSaving(toy,referenceID);
        
        var prismToy = new PrismToy(toy, false);
        
        _createdToy = prismToy;
        PrismShortcuts.PrismToys.Add(toy.ReferenceId,prismToy);
        return _createdToy;
    }
}