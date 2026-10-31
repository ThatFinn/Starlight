using System;
using System.Linq;
using UObject = UnityEngine.Object;
using Starlight.Prism.Lib;

namespace Starlight.Prism.Data.Appearance;

/// <summary>
/// Copies a slime's appearance
/// </summary>
public static class AppearanceCloner
{
    /// <summary>
    /// Returns a copy of the <paramref name="source"/> appearance
    /// </summary>
    /// <param name="source"> The appearance to copy </param>
    /// <param name="name"> Object name for the copy </param>
    public static SlimeAppearance Clone(SlimeAppearance source, string name)
    {
        ArgumentNullException.ThrowIfNull(source);

        var copy = UObject.Instantiate(source);
        copy.hideFlags = HideFlags.DontUnloadUnusedAsset;
        copy.name = name;

        CloneStructures(copy, source);
        CloneSubAppearances(copy, source);

        return copy;
    }

    /// <summary>
    /// Replaces the copy's structure array with independent structures
    /// </summary>
    private static void CloneStructures(SlimeAppearance copy, SlimeAppearance source)
    {
        if (source.Structures == null) return;

        var structures = new List<SlimeAppearanceStructure>();

        foreach (var sourceStructure in source.Structures)
        {
            if (sourceStructure == null) continue;
            structures.Add(CloneStructure(sourceStructure));
        }

        copy.Structures = structures.ToIl2CppArray();
    }

    /// <summary>
    /// Copies one structure
    /// </summary>
    internal static SlimeAppearanceStructure CloneStructure(SlimeAppearanceStructure source)
    {
        var structure = new SlimeAppearanceStructure(source)
        {
            DefaultMaterials = CloneMaterials(source.DefaultMaterials)
        };

        if (source.Element != null)
            structure.Element = CloneElement(source.Element);

        if (source.ElementMaterials != null)
            structure.ElementMaterials = CloneElementMaterials(source.ElementMaterials);

        return structure;
    }
    
    private static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Material> CloneMaterials(
        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Material>? materials)
        => materials.AsEnumerable()
            .Where(material => material != null)
            .Select(UObject.Instantiate)
            .ToIl2CppArray();

    private static SlimeAppearanceElement CloneElement(SlimeAppearanceElement source)
    {
        var element = UObject.Instantiate(source);
        element.hideFlags = HideFlags.DontUnloadUnusedAsset;
        
        element.Prefabs = source.Prefabs.AsEnumerable()
            .Where(prefab => prefab != null)
            .Select(prefab => Prefabs.Create($"copy_{prefab.name}", prefab.gameObject)
                .GetComponent<SlimeAppearanceObject>())
            .ToIl2CppArray();

        return element;
    }

    private static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<SlimeAppearanceMaterials> CloneElementMaterials(
        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<SlimeAppearanceMaterials> source)
        => source.AsEnumerable()
            .Where(entry => entry != null)
            .Select(entry => new SlimeAppearanceMaterials
            {
                OverrideDefaults = entry.OverrideDefaults,
                Materials = CloneMaterials(entry.Materials)
            })
            .ToIl2CppArray();

    /// <summary>
    /// Copies the sub appearances
    /// </summary>
    private static void CloneSubAppearances(SlimeAppearance copy, SlimeAppearance source)
    {
        if (source._face != null) copy._face = UObject.Instantiate(source._face);
        if (source._idleAnimationOverride != null) copy._idleAnimationOverride = UObject.Instantiate(source._idleAnimationOverride);
        if (source._wingFlapAnimationOverride != null) copy._wingFlapAnimationOverride = UObject.Instantiate(source._wingFlapAnimationOverride);
        if (source._biteAnimationOverride != null) copy._biteAnimationOverride = UObject.Instantiate(source._biteAnimationOverride);
        if (source._crystalAppearance != null) copy._crystalAppearance = UObject.Instantiate(source._crystalAppearance);
        if (source._deathAppearance != null) copy._deathAppearance = UObject.Instantiate(source._deathAppearance);
        if (source._explosionAppearance != null) copy._explosionAppearance = UObject.Instantiate(source._explosionAppearance);
        if (source._glintAppearance != null) copy._glintAppearance = UObject.Instantiate(source._glintAppearance);
        if (source._qubitAppearance != null) copy._qubitAppearance = UObject.Instantiate(source._qubitAppearance);
        if (source._roarAppearance != null) copy._roarAppearance = UObject.Instantiate(source._roarAppearance);
        if (source._shockedAppearance != null) copy._shockedAppearance = UObject.Instantiate(source._shockedAppearance);
        if (source._stoneFormAppearance != null) copy._stoneFormAppearance = UObject.Instantiate(source._stoneFormAppearance);
        if (source._tornadoAppearance != null) copy._tornadoAppearance = UObject.Instantiate(source._tornadoAppearance);
        if (source._vineAppearance != null) copy._vineAppearance = UObject.Instantiate(source._vineAppearance);
    }
}