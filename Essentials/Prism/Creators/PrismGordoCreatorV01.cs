using Starlight.Prism.Lib;
using Starlight.Prism.Patches;
using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

public class PrismGordoCreatorV01
{
    private PrismGordo _createdGordo;
    
    public Sprite Icon;
    public LocalizedString Localized;
    public PrismBaseSlime BaseSlime = null;
    public string referenceID => "IdentifiableType.Modded" + BaseSlime.SlimeDefinition.name +"Gordo";

    public bool DisableAutomaticStructureGeneration = true;
    
    public int CustomMaxEatCount = 0;
    public Material CustomEyesBlink;
    public Material CustomEyesNormal;
    public Material CustomMouthHappy;
    public Material CustomMouthChompOpen;
    public Material CustomMouthEating;
    
    public PrismGordoCreatorV01(PrismBaseSlime baseSlime, Sprite icon, LocalizedString localized)
    {
        this.BaseSlime = baseSlime;
        this.Icon = icon;
        this.Localized = localized;
    }
    
    public bool IsValid()
    {
        if (BaseSlime==null) return false;
        if (Localized==null) return false;
        return true;
    }


    private static IdentifiableType baseType = null;
    
    public PrismGordo CreateGordo()
    {
        if (!IsValid()) return null;
        if (_createdGordo != null) return _createdGordo;

        var baseMaterial = BaseSlime.GetBaseMaterial();
        if (baseMaterial == null) return null;
        if (baseType == null) baseType = Get<IdentifiableType>("PinkGordo");
        if (baseType == null) return null;
        var gordoType = Object.Instantiate(baseType);
        gordoType.name = BaseSlime.SlimeDefinition.name.ToLower() + "ModdedGordo";
        gordoType.icon = Icon ?? PrismShortcuts.UnavailableIcon;
        gordoType.localizedName = Localized;
        gordoType.referenceId = referenceID;
        gordoType._pediaPersistenceSuffix=BaseSlime.SlimeDefinition.name.ToLower()+"_gordo";
        
        gordoType.Prism_AddToGroup("GordoGroup");
        
        PrismLibSaving.SetupForSaving(gordoType,referenceID);

        var gordo = baseType.prefab.CopyObject();
        gordo.GetComponent<GordoIdentifiable>().identType = gordoType;
        gordo.GetComponent<GordoEat>().SlimeDefinition = BaseSlime;
        if(CustomMaxEatCount!=0)
            gordo.GetComponent<GordoEat>().TargetCount = CustomMaxEatCount;
        
        gordo.hideFlags = HideFlags.DontUnloadUnusedAsset;
        
        var faceComp = gordo.GetComponent<GordoFaceComponents>();
        
        var baseAppearance = BaseSlime.GetSlimeAppearance();
        faceComp.BlinkEyes = baseAppearance.Face.GetExpressionFace(SlimeFace.SlimeExpression.BLINK).Eyes;
        faceComp.StrainEyes = baseAppearance.Face.GetExpressionFace(SlimeFace.SlimeExpression.SCARED).Eyes;
        
        if (CustomEyesBlink != null) faceComp.BlinkEyes = CustomEyesBlink;
        if (CustomEyesNormal != null) faceComp.StrainEyes = CustomEyesNormal;
        if (CustomMouthHappy != null) faceComp.HappyMouth = CustomMouthHappy;
        if (CustomMouthChompOpen != null) faceComp.ChompOpenMouth = CustomMouthChompOpen;
        if (CustomMouthEating != null) faceComp.StrainMouth = CustomMouthEating;
        
        var meshRenderer = gordo.GetObjectRecursively<SkinnedMeshRenderer>("slime_gordo");
        var i = 0;
        meshRenderer.material = Object.Instantiate(baseMaterial);
        meshRenderer.materials = new List<Material>() { 
            meshRenderer.material,
            Object.Instantiate(faceComp.BlinkEyes),
            Object.Instantiate(faceComp.HappyMouth) }.ToArray();

        if (!DisableAutomaticStructureGeneration)
        {
            GordoEatAwakePatch.Gordos.Add(gordoType,BaseSlime);
        }
        
        gordoType.prefab = gordo;

        gameContext.LookupDirector._gordoDict.Add(gordoType, gordo);
        gameContext.LookupDirector._gordoEntries.items.Add(gordo);
        
        
        
        var gordoSlime = new PrismGordo(gordoType, false);
        
        
        _createdGordo = gordoSlime;
        PrismShortcuts.PrismGordos.Add(gordoType.ReferenceId,gordoSlime);
        return gordoSlime;
    }
}