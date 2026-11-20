using System;
using UObject = UnityEngine.Object;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Lib;
using Starlight.Prism.Patches;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// A request to create an ordinary item
/// </summary>
public record ItemRequest
{
    /// <summary>
    /// The raw identifier
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The Display name
    /// </summary>
    public required LocalizedString DisplayName { get; init; }

    /// <summary>
    /// Existing content copied for it's structure
    /// </summary>
    public required IdentifiableType Template { get; init; }

    /// <summary>
    /// Suffix appended to the object name, e.g. "Plort" / "Material"
    /// </summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>
    /// The inventory icon
    /// </summary>
    /// <remarks> Null falls back to the template's icon </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// It's tint
    /// </summary>
    /// <remarks> Null keeps the template's tint </remarks>
    public Color? Color { get; init; }

    /// <summary>
    /// It's custom model
    /// </summary>
    /// <remarks> Null keeps the template's model </remarks>
    public Mesh? Mesh { get; init; }

    /// <summary>
    /// Custom texture over the template's material
    /// </summary>
    /// <remarks> Null keeps the template's texture </remarks>
    public Texture2D? Texture { get; init; }

    /// <summary>
    /// Identifiable groups the item joins
    /// </summary>
    public IReadOnlyList<string> Groups { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Component changes applied to the item's prefab
    /// </summary>
    public IReadOnlyList<ComponentEdit> Components { get; init; } = Array.Empty<ComponentEdit>();

    /// <summary>
    /// Makes the definition object, for content the game models as a subclass
    /// </summary>
    public Func<IdentifiableType>? CreateType { get; init; }

    /// <summary>
    /// The item's Slimepedia page
    /// </summary>
    /// <remarks> By default, one that says "No information" </remarks>
    public PediaInfo? Pedia { get; init; } = new();

    /// <summary>
    /// The large picture on the item's Slimepedia page
    /// </summary>
    /// <remarks> Without one the Slimepedia page shows the icon </remarks>
    public Sprite? FullArt { get; init; }

    /// <summary>
    /// The category tab the Slimepedia page is in
    /// </summary>
    /// <remarks> <see cref="PediaInfo.Category"/> overwrites it </remarks>
    public PediaCategoryKind PediaCategory { get; init; } = PediaCategoryKind.Resources;

    /// <summary>
    /// The facts panel the Slimepedia page shows
    /// </summary>
    /// <remarks> <see cref="PediaInfo.FactSet"/> overwrites it </remarks>
    public PediaFactSetKind PediaFactSet { get; init; } = PediaFactSetKind.None;

    /// <summary>
    /// What happens when something happens to one of these items
    /// </summary>
    /// <remarks> Examples: it spawns, is vacuumed in, is shot out or it is sold </remarks>
    public ItemActions? Actions { get; init; }
}

/// <summary>
/// An item created by Prism
/// </summary>
/// <param name="Type"> The identifiable type </param>
/// <param name="ReferenceId"> Its persistence id </param>
/// <param name="ObjectName"> Its object name </param>
/// <param name="Kind"> What it is (<c>Plort</c>, <c>Fruit</c>, <c>Hen</c>), or empty for a plain item </param>
public record CreatedItem(
    IdentifiableType Type,
    string ReferenceId,
    string ObjectName,
    string Kind = "");

/// <summary>
/// The item creator, used to create anything that has no explicit creator
/// </summary>
public static class PrismItemCreatorV02
{
    /// <summary>
    /// Used to copy things on to a new item from a template
    /// </summary>
    /// <param name="item"> The new item </param>
    /// <param name="template"> The template item </param>
    private static void CopyFromTemplate(IdentifiableType item, IdentifiableType template)
    {
        item.properties = template.properties;

        if (template.defaultPropertyValues != null)
            item.defaultPropertyValues = new Il2CppSystem.Collections.Generic.List<Il2CppMonomiPark.UnitPropertySystem.PropertyDefinition.PropertyDefaultOverride>(template.defaultPropertyValues.Cast<Il2CppSystem.Collections.Generic.IEnumerable<Il2CppMonomiPark.UnitPropertySystem.PropertyDefinition.PropertyDefaultOverride>>());

        if (template.Categories != null)
            item.Categories = new Il2CppSystem.Collections.Generic.List<IdentifiableCategory>(template.Categories.Cast<Il2CppSystem.Collections.Generic.IEnumerable<IdentifiableCategory>>());

        item.amountAcquiredPerVac = template.amountAcquiredPerVac;
        item.needsInstantiation   = template.needsInstantiation;
        item.groupType            = template.groupType;
        item.IsPlort              = template.IsPlort;
        item.IsAnimal             = template.IsAnimal;
        item.IsUnstable           = template.IsUnstable;
        item.IsSceneObject        = template.IsSceneObject;
        item.FashionParentOffset  = template.FashionParentOffset;
        item.gordoSnareBaitPrefab = template.gordoSnareBaitPrefab;
        item.showForZones         = template.showForZones;
    }

    /// <summary>
    /// Creates an item from an <see cref="ItemRequest"/>
    /// </summary>
    public static CreatedItem? Create(ItemRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsGameReady)
        {
            LogError($"Prism: Can't create '{request.Name}': the game isn't ready");
            return null;
        }

        if (request.Template?.prefab == null)
        {
            LogError($"Prism: Can't create '{request.Name}': the template has no prefab");
            return null;
        }

        var stripped = ContentNames.WithoutKindSuffix(request.Name, request.Kind);
        
        var name = ContentNames.Unique(stripped, request.Kind);
        var objectName = name + request.Kind;
        var referenceId = ContentNames.ReferenceId(
            "IdentifiableType", ContentNames.StableName(request.Name, request.Kind), request.Kind);

        var legacyIds = new[]
        {
            ContentNames.LegacyReferenceId("IdentifiableType", ContentNames.StableName(request.Name, request.Kind), request.Kind),
            ContentNames.LegacyReferenceId("IdentifiableType", name, request.Kind),
            ContentNames.LegacyReferenceId("IdentifiableType", ContentNames.Sanitize(request.Name), request.Kind)
        };

        var item = request.CreateType?.Invoke() ?? ScriptableObject.CreateInstance<IdentifiableType>();
        SpriteEUtil.Hold(request.Icon);
        SpriteEUtil.Hold(request.Texture);
        SpriteEUtil.Hold(request.Mesh);

        item.hideFlags     = HideFlags.DontUnloadUnusedAsset;
        item.name          = objectName;
        item.localizedName = request.DisplayName;
        item.icon          = request.Icon ?? (request.Color.HasValue && request.Texture == null
                            ? SpriteEUtil.Recolour(request.Template.icon, request.Template.color, request.Color.Value) ?? request.Template.icon
                            : request.Template.icon);
        
        PageArt.ClearFullArt(item);
        
        if (request.Icon == null && request.Color == null && request.Texture == null && request.Mesh == null && request.FullArt == null)
            PageArt.KeepFullArtFrom(item, request.Template);
        
        item.color = request.Color ?? request.Template.color;
        
        CopyFromTemplate(item, request.Template);
        
        PediaSaveIds.SetSuffix(
            item,
            ContentNames.StableName(request.Name, request.Kind),
            name,
            string.IsNullOrEmpty(request.Kind) ? "item" : request.Kind.ToLowerInvariant());

        item.prefab = Prefabs.Create(objectName + "Prefab", request.Template.prefab);

        var actor = item.prefab.GetComponent<IdentifiableActor>();
        if (actor != null) actor.identType = item;

        ApplyAppearance(item, request);
        ComponentEdit.ApplyEdits(item.prefab, request.Components);
        
        if (!SaveRegistry.Register(item, referenceId, legacyIds))
        {
            try
            {
                if (item.prefab != null) UObject.Destroy(item.prefab);
                UObject.Destroy(item);
            }
            catch { }

            return null;
        }

        ItemEvents.Register(item, request.Actions);

        Groups.Add(item, Groups.Names.AllIdentifiableTypes);
        foreach (var group in request.Groups)
            Groups.Add(item, group);
        
        Groups.CopyGroups(item, request.Template);

        PageArt.Register(item, request.FullArt);
        PrismPediaCreatorV02.CreateFor(item, request.Pedia, request.PediaCategory, request.PediaFactSet);

        return new CreatedItem(item, referenceId, objectName, request.Kind);
    }

    /// <summary>
    /// Applies the requested model and texture
    /// </summary>
    internal static void ApplyAppearance(IdentifiableType item, ItemRequest request)
    {
        if (request.Mesh != null)
            ApplyMesh(item.prefab, request.Mesh);

        if (request.Texture == null) return;
        
        MaterialTextures.ApplyToHierarchy(item.prefab, request.Texture);

        if (request.Color.HasValue) return;

        var sampled = request.Texture.AverageColor();
        if (sampled.HasValue) item.color = sampled.Value;
    }

    /// <summary>
    /// Applies a custom mesh to every model slot of a prefab & scales it to the model size
    /// </summary>
    private static void ApplyMesh(GameObject prefab, Mesh mesh)
    {
        mesh.RecalculateBounds();

        var filters = prefab.GetComponentsInChildren<MeshFilter>(includeInactive: true);

        foreach (var filter in filters)
        {
            var originalSize = filter.sharedMesh != null ? filter.sharedMesh.bounds.size.magnitude : 0f;
            filter.sharedMesh = mesh;

            if (filter.GetComponent<MeshRenderer>() == null) continue;

            var newSize = mesh.bounds.size.magnitude;
            if (originalSize <= 0.0001f || newSize <= 0.0001f) continue;

            filter.transform.localScale *= originalSize / newSize;
        }
    }
}