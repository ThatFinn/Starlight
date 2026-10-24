using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Starlight.Enums.Features;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Lib;
using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

public class PrismBaseSlimeCreatorV01
{
    private PrismBaseSlime _createdSlime;
    
    
    public string Name;
    public Sprite Icon;
    public Sprite RadiantIcon;
    public LocalizedString Localized;
    public string referenceID => "SlimeDefinition.Modded" + Name;

    public List<PrismToy> FavouriteToys = new();
    public PrismPlort Plort = null;
    public GameObject CustomBasePrefab = null;
    public SlimeAppearance CustomBaseAppearance = null;
    public SlimeAppearance CustomRadiantAppearance = null;
    public bool DisableSinkInShallowWater = false;
    public bool DisableEdibleByTarrs = false;
    public bool DisableVaccable = false;

    public PrismLargoMergeSettings CustomAutoLargoMergeSettings = null;
    public bool CanLargofy = false;
    public bool CreateAllLargos = false;
    public bool DisableAutoModdedLargos = false;
    public Color VacColor = new Color32(0,0,0,255);
    public bool SupportRadiant = true;
    public int RadiantBagSize = 1500;
    
    
    public PrismBaseSlimeCreatorV01(string name, Sprite icon, LocalizedString localized)
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
            if (!CustomBasePrefab.HasComponent<SlimeAppearanceApplicator>()) return false;
            if (!CustomBasePrefab.HasComponent<IdentifiableActor>()) return false;
        }
        return true;
    }




    private void Duplicate(SlimeAppearance appearance, SlimeAppearance baseAppearance)
    {
        var newSlimeAppearanceStructure = new List<SlimeAppearanceStructure>();
        foreach (var slimeAppearanceStructure in baseAppearance.Structures)
        {
            var newStructure = new SlimeAppearanceStructure(slimeAppearanceStructure);
            
            var newDefaultMaterials = new List<Material>();
            foreach (var mat in slimeAppearanceStructure.DefaultMaterials)
                newDefaultMaterials.Add(Object.Instantiate(mat));
            newStructure.DefaultMaterials = newDefaultMaterials.ToArray();

            if(slimeAppearanceStructure.Element!=null)
            {
                newStructure.Element = Object.Instantiate(slimeAppearanceStructure.Element);
                var newElementPrefabs = new List<SlimeAppearanceObject>();
                foreach (var prefab in newStructure.Element.Prefabs)
                {
                    var prefabCopy = CreatePrefab("copy_" + prefab.name, prefab.gameObject);
                    newElementPrefabs.Add(prefabCopy.GetComponent<SlimeAppearanceObject>());
                }

                newStructure.Element.Prefabs = newElementPrefabs.ToArray();
            }

            if (slimeAppearanceStructure.ElementMaterials != null)
            {
                var newElementMaterials = new List<SlimeAppearanceMaterials>();
                foreach (var elementMaterial in slimeAppearanceStructure.ElementMaterials)
                {
                    var newElementMaterial = new SlimeAppearanceMaterials();
                    newElementMaterial.OverrideDefaults = elementMaterial.OverrideDefaults;
                
                    var newElementMaterialsMaterials = new List<Material>();
                    foreach (var mat in elementMaterial.Materials)
                        newElementMaterialsMaterials.Add(Object.Instantiate(mat));
                    newElementMaterial.Materials = newElementMaterialsMaterials.ToArray();
                    newElementMaterials.Add(newElementMaterial);
                }
                newStructure.ElementMaterials = newElementMaterials.ToArray();
            }
            newSlimeAppearanceStructure.Add(newStructure);
        }
        appearance.Structures = newSlimeAppearanceStructure.ToArray();

        if (baseAppearance._face != null) appearance._face = Object.Instantiate(baseAppearance._face);
        if(baseAppearance._idleAnimationOverride!=null) appearance._idleAnimationOverride = Object.Instantiate(baseAppearance._idleAnimationOverride);
        if (baseAppearance._crystalAppearance != null) appearance._crystalAppearance = Object.Instantiate(baseAppearance._crystalAppearance);
        if (baseAppearance._deathAppearance != null) appearance._deathAppearance = Object.Instantiate(baseAppearance._deathAppearance);
        if (baseAppearance._explosionAppearance != null) appearance._explosionAppearance = Object.Instantiate(baseAppearance._explosionAppearance);
        if (baseAppearance._glintAppearance != null) appearance._glintAppearance = Object.Instantiate(baseAppearance._glintAppearance);
        if (baseAppearance._qubitAppearance != null) appearance._qubitAppearance = Object.Instantiate(baseAppearance._qubitAppearance);
        if (baseAppearance._roarAppearance != null) appearance._roarAppearance = Object.Instantiate(baseAppearance._roarAppearance);
        if (baseAppearance._shockedAppearance != null) appearance._shockedAppearance = Object.Instantiate(baseAppearance._shockedAppearance);
        if (baseAppearance._stoneFormAppearance != null) appearance._stoneFormAppearance = Object.Instantiate(baseAppearance._stoneFormAppearance);
        if (baseAppearance._tornadoAppearance != null) appearance._tornadoAppearance = Object.Instantiate(baseAppearance._tornadoAppearance);
        if (baseAppearance._vineAppearance != null) appearance._vineAppearance = Object.Instantiate(baseAppearance._vineAppearance);
        if (baseAppearance._wingFlapAnimationOverride != null) appearance._wingFlapAnimationOverride = Object.Instantiate(baseAppearance._wingFlapAnimationOverride);
        if (baseAppearance._biteAnimationOverride != null) appearance._biteAnimationOverride = Object.Instantiate(baseAppearance._biteAnimationOverride);
    }

    private SlimeAppearance RadiantStuff(SlimeAppearance appearance, SlimeDefinition slimeDef)
    {
        appearance._appearType = SlimeAppearance.AppearanceType.DEFAULT;
        appearance._fullArt = null;
        slimeDef.RadiantBase = null;
        slimeDef.RadiantLargo0 = null;
        slimeDef.RadiantLargo1 = null;
        if (SupportRadiant)
        {
            SlimeAppearance radiantAppearance = null;
            var baseRadiantAppearance = CustomRadiantAppearance;
            if (baseRadiantAppearance == null)
                baseRadiantAppearance = PrismNativeBaseSlime.Pink.GetPrismBaseSlime().TryGetSlimeAppearanceRadiant();
            radiantAppearance = Object.Instantiate(baseRadiantAppearance);
            radiantAppearance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            radiantAppearance.name = Name + "Radiant";
            radiantAppearance._icon = RadiantIcon ?? Icon;
            radiantAppearance._icon ??= PrismShortcuts.UnavailableIcon;
            slimeDef.AppearancesDefault = slimeDef.AppearancesDefault.AddToNew(radiantAppearance);
            if (slimeDef.AppearancesDefault[1] == null)
                slimeDef.AppearancesDefault[1] = radiantAppearance;

            Duplicate(radiantAppearance, baseRadiantAppearance);
            slimeDef.RadiantBase = radiantAppearance;
            radiantAppearance._appearType = SlimeAppearance.AppearanceType.RADIANT_BASE;
            radiantAppearance._fullArt = null;

            PrismShortcuts.mainAppearanceDirector.RegisterDependentAppearances(slimeDef, baseRadiantAppearance);
            PrismShortcuts.mainAppearanceDirector.UpdateChosenSlimeAppearance(slimeDef, baseRadiantAppearance);
            return radiantAppearance;
        }
        return null;
    }

    public PrismBaseSlime CreateSlime()
    {
        if (!IsValid()) return null;
        if (_createdSlime != null) return _createdSlime;
        if (!FeatureFlag.SupportRadiant.HasFlag()) SupportRadiant = false; 
        
        var slimeDef = Object.Instantiate( PrismNativeBaseSlime.Pink.GetPrismBaseSlime().GetSlimeDefinition());
        slimeDef.hideFlags = HideFlags.DontUnloadUnusedAsset;
        slimeDef.Name = Name;
        slimeDef.name = Name;
        slimeDef.AppearancesDefault = new Il2CppReferenceArray<SlimeAppearance>(0);
        try
        {
            dynamic dynamicIdent = slimeDef;
            dynamicIdent._requiresFullArt = false;
            dynamicIdent._fullArt = null;
        }
        catch { }

        var baseAppearance = CustomBaseAppearance;
        if (baseAppearance == null) baseAppearance = PrismNativeBaseSlime.Pink.GetPrismBaseSlime().GetSlimeAppearance();
        var appearance = Object.Instantiate(baseAppearance);
        appearance.hideFlags = HideFlags.DontUnloadUnusedAsset;
        appearance.name = Name+"Default";
        appearance._icon = Icon ?? PrismShortcuts.UnavailableIcon;
        slimeDef.AppearancesDefault = slimeDef.AppearancesDefault.AddToNew(appearance);
        if (slimeDef.AppearancesDefault[0] == null)
            slimeDef.AppearancesDefault[0] = appearance;

        Duplicate(appearance, baseAppearance);
        
        SlimeAppearance radiantAppearance = null;
        if (FeatureFlag.SupportRadiant.HasFlag())
            radiantAppearance = RadiantStuff(appearance,slimeDef);
        
        var basePrefab = CustomBasePrefab;
        if (basePrefab == null) basePrefab = PrismNativeBaseSlime.Pink.GetPrismBaseSlime().GetPrefab();
        slimeDef.prefab = CreatePrefab("slime"+Name, basePrefab);
        if(slimeDef.prefab.HasComponent<SlimeEat>())
            slimeDef.prefab.GetComponent<SlimeEat>().SlimeDefinition = slimeDef; 
        slimeDef.prefab.GetComponent<SlimeAppearanceApplicator>().SlimeDefinition = slimeDef;
        slimeDef.prefab.GetComponent<IdentifiableActor>().identType = slimeDef;
        var slimeDiet = PrismLibDiet.CreateNewDiet();
        slimeDef.Diet = slimeDiet;
        VacColor.a = 255;
        slimeDef.color = VacColor;
        slimeDef.icon = Icon;

        PrismShortcuts.mainAppearanceDirector.RegisterDependentAppearances(slimeDef, appearance);
        PrismShortcuts.mainAppearanceDirector.UpdateChosenSlimeAppearance(slimeDef, appearance);
        PrismLibSaving.SetupForSaving(slimeDef,referenceID);

        slimeDef.FavoriteToyIdents = new Il2CppReferenceArray<ToyDefinition>(FavouriteToys.Count);
        for (int i = 0; i < FavouriteToys.Count; i++)
            slimeDef.FavoriteToyIdents[i] = FavouriteToys[i].ToyDef;
        
        
        
        if(!DisableVaccable) 
            slimeDef.Prism_AddToGroup("VaccableBaseSlimeGroup");
        slimeDef.Prism_AddToGroup("SmallSlimeGroup");
        if(!DisableEdibleByTarrs)
            slimeDef.Prism_AddToGroup("EdibleSlimeGroup");
        slimeDef.Prism_AddToGroup("IdentifiableTypesGroup");
        if(!DisableSinkInShallowWater) slimeDef.Prism_AddToGroup("SlimesSinkInShallowWaterGroup");
        
        
        appearance._colorPalette = new SlimeAppearance.Palette { Ammo = VacColor, Bottom = VacColor, Middle = VacColor, Top = VacColor };
        appearance._splatColor = VacColor;

        if (radiantAppearance != null)
        {
            radiantAppearance._colorPalette = new SlimeAppearance.Palette { Ammo = VacColor, Bottom = VacColor, Middle = VacColor, Top = VacColor };
            radiantAppearance._splatColor = VacColor;
        }
        
        slimeDef.CanLargofy = CanLargofy;
        
        slimeDef.localizedName = Localized;
        slimeDef._pediaPersistenceSuffix = "modded"+Name.ToLower()+"_slime";

        if (!DisableEdibleByTarrs)
            PrismNativeBaseSlime.Tarr.GetPrismBaseSlime().RefreshEatMap();
        var prismSlime = new PrismBaseSlime(slimeDef, false);
        prismSlime.AllowLargos = CanLargofy;;
        prismSlime.DisableAutoModdedLargos = DisableAutoModdedLargos;
        prismSlime.NonNativeBagSize = RadiantBagSize;
        if (CanLargofy&&CreateAllLargos)
        {
            PrismShortcuts.CreateLargoActions.Add(() =>
            {
                foreach (var slime in LookupEUtil.baseSlimeTypes)
                {
                    try
                    {
                        var otherSlimeDef = slime.Cast<SlimeDefinition>();
                        if(otherSlimeDef.ReferenceId==slimeDef.ReferenceId) continue;
                        var otherPrism = otherSlimeDef.GetPrismBaseSlime();
                        if (otherPrism.AllowLargos&& !(DisableAutoModdedLargos && !otherPrism.GetIsNative()))
                        {
                            var largoCreator = new PrismLargoCreatorV01(prismSlime, otherPrism);
                            if (CustomAutoLargoMergeSettings != null)
                                largoCreator.LargoMergeSettings = CustomAutoLargoMergeSettings;
                            else largoCreator.LargoMergeSettings = new PrismLargoMergeSettings();
                            largoCreator.CreateLargo();
                        }
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                        LogError("Error creating largo with "+slime.ReferenceId);
                    }
                }
            });
        }

        
        
        if(Plort!=null)
            PrismLibDiet.AddEatProduction(prismSlime, Plort);
        
        _createdSlime = prismSlime;
        PrismShortcuts.PrismBaseSlimes.Add(slimeDef.ReferenceId,_createdSlime);
        return _createdSlime;
    }

}