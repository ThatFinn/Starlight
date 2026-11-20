using System;
using UObject = UnityEngine.Object;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Patches;
using Starlight.Prism.Lib;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// A request to create a gordo slime
/// </summary>
public record GordoRequest
{
    /// <summary>
    /// The slime this is a gordo of
    /// </summary>
    /// <remarks> Its colors, face and body structures carry over </remarks>
    public required CreatedSlime Slime { get; init; }

    /// <summary>
    /// The Display name
    /// </summary>
    public required LocalizedString DisplayName { get; init; }

    /// <summary>
    /// The icon on the map and in the slimepedia
    /// </summary>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// How much it eats before it bursts
    /// </summary>
    /// <remarks> Null for the copied gordo slime's amount </remarks>
    public int? TargetCount { get; init; }

    /// <summary>
    /// What happens when something happens to this gordo
    /// </summary>
    /// <remarks> Examples: it is fed, it is fed a favourite or it bursts </remarks>
    public GordoActions? Actions { get; init; }
}

/// <summary>
/// A gordo slime created by Prism
/// </summary>
/// <param name="Type"> The live identifiable type </param>
/// <param name="ReferenceId"> Its save id </param>
/// <param name="ObjectName"> Its object name </param>
public sealed record CreatedGordo(
    IdentifiableType Type,
    string ReferenceId,
    string ObjectName);

/// <summary>
/// The gordo slime creator
/// </summary>
public static class PrismGordoCreatorV02
{
    /// <summary>
    /// The suffix of every gordo slime's name
    /// </summary>
    private const string Kind = "Gordo";

    /// <summary>
    /// Creates a gordo slime from a <see cref="GordoRequest"/>
    /// </summary>
    public static CreatedGordo? Create(GordoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsGameReady)
        {
            LogError("Prism: Can't create a gordo slime: the game isn't ready");
            return null;
        }

        var slime = request.Slime;

        var template = Template();
        if (template.Type == null || template.Prefab == null)
        {
            LogError($"Prism: Can't create gordo slime for '{slime.ObjectName}': no native gordo slime to copy");
            return null;
        }

        var stripped = ContentNames.WithoutKindSuffix(slime.ObjectName, Kind);
        var name = ContentNames.Unique(stripped, Kind);
        var objectName = name + Kind;
        var referenceId = ContentNames.ReferenceId(
            "IdentifiableType", ContentNames.StableName(slime.ObjectName, Kind), Kind);

        var stableName = ContentNames.StableName(slime.ObjectName, Kind);
        var legacyIds = new[]
        {
            ContentNames.LegacyReferenceId("IdentifiableType", name, Kind),
            ContentNames.LegacyReferenceId("IdentifiableType", stableName, Kind)
        };

        var type                     = UObject.Instantiate(template.Type);
        type.hideFlags               = HideFlags.DontUnloadUnusedAsset;
        type.name                    = objectName;
        type.localizedName           = request.DisplayName;
        type.icon                    = request.Icon ?? template.Type.icon;
        PageArt.ClearFullArt(type);
        type.color                   = template.Type.color;
        PediaSaveIds.SetSuffix(type, stableName, name, Kind.ToLowerInvariant());

        var prefab = Prefabs.Create(objectName + "Prefab", template.Prefab);

        Wire(prefab, type, slime, request.TargetCount);

        type.prefab = prefab;
        
        if (!SaveRegistry.Register(type, referenceId, legacyIds))
        {
            Discard(type, prefab);
            return null;
        }

        Groups.Add(type, Groups.Names.AllIdentifiableTypes);
        Groups.CopyGroups(type, template.Type);

        if (!Register(type, prefab, objectName))
        {
            Discard(type, prefab);
            return null;
        }

        GordoEvents.Register(type, request.Actions);

        return new CreatedGordo(type, referenceId, objectName);
    }

    /// <summary>
    /// Wires the new slime on to the gordo slime prefab
    /// </summary>
    private static void Wire(
        GameObject prefab,
        IdentifiableType type,
        CreatedSlime slime,
        int? targetCount)
    {
        var identifiable = prefab.GetComponent<GordoIdentifiable>();
        if (identifiable != null) identifiable.identType = type;

        var eat = prefab.GetComponent<GordoEat>();
        if (eat != null)
        {
            eat.SlimeDefinition = slime.Definition;

            if (targetCount is > 0) eat.TargetCount = targetCount.Value;
        }

        ApplyFace(prefab, slime);
        ApplyColours(prefab, slime);
        CarryStructures(prefab, slime);
    }

    /// <summary>
    /// Gives the gordo the slime's eyes and mouth
    /// </summary>
    private static void ApplyFace(GameObject prefab, CreatedSlime slime)
    {
        var face = prefab.GetComponent<GordoFaceComponents>();
        if (face == null) return;

        var slimeFace = slime.Appearance?._face;
        if (slimeFace == null) return;
        
        if (Expression(slimeFace, SlimeFace.SlimeExpression.BLINK) is { Eyes: not null } blink)
        {
            face.BlinkEyes = blink.Eyes;
        }

        if (Expression(slimeFace, SlimeFace.SlimeExpression.SCARED) is { } scared)
        {
            if (scared.Eyes != null) face.StrainEyes = scared.Eyes;
            if (scared.Mouth != null) face.StrainMouth = scared.Mouth;
        }

        if (Expression(slimeFace, SlimeFace.SlimeExpression.HAPPY) is { Mouth: not null } happy)
        {
            face.HappyMouth = happy.Mouth;
        }
    }

    private static FaceEntry? Expression(SlimeFace face, SlimeFace.SlimeExpression wanted)
    {
        var count = FaceExpressionTable.Count(face);

        for (var index = 0; index < count; index++)
        {
            if (FaceExpressionTable.TryGet(face, index, out var entry) && entry.Expression == wanted)
                return entry;
        }

        return null;
    }

    /// <summary>
    /// Paints the gordo slime in the slime's colors
    /// </summary>
    private static void ApplyColours(GameObject prefab, CreatedSlime slime)
    {
        var body = SlimeBodyMaterial(slime);
        if (body == null) return;

        var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);

        foreach (var renderer in renderers)
        {
            if (renderer.materials == null || renderer.materials.Length == 0) continue;

            var materials = renderer.materials;
            materials[0] = UObject.Instantiate(body);
            materials[0]!.hideFlags = HideFlags.DontUnloadUnusedAsset;
            renderer.materials = materials;
        }
    }

    private static Material? SlimeBodyMaterial(CreatedSlime slime)
    {
        var structures = slime.Appearance?.Structures;
        if (structures == null) return null;

        foreach (var structure in structures.AsEnumerable())
        {
            if (structure?.DefaultMaterials is { Length: > 0 } materials && materials[0] != null)
                return materials[0];
        }

        return null;
    }

    /// <summary>
    /// Carries the structures over from the slime
    /// </summary>
    private static void CarryStructures(GameObject prefab, CreatedSlime slime)
    {
        var structures = slime.Appearance?.Structures;
        if (structures == null) return;

        var host = BodyOf(prefab);
        if (host == null) return;

        var scale = ScaleOf(prefab);

        foreach (var structure in structures.AsEnumerable())
        {
            try
            {
                AttachStructure(structure, host, scale);
            }
            catch { }
        }
    }

    private static bool AttachStructure(SlimeAppearanceStructure? structure, Transform host, float scale)
    {
        var prefabs = structure?.Element?.Prefabs;
        if (prefabs == null || prefabs.Length == 0) return false;

        var source = prefabs[0];
        if (source == null) return false;

        var filter = source.GetComponent<MeshFilter>();
        if (filter?.sharedMesh == null) return false;

        var piece = new GameObject(source.name + "OnGordo");
        piece.transform.SetParent(host, worldPositionStays: false);
        
        piece.transform.localPosition = source.transform.localPosition * scale;
        piece.transform.localRotation = source.transform.localRotation;
        piece.transform.localScale = source.transform.localScale * scale;

        piece.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;

        var renderer = piece.AddComponent<MeshRenderer>();
        var materials = structure!.DefaultMaterials;

        if (materials is { Length: > 0 } && materials[0] != null)
            renderer.material = materials[0];

        return true;
    }

    private static Transform? BodyOf(GameObject prefab)
    {
        foreach (var child in prefab.GetComponentsInChildren<Transform>(true))
            if (child.name.Contains("Vibrat", StringComparison.OrdinalIgnoreCase)) return child;
        
        var renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
        return renderer != null ? renderer.transform.parent : prefab.transform;
    }

    /// <summary>
    /// How much bigger the gordo slime is than its base
    /// </summary>
    private static float ScaleOf(GameObject prefab)
    {
        var renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>(includeInactive: true);
        if (renderer?.sharedMesh == null) return DefaultScale;
        
        var size = renderer.sharedMesh.bounds.size;
        var largest = Mathf.Max(size.x, Mathf.Max(size.y, size.z));

        return largest > 0.1f ? largest : DefaultScale;
    }

    /// <summary>
    /// The fallback scale
    /// </summary>
    private const float DefaultScale = 5f;

    /// <summary>
    /// An existing gordo slime to copy
    /// </summary>
    private static (IdentifiableType? Type, GameObject? Prefab) Template()
    {
        var director = GameContext.Instance?.LookupDirector;
        var known = director?._gordoDict;

        if (known == null) return (null, null);

        foreach (var pair in known)
            if (pair.Key != null && pair.Value != null) return (pair.Key, pair.Value);

        return (null, null);
    }

    /// <summary>
    /// Registers a gordo slime
    /// </summary>
    private static bool Register(IdentifiableType type, GameObject prefab, string objectName)
    {
        var director = GameContext.Instance?.LookupDirector;
        if (director == null)
        {
            LogError($"Prism: Can't register gordo '{objectName}': no lookup director");
            return false;
        }

        try
        {
            if (director._gordoDict != null && !director._gordoDict.ContainsKey(type))
                director._gordoDict.Add(type, prefab);

            director._gordoEntries?.items?.Add(prefab);
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to register gordo '{objectName}'", ex);
            return false;
        }
    }

    private static void Discard(IdentifiableType type, GameObject prefab)
    {
        try
        {
            if (prefab != null) UObject.Destroy(prefab);
            UObject.Destroy(type);
        }
        catch { }
    }
}