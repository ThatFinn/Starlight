using System;
using System.Linq;
using UObject = UnityEngine.Object;
using Starlight.Prism.Lib;

namespace Starlight.Prism.Data.Appearance;

/// <summary> A mesh piece attached to a slime's body </summary>
/// <param name="Name"> Display name of the structure</param>
/// <param name="Mesh"> The mesh to attach </param>
/// <param name="Texture"> Texture for the structure, or null to reuse the slime body's material </param>
/// <param name="Material"> A material for the structure </param>
/// <param name="EulerAngles"> Local rotation relative to the bone it's attached to </param>
/// <param name="Position"> Local offset relative to the bone it hangs from </param>
/// <param name="Scale"> Local scale </param>
/// <param name="ParentBone"> Which bone the structure follows </param>
/// <param name="RootBone"> Bone used as the deformation root </param>
/// <param name="ElementType"> How the game categorises the piece (ears, tail, ...) </param>
public readonly record struct SlimeStructureRequest(
    string Name,
    Mesh Mesh,
    Texture2D? Texture,
    Material? Material,
    Vector3 EulerAngles,
    Vector3 Position,
    Vector3 Scale,
    SlimeAppearance.SlimeBone ParentBone,
    SlimeAppearance.SlimeBone RootBone,
    SlimeAppearanceElement.ElementType ElementType);

/// <summary>
/// Attaches and removes mesh pieces on a slime's appearance
/// </summary>
public static class SlimeStructures
{
    /// <summary>
    /// Attaches a mesh piece to the appearance
    /// </summary>
    /// <returns> True if the piece was attached </returns>
    public static bool Attach(SlimeAppearance appearance, string ownerName, SlimeStructureRequest request)
    {
        ArgumentNullException.ThrowIfNull(appearance);

        if (appearance.Structures == null || appearance.Structures.Length == 0)
        {
            LogWarning($"Prism: Can't attach '{request.Name}': the slime has no structures to take a material from.");
            return false;
        }

        var pieceName = ownerName + ContentNames.Sanitize(request.Name);
        RemoveConflicting(appearance, request, ownerName);

        var template = appearance.Structures[0];
        
        var host = new GameObject(pieceName) { hideFlags = HideFlags.DontUnloadUnusedAsset };
        UObject.DontDestroyOnLoad(host);

        host.transform.localEulerAngles = request.EulerAngles;
        host.transform.localPosition = request.Position;
        host.transform.localScale = request.Scale;

        host.AddComponent<MeshFilter>().sharedMesh = request.Mesh;
        var renderer = host.AddComponent<MeshRenderer>();

        var appearanceObject = host.AddComponent<SlimeAppearanceObject>();
        appearanceObject.ParentBone = request.ParentBone;
        appearanceObject.RootBone = request.RootBone;
        appearanceObject.IgnoreLODIndex = true;

        var structure = new SlimeAppearanceStructure(template) { SupportsFaces = false };
        ApplyMaterial(structure, template, renderer, request, appearance);

        var element = ScriptableObject.CreateInstance<SlimeAppearanceElement>();
        element.hideFlags = HideFlags.DontUnloadUnusedAsset;
        element.name      = pieceName;
        element.Name      = request.Name;
        element.Type      = request.ElementType;
        
        element.MaterialInheritance = SlimeAppearanceElement.MatInheritType.NONE;
        element.Prefabs = new[] { appearanceObject }.ToIl2CppArray();

        structure.Element = element;
        appearance.Structures = appearance.Structures.AddToNew(structure);
        return true;
    }

    /// <summary>
    /// Copies another slime's body parts onto this one
    /// </summary>
    /// <returns> The names of the parts copied. Empty if the appearance already had them all </returns>
    public static IReadOnlyList<string> Borrow(SlimeAppearance appearance, SlimeAppearance donor)
    {
        ArgumentNullException.ThrowIfNull(appearance);
        ArgumentNullException.ThrowIfNull(donor);

        if (donor.Structures == null || donor.Structures.Length == 0) return Array.Empty<string>();

        var present = appearance.Structures.AsEnumerable()
            .Select(structure => structure?.Element?.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var copied = new List<string>();

        foreach (var structure in donor.Structures.AsEnumerable())
        {
            if (structure?.Element == null) continue;
            if (IsBody(structure)) continue;

            var name = structure.Element.Name;
            if (string.IsNullOrEmpty(name) || !present.Add(name)) continue;

            var copy = AppearanceCloner.CloneStructure(structure);
            
            foreach (var material in copy.DefaultMaterials.AsEnumerable())
                if (material != null) Recolor(material, appearance);

            appearance.Structures = appearance.Structures.AddToNew(copy);
            copied.Add(name);
        }

        return copied;
    }

    /// <summary>
    /// Whether a structure is the body rather than a piece hanging off it
    /// </summary>
    private static bool IsBody(SlimeAppearanceStructure structure) =>
        structure.SupportsFaces
        || structure.Element.Type == SlimeAppearanceElement.ElementType.BODY
        || structure.Element.Name?.Contains("Body", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Gives the new piece its own material with the artwork
    /// </summary>
    private static void ApplyMaterial(
        SlimeAppearanceStructure structure,
        SlimeAppearanceStructure template,
        MeshRenderer renderer,
        SlimeStructureRequest request,
        SlimeAppearance appearance)
    {
        if (request.Material != null && StarlightEntryPoint.prismStructureArtwork)
        {
            Recolor(request.Material, appearance);

            renderer.material = request.Material;
            structure.DefaultMaterials = new[] { request.Material }.ToIl2CppArray();
            return;
        }

        if (template.DefaultMaterials is not { Length: > 0 })
        {
            structure.DefaultMaterials = template.DefaultMaterials;
            return;
        }

        var material = UObject.Instantiate(template.DefaultMaterials[0]);
        material.hideFlags = HideFlags.DontUnloadUnusedAsset;

        if (request.Texture != null && HasArtworkSlot(material))
            MaterialTextures.ApplyToAllTextureSlots(material, request.Texture);

        Recolor(material, appearance);

        renderer.material = material;
        structure.DefaultMaterials = new[] { material }.ToIl2CppArray();
    }

    /// <summary>
    /// Recolors a structure to be in the colours of the new slime
    /// </summary>
    private static void Recolor(Material material, SlimeAppearance appearance)
    {
        var palette = appearance._colorPalette;
        var written = SlimeColors.Apply(material, new SlimePalette(palette.Top, palette.Middle, palette.Bottom));

        if (written > 0) return;
        
        SlimeColors.Tint(material, palette.Middle);
    }
    
    private static readonly string[] ArtworkProperties = { "_MainTex", "_BaseMap", "_Albedo", "_BaseColorMap", "_PrimaryTex" };

    /// <summary>
    /// Whether this material's shader can display a texture at all
    /// </summary>
    private static bool HasArtworkSlot(Material material) =>
        ArtworkProperties.Any(material.HasProperty);

    /// <summary>
    /// Removes base structures the incoming piece would duplicate
    /// </summary>
    private static void RemoveConflicting(
        SlimeAppearance appearance,
        SlimeStructureRequest request,
        string ownerName)
    {
        var conflicts = BodyPartKeywords
            .Where(part => request.Name.Contains(part.Keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (conflicts.Count == 0) return;

        var kept = new List<SlimeAppearanceStructure>();
        foreach (var structure in appearance.Structures.AsEnumerable())
        {
            if (structure == null) continue;

            var element = structure.Element;
            if (element != null &&
                element.name.StartsWith(ownerName, StringComparison.Ordinal))
            {
                kept.Add(structure);
                continue;
            }

            var name = element?.Name;
            var type = element?.Type;

            var isConflict = conflicts.Any(part =>
                (name != null && name.Contains(part.Keyword, StringComparison.OrdinalIgnoreCase)) ||
                (type != null && type == part.ElementType));

            if (!isConflict) kept.Add(structure);
        }

        if (kept.Count != appearance.Structures.Length)
            appearance.Structures = kept.ToIl2CppArray();
    }

    private static readonly (string Keyword, SlimeAppearanceElement.ElementType ElementType)[] BodyPartKeywords =
    {
        ("ear", SlimeAppearanceElement.ElementType.EARS),
        ("tail", SlimeAppearanceElement.ElementType.TAIL),
        ("wing", SlimeAppearanceElement.ElementType.WINGS),
        ("whisker", SlimeAppearanceElement.ElementType.WHISKERS),
        ("antenna", SlimeAppearanceElement.ElementType.ANTENNAE),
        ("glasses", SlimeAppearanceElement.ElementType.GLASSES)
    };

    /// <summary>
    /// Which body part a structure is
    /// </summary>
    public static SlimeAppearanceElement.ElementType ElementTypeFor(string partName)
    {
        ArgumentNullException.ThrowIfNull(partName);

        foreach (var (keyword, type) in PartNameKeywords)
            if (partName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) return type;

        return SlimeAppearanceElement.ElementType.EARS;
    }

    /// <summary>
    /// Name keywords to element types
    /// </summary>
    private static readonly (string Keyword, SlimeAppearanceElement.ElementType ElementType)[] PartNameKeywords =
    {
        ("whisker", SlimeAppearanceElement.ElementType.WHISKERS),
        ("antenna", SlimeAppearanceElement.ElementType.ANTENNAE),
        ("glasses", SlimeAppearanceElement.ElementType.GLASSES),
        ("hyperaura", SlimeAppearanceElement.ElementType.HYPERAURA),
        ("sloomber", SlimeAppearanceElement.ElementType.SLOOMBERAURA),
        ("aura", SlimeAppearanceElement.ElementType.AURA),
        ("orbit", SlimeAppearanceElement.ElementType.ORBITAL),
        ("forehead", SlimeAppearanceElement.ElementType.FOREHEAD),
        ("core", SlimeAppearanceElement.ElementType.CORE),
        ("surface", SlimeAppearanceElement.ElementType.SURFACE),
        ("wing", SlimeAppearanceElement.ElementType.WINGS),
        ("tail", SlimeAppearanceElement.ElementType.TAIL),
        ("ear", SlimeAppearanceElement.ElementType.EARS),

        // Leftover of SRCL
        ("horn", SlimeAppearanceElement.ElementType.TOP),
        ("halo", SlimeAppearanceElement.ElementType.TOP),
        ("crown", SlimeAppearanceElement.ElementType.TOP),
        ("hat", SlimeAppearanceElement.ElementType.TOP),
        
        ("side", SlimeAppearanceElement.ElementType.SIDE)
    };
}