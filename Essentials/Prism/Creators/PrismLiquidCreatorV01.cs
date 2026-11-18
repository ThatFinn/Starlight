using Il2CppMonomiPark.SlimeRancher;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Lib;
using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

[System.Obsolete("PrismLiquidCreatorV01 does not work after version 1.3.0, use the V02 creators instead")]
public class PrismLiquidCreatorV01
{
    private PrismLiquid _createdLiquid;

    public string Name;
    public Sprite Icon;
    public LocalizedString Localized;
    private string referenceID => "LiquidDefinition.Modded" + Name + "Liquid";

    public Color VacColor = new Color(0.2f,0.5f,0.6f,255);
    public GameObject CustomBasePrefab = null;


    public GameObject CustomVacFailFxPrefab = null;
    public GameObject CustomVacInFxPrefab = null;

    public bool IsWater = true;
    
    public PrismLiquidCreatorV01(string name, Sprite icon, LocalizedString localized)
    {
        this.Name = name;
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

    public PrismLiquid CreateLiquid()
    {
        if (!IsValid()) return null;
        if (_createdLiquid != null) return _createdLiquid;
        var liquid = ScriptableObject.CreateInstance<LiquidDefinition>();
        liquid.hideFlags = HideFlags.DontUnloadUnusedAsset;
        liquid.name = Name + "Liquid";
        liquid.color = VacColor;
        liquid.icon = Icon ?? PrismShortcuts.UnavailableIcon;
        liquid._isWater = IsWater;
        try
        {
            dynamic dynamicIdent = liquid;
            dynamicIdent._requiresFullArt = false;
            dynamicIdent._fullArt = null;
        }
        catch { }

        try
        {
            liquid._vacFailFX = CustomVacFailFxPrefab;
            liquid._inFX = CustomVacInFxPrefab;
            var water = LookupEUtil.liquidTypes.GetEntryByRefID("LiquidDefinition.Water");
            liquid._vacFailFX ??= water._vacFailFX;
            liquid._inFX ??= water._inFX;
            
        }
        catch (Exception e)
        {
            LogError(e);
            LogError("An error occured when fetching the FX from water!");
        }
        liquid.localizedName = Localized;
        liquid._pediaPersistenceSuffix = "modded"+Name.ToLower()+"_liquid";
        
        liquid.Prism_AddToGroup("LiquidGroup");
        if(IsWater)
            liquid.Prism_AddToGroup("WaterGroup");
        
        var basePrefab = CustomBasePrefab;
        if (basePrefab == null) basePrefab = PrismNativeLiquid.Water.GetPrismLiquid().GetPrefab();
        liquid.prefab = CreatePrefab("liquid"+Name, basePrefab);
        liquid.prefab.GetComponent<IdentifiableActor>().identType = liquid;
        try
        {
            liquid.prefab.GetComponent<MeshRenderer>().material =
                Object.Instantiate(liquid.prefab.GetComponent<MeshRenderer>().material);
        } catch { }

        liquid.prefab.RemoveComponent<WaterIdentifiableVFXSwap>();
        if (CustomBasePrefab == null)
        {
            liquid.prefab.GetObjectRecursively<GameObject>("Sphere_Regular").name="Sphere";
            Object.DestroyImmediate(liquid.prefab.GetObjectRecursively<GameObject>("Sphere_Prisma"));
        }
        foreach (var child in liquid.prefab.GetAllChildren())
            if(child.HasComponent<MeshRenderer>())
                try { child.GetComponent<MeshRenderer>().material = Object.Instantiate(child.GetComponent<MeshRenderer>().material); } catch { }
        PrismLibSaving.SetupForSaving(liquid,referenceID);
        
        var prismLiquid = new PrismLiquid(liquid, false);
        
        _createdLiquid = prismLiquid;
        PrismShortcuts.PrismLiquids.Add(liquid.ReferenceId,prismLiquid);
        
        _createdLiquid.SetColors(VacColor*0.3882f,new Color(VacColor.r,VacColor.g,VacColor.b,0),VacColor,VacColor);
        return _createdLiquid;
    }
}