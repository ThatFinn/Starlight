using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Wrappers;

namespace Starlight.Prism.Lib;

/// <summary>
/// A library of helper functions for dealing with slime appearances.
/// </summary>
public static class PrismLibAppearances
{
    private const string TopColor = "_TopColor";
    private const string MiddleColor = "_MiddleColor";
    private const string BottomColor = "_BottomColor";
    private const string TwinTopColor = "_TwinTopColor";
    private const string TwinMiddleColor = "_TwinMiddleColor";
    private const string TwinBottomColor = "_TwinBottomColor";
    private const string SpecColor = "_SpecColor";
    private const string TwinSpecColor = "_TwinSpecColor";
    private const string SloomberTopColor = "_SloomberTopColor";
    private const string SloomberMiddleColor = "_SloomberMiddleColor";
    private const string SloomberBottomColor = "_SloomberBottomColor";
    private const string NoiseEdge = "_NoiseEdge";
    private const string SloomberStarMask = "_SloomberStarMask";
    private const string SloomberColorOverlay = "_SloomberColorOverlay";
    private const string StarMaskIntensityBodycoloringSloomber = "_StarMaskIntensity_BODYCOLORING_SLOOMBER";
    private const string StarTiling = "_StarTiling";
    private const string TwinEffectSize = "_TwinEffectSize";
    private const string TwinLineDivisionIntensity = "_TwinLineDivisionIntensity";
    private const string EnableVortex = "_EnableVortex";
    private const string VortexDistortionSize = "_VortexDistortionSize";
    private const string TwinVortexOffset = "_TwinVortexOffset";
    private const string TwinEffectOn = "_ENABLETWINEFFECT_ON";
    private const string BodyColoringSloomber = "_BODYCOLORING_SLOOMBER";

    #region Structure Management

    static bool IsRadiant(PrismAppearanceIndex index) =>
        index == PrismAppearanceIndex.BaseSlimeRadiant ? true :
        index == PrismAppearanceIndex.LargoRadiant1 ? true :
        index == PrismAppearanceIndex.LargoRadiant2;
    /// <summary>
    /// Gets a structure from a slime by index.
    /// </summary>
    public static SlimeAppearanceStructure GetStructure(this PrismSlime slime, int structureIndex, PrismAppearanceIndex appearanceIndex)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return null;
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null || structureIndex < 0 || structureIndex >= app.Structures.Count) return null;
        return app.Structures[structureIndex];
    }

    /// <summary>
    /// Gets a structure from a slime by element type.
    /// </summary>
    public static SlimeAppearanceStructure GetStructure(this PrismSlime slime, SlimeAppearanceElement.ElementType type, PrismAppearanceIndex appearanceIndex)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return null;
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return null;
        foreach (var s in app.Structures)
            if (s.Element.Type == type) return s;
        return null;
    }

    /// <summary>
    /// Replaces structures of a certain type
    /// </summary>
    public static void SetStructure(this PrismSlime slime, SlimeAppearanceStructure structure, SlimeAppearanceElement.ElementType type, PrismAppearanceIndex appearanceIndex)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return;
        for (int i = 0; i < app.Structures.Count; i++) 
            if (app.Structures[i].Element.Type == type)
            {
                app._structures = app._structures.ReplaceToNew(structure, i);
                break;
            }
    }
    /// <summary>
    /// Replaces structures of a certain type across all appearances.
    /// </summary>
    public static void SetStructureAll(this PrismSlime slime, SlimeAppearanceStructure structure, SlimeAppearanceElement.ElementType type)
    {
        foreach (var app in slime.GetSlimeDefinition().AppearancesDefault)
            for (int i = 0; i < app.Structures.Count; i++)
                if (app.Structures[i].Element.Type == type)
                {
                    app._structures = app._structures.ReplaceToNew(structure, i);
                    break;
                }
    }

    /// <summary>
    /// Adds a structure to all appearances.
    /// </summary>
    public static void AddStructure(this PrismSlime slime, SlimeAppearanceStructure structure, PrismAppearanceIndex appearanceIndex)
    {
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return;
        app._structures = app._structures.AddToNew(structure);
    }
    /// <summary>
    /// Adds a structure to all appearances.
    /// </summary>
    public static void AddStructureAll(this PrismSlime slime, SlimeAppearanceStructure structure)
    {
        foreach (var app in slime.GetSlimeDefinition().AppearancesDefault)
            app._structures = app._structures.AddToNew(structure);
    }

    /// <summary>
    /// Adds a new structure with a mesh to all appearances.
    /// </summary>
    public static void AddStructure(this PrismSlime slime, Mesh mesh, SlimeAppearance.SlimeBone rootBone, SlimeAppearance.SlimeBone parentBone, string elementName, PrismAppearanceIndex appearanceIndex)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return;
        var baseStruct = app._structures[0];
        var structPrefab = baseStruct?.Element.Prefabs[0]?.gameObject.CopyObject();
        if (structPrefab == null) return;
        var smr = structPrefab.GetComponent<SkinnedMeshRenderer>();
        if (smr != null) smr.sharedMesh = mesh;
            
        var structObj = structPrefab.GetComponent<SlimeAppearanceObject>();
        structObj.IgnoreLODIndex = true;
        structObj.RootBone = rootBone;
        structObj.ParentBone = parentBone;
        structObj.AttachedBones = new Il2CppStructArray<SlimeAppearance.SlimeBone>(0);
            
        var structure = new SlimeAppearanceStructure(baseStruct);
        structure.Element = ScriptableObject.CreateInstance<SlimeAppearanceElement>();
        structure.Element.CastsShadows = true;
        structure.Element.Name = elementName;
        structure.Element.Prefabs = new Il2CppReferenceArray<SlimeAppearanceObject>(new[] { structObj });
            
        app._structures = app._structures.AddToNew(structure);
        
    }

    /// <summary>
    /// Adds a new structure with a mesh to all appearances.
    /// </summary>
    public static void AddStructureAll(this PrismSlime slime, Mesh mesh, SlimeAppearance.SlimeBone rootBone, SlimeAppearance.SlimeBone parentBone, string elementName)
    {
        var def = slime.GetSlimeDefinition();
        foreach (var app in def.AppearancesDefault)
        {
            var baseStruct = app._structures[0];
            var structPrefab = baseStruct?.Element.Prefabs[0]?.gameObject.CopyObject();
            if (structPrefab == null) continue;

            var smr = structPrefab.GetComponent<SkinnedMeshRenderer>();
            if (smr != null) smr.sharedMesh = mesh;
            
            var structObj = structPrefab.GetComponent<SlimeAppearanceObject>();
            structObj.IgnoreLODIndex = true;
            structObj.RootBone = rootBone;
            structObj.ParentBone = parentBone;
            structObj.AttachedBones = new Il2CppStructArray<SlimeAppearance.SlimeBone>(0);
            
            var structure = new SlimeAppearanceStructure(baseStruct);
            structure.Element = ScriptableObject.CreateInstance<SlimeAppearanceElement>();
            structure.Element.CastsShadows = true;
            structure.Element.Name = elementName;
            structure.Element.Prefabs = new Il2CppReferenceArray<SlimeAppearanceObject>(new[] { structObj });
            
            app._structures = app._structures.AddToNew(structure);
        }
    }

    #endregion

    #region Coloring (PrismBaseSlime)

    /// <summary>
    /// Sets a color on a structure's material by structure index.
    /// </summary>
    public static void SetColor(this PrismBaseSlime slime, string property, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        var s = slime.GetStructure(structureIndex, appearanceIndex);
        if (s == null || materialIndex < 0 || materialIndex >= s.DefaultMaterials.Count) return;
        s.DefaultMaterials[materialIndex]?.SetColor(property, color);
    }

    /// <summary>
    /// Sets a color on a structure's material by element type.
    /// </summary>
    public static void SetColor(this PrismBaseSlime slime, string property, Color color, PrismAppearanceIndex appearanceIndex, SlimeAppearanceElement.ElementType type, int materialIndex = 0)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        var s = slime.GetStructure(type, appearanceIndex);
        if (s == null || materialIndex < 0 || materialIndex >= s.DefaultMaterials.Count) return;
        s.DefaultMaterials[materialIndex]?.SetColor(property, color);
    }

    // Individual colors
    public static void SetTopColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(TopColor, color, appearanceIndex, structureIndex, materialIndex);
    public static void SetMiddleColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(MiddleColor, color, appearanceIndex, structureIndex, materialIndex);
    public static void SetBottomColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(BottomColor, color, appearanceIndex, structureIndex, materialIndex);
    public static void SetSpecColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(SpecColor, color, appearanceIndex, structureIndex, materialIndex);

    public static void SetTwinTopColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(TwinTopColor, color, appearanceIndex, structureIndex, materialIndex);
    public static void SetTwinMiddleColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(TwinMiddleColor, color, appearanceIndex, structureIndex, materialIndex);
    public static void SetTwinBottomColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(TwinBottomColor, color, appearanceIndex, structureIndex, materialIndex);
    public static void SetTwinSpecColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(TwinSpecColor, color, appearanceIndex, structureIndex, materialIndex);

    public static void SetSloomberTopColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(SloomberTopColor, color, appearanceIndex, structureIndex, materialIndex);
    public static void SetSloomberMiddleColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(SloomberMiddleColor, color, appearanceIndex, structureIndex, materialIndex);
    public static void SetSloomberBottomColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex, int structureIndex = 0, int materialIndex = 0) => slime.SetColor(SloomberBottomColor, color, appearanceIndex, structureIndex, materialIndex);
    
    public static void SetPaletteTopColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return;
        var colorPalette = app._colorPalette;
        colorPalette.Top= color;
        app._colorPalette = colorPalette;
    }
    public static void SetPaletteMiddleColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return;
        var colorPalette = app._colorPalette;
        colorPalette.Middle = color;
        app._colorPalette = colorPalette;
    }
    public static void SetPaletteBottomColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return;
        var colorPalette = app._colorPalette;
        colorPalette.Bottom = color;
        app._colorPalette = colorPalette;
    }
    public static void SetPaletteAmmoColor(this PrismBaseSlime slime, Color color, PrismAppearanceIndex appearanceIndex)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return;
        var colorPalette = app._colorPalette;
        colorPalette.Ammo = color;
        app._colorPalette = colorPalette;
    }
    
    public static void SetRadiantColor(this PrismBaseSlime slime, Color? color, PrismAppearanceIndex appearanceIndex)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        SetRadiantColor_Radiant(slime, color, appearanceIndex);
    }

    static void SetRadiantColor_Radiant(PrismBaseSlime slime, Color? color, PrismAppearanceIndex appearanceIndex)
    {
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return;
        if (color.HasValue) app._radiantColor = color.Value;
        else app._radiantColor = new Color(0, 0, 0, 1);
        app.SetRadiantColor();
    }
    // Group Color setters
    public static void SetBaseColors(this PrismBaseSlime slime, PrismAppearanceIndex appearanceIndex, Color top, Color middle, Color bottom, Color? specular = null, int structureIndex = 0, int materialIndex = 0)
    {
        slime.SetTopColor(top, appearanceIndex, structureIndex, materialIndex);
        slime.SetMiddleColor(middle, appearanceIndex, structureIndex, materialIndex);
        slime.SetBottomColor(bottom, appearanceIndex, structureIndex, materialIndex);
        slime.SetSpecColor(specular ?? middle, appearanceIndex, structureIndex, materialIndex);
    }

    public static void SetTwinColors(this PrismBaseSlime slime, PrismAppearanceIndex appearanceIndex, Color top, Color middle, Color bottom, Color? specular = null, int structureIndex = 0, int materialIndex = 0)
    {
        slime.SetTwinTopColor(top, appearanceIndex, structureIndex, materialIndex);
        slime.SetTwinMiddleColor(middle, appearanceIndex, structureIndex, materialIndex);
        slime.SetTwinBottomColor(bottom, appearanceIndex, structureIndex, materialIndex);
        slime.SetTwinSpecColor(specular ?? middle, appearanceIndex, structureIndex, materialIndex);
    }

    public static void SetSloomberColors(this PrismBaseSlime slime, PrismAppearanceIndex appearanceIndex, Color top, Color middle, Color bottom, int structureIndex = 0, int materialIndex = 0)
    {
        slime.SetSloomberTopColor(top, appearanceIndex, structureIndex, materialIndex);
        slime.SetSloomberMiddleColor(middle, appearanceIndex, structureIndex, materialIndex);
        slime.SetSloomberBottomColor(bottom, appearanceIndex, structureIndex, materialIndex);
    }
    
    public static void SetPaletteColors(this PrismBaseSlime slime, PrismAppearanceIndex appearanceIndex, Color top, Color middle, Color bottom, Color ammo)
    {
        if (!SupportRadiant.HasFlag()&&IsRadiant(appearanceIndex)) return;
        var app = slime.GetSlimeDefinition().AppearancesDefault[(int)appearanceIndex];
        if (app == null) return;
        app._colorPalette = new SlimeAppearance.Palette() { Top = top, Middle = middle,Bottom = bottom, Ammo=ammo };
    }

    #endregion

    #region Effects

    public static void EnableTwinEffect(this PrismSlime slime, bool applyTextures = true)
    {
        var twinMat = PrismNativeBaseSlime.Twin.GetPrismBaseSlime()?.GetSlimeAppearance()?._structures[0]?.DefaultMaterials[0];
        foreach (var app in slime.GetSlimeDefinition().AppearancesDefault)
        {
            var mat = app.Structures[0]?.DefaultMaterials[0];
            if (mat == null) continue;
            mat.EnableKeyword(TwinEffectOn);
            if (applyTextures && twinMat != null) mat.SetTexture(NoiseEdge, twinMat.GetTexture(NoiseEdge));
        }
        slime.AdjustTwinEffect();
    }

    public static void EnableSloomberEffect(this PrismSlime slime, bool applyTextures = true)
    {
        var sloomberMat = PrismNativeBaseSlime.Sloomber.GetPrismBaseSlime()?.GetSlimeAppearance()?._structures[0]?.DefaultMaterials[0];
        foreach (var app in slime.GetSlimeDefinition().AppearancesDefault)
        {
            var mat = app.Structures[0]?.DefaultMaterials[0];
            if (mat == null) continue;
            mat.EnableKeyword(BodyColoringSloomber);
            if (applyTextures && sloomberMat != null)
            {
                mat.SetTexture(SloomberStarMask, sloomberMat.GetTexture(SloomberStarMask));
                mat.SetTexture(SloomberColorOverlay, sloomberMat.GetTexture(SloomberColorOverlay));
            }
        }
        slime.AdjustSloomberSparkles();
    }

    public static void AdjustSloomberSparkles(this PrismSlime slime, float size = 3.26f, float intensity = 1)
    {
        foreach (var app in slime.GetSlimeDefinition().AppearancesDefault)
        {
            var mat = app.Structures[0]?.DefaultMaterials[0];
            if (mat == null) continue;
            mat.SetFloat(StarTiling, size);
            mat.SetFloat(StarMaskIntensityBodycoloringSloomber, intensity);
        }
    }

    public static void AdjustTwinEffect(this PrismSlime slime, float size = 1f, float intensity = 0.8f, bool vortex = true, float vortexDistortion = 12f, Vector4? vortexOffset = null)
    {
        Vector4 offset = vortexOffset ?? new Vector4(0, -0.5f, 0, 0);
        foreach (var app in slime.GetSlimeDefinition().AppearancesDefault)
        {
            var mat = app.Structures[0]?.DefaultMaterials[0];
            if (mat == null) continue;
            mat.SetFloat(TwinEffectSize, size);
            mat.SetFloat(TwinLineDivisionIntensity, intensity);
            mat.SetFloat(EnableVortex, vortex ? 1f : 0f);
            mat.SetFloat(VortexDistortionSize, vortexDistortion);
            mat.SetVector(TwinVortexOffset, offset);
        }
    }

    public static void DisableTwinEffect(this PrismSlime slime)
    {
        foreach (var app in slime.GetSlimeDefinition().AppearancesDefault)
            app.Structures[0]?.DefaultMaterials[0]?.DisableKeyword(TwinEffectOn);
    }

    #endregion

    #region Plorts

    public static void SetBaseColors(this PrismPlort plort, Color top, Color middle, Color bottom)
    {
        var material = plort.GetPrefab().GetComponent<MeshRenderer>().sharedMaterial;
        material.SetColor(TopColor, top);
        material.SetColor(MiddleColor, middle);
        material.SetColor(BottomColor, bottom);
    }

    public static void SetTwinColors(this PrismPlort plort, Color top, Color middle, Color bottom)
    {
        var material = plort.GetPrefab().GetComponent<MeshRenderer>().sharedMaterial;
        material.SetColor(TwinTopColor, top);
        material.SetColor(TwinMiddleColor, middle);
        material.SetColor(TwinBottomColor, bottom);
    }

    #endregion
    
    #region Liquids
    
    public static void SetColors(this PrismLiquid liquid, Color center, Color baseColor, Color edge, Color emission)
    {
        var sphere = liquid.GetPrefab().GetObjectRecursively<MeshRenderer>("Sphere");
        if(sphere==null)
            LogError("You need to have a child called 'Sphere' with a MeshRenderer for this o work");
        var material = sphere.sharedMaterial;
        material.SetColor("_ColorBase", baseColor);
        material.SetColor("_ColorCenter", center);
        material.SetColor("_ColorEdge", edge);
        material.SetColor("_RimTint", edge);
        material.SetColor("_EmissionColor", emission);
    }
    
    public static void SetBaseColor(this PrismLiquid liquid, Color baseColor)
    {
        var sphere = liquid.GetPrefab().GetObjectRecursively<MeshRenderer>("Sphere");
        if(sphere==null)
            LogError("You need to have a child called 'Sphere' with a MeshRenderer for this o work");
        var material = sphere.sharedMaterial;
        material.SetColor("_ColorBase", baseColor);
    }
    public static void SetCenterColor(this PrismLiquid liquid, Color center)
    {
        var sphere = liquid.GetPrefab().GetObjectRecursively<MeshRenderer>("Sphere");
        if(sphere==null)
            LogError("You need to have a child called 'Sphere' with a MeshRenderer for this o work");
        var material = sphere.sharedMaterial;
        material.SetColor("_ColorCenter", center);
    }
    
    
    public static void SetEdgeColor(this PrismLiquid liquid, Color edge)
    {
        var sphere = liquid.GetPrefab().GetObjectRecursively<MeshRenderer>("Sphere");
        if(sphere==null)
            LogError("You need to have a child called 'Sphere' with a MeshRenderer for this o work");
        var material = sphere.sharedMaterial;
        material.SetColor("_ColorEdge", edge);
        material.SetColor("_RimTint", edge);
    }
    
    public static void SetEmissionColor(this PrismLiquid liquid, Color emission)
    {
        var sphere = liquid.GetPrefab().GetObjectRecursively<MeshRenderer>("Sphere");
        if(sphere==null)
            LogError("You need to have a child called 'Sphere' with a MeshRenderer for this o work");
        var material = sphere.sharedMaterial;
        material.SetColor("_EmissionColor", emission);
    }
    
    #endregion
}