using System;
using System.Linq;
using UObject = UnityEngine.Object;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.Economy;
using Starlight.Gadgets;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Patches;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Lib;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

/// <summary>
/// A gadget menu category tab
/// </summary>
public record GadgetCategorySpec
{
    /// <summary>
    /// Internal name of the tab
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The title of the tab
    /// </summary>
    /// <remarks> Display name </remarks>
    public LocalizedString? Title { get; init; }

    /// <summary>
    /// The tab's icon
    /// </summary>
    /// <remarks> Null to keep the copied tab's icon  </remarks>
    public Sprite? Icon { get; init; }
}

/// <summary>
/// The cost to craft a gadget
/// </summary>
public record GadgetCost
{
    /// <summary>
    /// Newbucks
    /// </summary>
    public int Newbucks { get; init; }

    /// <summary>
    /// The items it costs
    /// </summary>
    /// <remarks> Together with Newbucks, at most <see cref="PrismGadgetCreatorV02.MaxCraftingIngredients"/> possible </remarks>
    public IReadOnlyList<(IdentifiableType Item, int Amount)> Items { get; init; } = Array.Empty<(IdentifiableType, int)>();
}

/// <summary>
/// A request to create one gadget
/// </summary>
public record GadgetRequest
{
    /// <summary>
    /// The internal name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The display name
    /// </summary>
    public required LocalizedString DisplayName { get; init; }

    /// <summary>
    /// The gadget's template;
    /// A teleporter, warp depot, cannon, linked cannon, etc.
    /// </summary>
    public required GadgetDefinition Template { get; init; }

    /// <summary>
    /// The description in the gadget menu and the Slimepedia
    /// </summary>
    /// <remarks> Null = the template's description </remarks>
    public LocalizedString? Description { get; init; }

    /// <summary>
    /// The gadget's icon
    /// </summary>
    /// <remarks> Null = the template's icon </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// Its color
    /// </summary>
    /// <remarks> Null = the template's color </remarks>
    public Color? Color { get; init; }

    /// <summary>
    /// How the colors of the template's icon and full art change
    /// </summary>
    /// <remarks> Null keeps the template's icon and art as they are; ignored when there is an <see cref="Icon"/> specified </remarks>
    public ColorShift? ArtShift { get; init; }

    /// <summary>
    /// Color properties set on the model's materials, such as the slots of a palette shader
    /// </summary>
    /// <remarks> Only materials that have one of the properties are copied and changed </remarks>
    public IReadOnlyDictionary<string, Color>? MaterialColors { get; init; }

    /// <summary>
    /// A custom model
    /// </summary>
    /// <remarks> Null = the template's model </remarks>
    public Mesh? Mesh { get; init; }

    /// <summary>
    /// A custom texture
    /// </summary>
    /// <remarks> Null = the template's texture </remarks>
    public Texture2D? Texture { get; init; }

    /// <summary>
    /// The cost of crafting this gadget
    /// </summary>
    public GadgetCost? Cost { get; init; }

    /// <summary>
    /// What unlocking the blueprint costs
    /// </summary>
    public int? BlueprintCost { get; init; }

    /// <summary>
    /// The most of these gadgets that can be bought
    /// </summary>
    /// <remarks> Null = the template's limit </remarks>
    public int? BuyCountLimit { get; init; }

    /// <summary>
    /// The most that can exist in the world at once
    /// </summary>
    /// <remarks> Null = the template's limit </remarks>
    public int? CountLimit { get; init; }

    /// <summary>
    /// Name of a tab of the game's gadget menu to list it under
    /// </summary>
    /// <remarks> Overwritten by <see cref="NewCategory"/> </remarks>
    public string? Category { get; init; }

    /// <summary>
    /// A new category tab it is listed under
    /// </summary>
    /// <remarks> Overwrites <see cref="Category"/> </remarks>
    public GadgetCategorySpec? NewCategory { get; init; }

    /// <summary>
    /// The Slimepedia entry it's blueprint unlocks
    /// </summary>
    /// <remarks> Null = <see cref="PediaPlaceholder"/> </remarks>
    public PediaEntry? PediaEntry { get; init; }

    /// <summary>
    /// The text for a placeholder entry
    /// </summary>
    /// <remarks> Without this or a <see cref="PediaEntry"/>, the template's Slimepedia entry is shared </remarks>
    public LocalizedString? PediaPlaceholder { get; init; }

    /// <summary>
    /// Its Slimepedia page
    /// </summary>
    /// <remarks> By default, one that says "No information" </remarks>
    public PediaInfo? Pedia { get; init; } = new();

    /// <summary>
    /// Whether the blueprint starts unlocked once the game is running
    /// </summary>
    // public bool StartsUnlocked { get; init; }
}

/// <summary>
/// A gadget created by Prism
/// </summary>
/// <param name="Definition"> The gadget definition </param>
/// <param name="ReferenceId"> Its persistence id </param>
/// <param name="ObjectName"> Its object name </param>
/// <param name="Category"> The group of the menu tab it is listed under, or null if it isn't in one </param>
public record CreatedGadget(GadgetDefinition Definition, string ReferenceId, string ObjectName, IdentifiableTypeGroup? Category);

/// <summary>
/// The gadget creator, used to create gadgets
/// </summary>
public static class PrismGadgetCreatorV02
{
    /// <summary>
    /// The suffix every gadget's name carries
    /// </summary>
    private const string Kind = "Gadget";

    /// <summary>
    /// The most ingredients the Fabricator's recipe panel fits, newbucks included
    /// </summary>
    /// <remarks> If you add more, the gadget cannot be crafted </remarks>
    public const int MaxCraftingIngredients = 6;

    /// <summary>
    /// Every created gadget
    /// </summary>
    private static readonly HashSet<IntPtr> CreatedGadgets = new();

    /// <summary>
    /// The template gadget for each created gadget
    /// </summary>
    private static readonly List<(GadgetDefinition Gadget, GadgetDefinition Template)> Templates = new();

    /// <summary>
    /// Whether Prism made this gadget
    /// </summary>
    public static bool IsCreated(GadgetDefinition? gadget) => gadget != null && CreatedGadgets.Contains(gadget.Pointer);

    /// <summary>
    /// Adds all created gadgets to their template's groups
    /// </summary>
    internal static void JoinGroups()
    {
        foreach (var (gadget, template) in Templates)
            if (gadget != null && template != null)
                Groups.JoinAsTemplate(gadget, template);
    }

    /// <summary>
    /// Category tabs created by the Gadget creator
    /// </summary>
    private static readonly Dictionary<string, IdentifiableTypeGroup> CreatedCategories = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a gadget from a <see cref="GadgetRequest"/>
    /// </summary>
    public static CreatedGadget? Create(GadgetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsGameReady)
        {
            LogError($"Prism: Can't create '{request.Name}': the game isn't ready");
            return null;
        }

        var template = request.Template;
        if (template == null || template.prefab == null)
        {
            LogError($"Prism: Can't create gadget '{request.Name}': the template has no prefab");
            return null;
        }

        var stripped = ContentNames.WithoutKindSuffix(request.Name, Kind);
        var name = ContentNames.Unique(stripped, Kind);
        var objectName = name + Kind;
        var referenceId = ContentNames.ReferenceId(
            "GadgetDefinition", ContentNames.StableName(request.Name, Kind), Kind);

        var gadget = UObject.Instantiate(template);
        gadget.hideFlags     = HideFlags.DontUnloadUnusedAsset;
        gadget.name          = objectName;
        gadget.localizedName = request.DisplayName;
        SpriteEUtil.Hold(request.Icon);
        PageArt.ClearFullArt(gadget);
        Recolor(gadget, template, request);
        gadget.color = request.Color ?? template.color;
        PediaSaveIds.SetSuffix(gadget, ContentNames.StableName(request.Name, Kind), name, "gadget");

        gadget.VariantOf = null;
        gadget.LinkedDefinition = null;

        if (request.Description != null) gadget._localizedDescription = request.Description;
        if (request.BlueprintCost.HasValue) gadget.BlueprintCost = request.BlueprintCost.Value;
        if (request.BuyCountLimit.HasValue) gadget.BuyCountLimit = request.BuyCountLimit.Value;
        if (request.CountLimit.HasValue) gadget.CountLimit = request.CountLimit.Value;
        if (request.Cost != null) gadget.CraftingCosts = BuildCost(request.Cost, objectName);

        gadget.CountOtherDefinitions = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<GadgetDefinition>(0);

        gadget.prefab = Prefabs.Create(objectName + "Prefab", template.prefab);

        foreach (var identifiable in gadget.prefab.GetComponentsInChildren<Identifiable>(includeInactive: true))
            identifiable.identType = gadget;
        Prefabs.Retarget(gadget.prefab, template, gadget);
        ApplyAppearance(gadget, request);

        var legacyId = ContentNames.LegacyReferenceId("GadgetDefinition", ContentNames.StableName(request.Name, Kind), Kind);

        if (!SaveRegistry.Register(gadget, referenceId, legacyId))
        {
            try
            {
                if (gadget.prefab != null) UObject.Destroy(gadget.prefab);
                UObject.Destroy(gadget);
            }
            catch { }

            return null;
        }

        Groups.Add(gadget, Groups.Names.AllIdentifiableTypes);
        Groups.CopyGroups(gadget, template);

        var category = List(gadget, request);

        Document(gadget, request);

        CreatedGadgets.Add(gadget.Pointer);
        Templates.Add((gadget, template));

        return new CreatedGadget(gadget, referenceId, objectName, category);
    }

    /// <summary>
    /// Creates two gadgets that are placed together and work as a pair from 2 <see cref="GadgetRequest"/>s
    /// </summary>
    /// <param name="primary"> The gadget the player crafts </param>
    /// <param name="secondary"> The primary gadget's partner </param>
    /// <returns> The pair, or null </returns>
    public static (CreatedGadget Primary, CreatedGadget Secondary)? CreatePair(GadgetRequest primary, GadgetRequest secondary)
    {
        ArgumentNullException.ThrowIfNull(primary);
        ArgumentNullException.ThrowIfNull(secondary);

        var first = Create(primary);
        if (first == null) return null;

        var second = Create(secondary);
        if (second == null)
        {
            LogError($"Prism: '{first.ObjectName}' was created but its partner '{secondary.Name}' wasn't, so it won't be placeable as a pair");
            return null;
        }

        first.Definition.LinkedDefinition = second.Definition;
        first.Definition.IsSecondary = false;
        second.Definition.LinkedDefinition = first.Definition;
        second.Definition.IsSecondary = true;

        return (first, second);
    }

    /// <summary>
    /// Builds what a gadget costs
    /// </summary>
    private static PurchaseCost BuildCost(GadgetCost cost, string gadgetName)
    {
        var currencies = new Il2CppSystem.Collections.Generic.List<CurrencyCostEntry>();
        if (cost.Newbucks > 0) currencies.Add(CurrencyCostEntry.CreateNewbucks(cost.Newbucks));

        var items = new Il2CppSystem.Collections.Generic.List<IdentCostEntry>();
        foreach (var (item, amount) in cost.Items)
        {
            if (item == null || amount <= 0) continue;
            items.Add(new IdentCostEntry(item, amount));
        }

        var ingredients = currencies.Count + items.Count;
        if (ingredients > MaxCraftingIngredients)
            LogWarning($"Prism: '{gadgetName}' costs {ingredients} ingredients, but the Fabricator only fits {MaxCraftingIngredients}, so it can't be crafted");

        return new PurchaseCost(currencies, items, new Il2CppSystem.Collections.Generic.List<Il2CppMonomiPark.SlimeRancher.Player.Component.UpgradeComponent>());
    }

    /// <summary>
    /// Shifts the colors of the template's icon and full art
    /// </summary>
    private static void Recolor(GadgetDefinition gadget, GadgetDefinition template, GadgetRequest request)
    {
        gadget.icon = request.Icon ?? template.icon;

        if (request.Icon != null || request.ArtShift is not { } shift) return;

        gadget.icon = template.icon.Recolor(shift) ?? template.icon;
        
        PageArt.RegisterRecolor(gadget, template.FullArtReference, shift);
    }

    /// <summary>
    /// Applies the colors, model and texture to the gadget's prefab
    /// </summary>
    private static void ApplyAppearance(GadgetDefinition gadget, GadgetRequest request)
    {
        if (request.MaterialColors is { Count: > 0 } colors)
            MaterialColors.ApplyToHierarchy(gadget.prefab, colors);

        if (request.Mesh == null && request.Texture == null) return;
        
        if (request.Mesh != null)
        {
            request.Mesh.RecalculateBounds();

            foreach (var filter in gadget.prefab.GetComponentsInChildren<MeshFilter>(includeInactive: true))
            {
                var originalSize = filter.sharedMesh != null ? filter.sharedMesh.bounds.size.magnitude : 0f;
                filter.sharedMesh = request.Mesh;

                var newSize = request.Mesh.bounds.size.magnitude;
                if (originalSize <= 0.0001f || newSize <= 0.0001f) continue;

                filter.transform.localScale *= originalSize / newSize;
            }
        }

        if (request.Texture != null)
        {
            MaterialTextures.ApplyToHierarchy(gadget.prefab, request.Texture);

            if (!request.Color.HasValue)
            {
                var sampled = request.Texture.AverageColor();
                if (sampled.HasValue) gadget.color = sampled.Value;
            }
        }
    }

    /// <summary>
    /// Tabs waiting for the gadget menu to load
    /// </summary>
    private static readonly List<(GadgetCategorySpec Spec, IdentifiableTypeGroup Group)> PendingTabs = new();

    /// <summary>
    /// Gadgets waiting to be listed in one of the game's own tabs
    /// </summary>
    private static readonly List<(GadgetDefinition Gadget, string Tab, IdentifiableTypeGroup? Group)> PendingListings = new();

    private static bool hooked;

    private static bool MenuReady => GadgetCategories.FindConfiguration()?.Categories != null;

    /// <summary>
    /// Lists the gadget under their respective menu tab
    /// </summary>
    /// <returns> The group of the tab it is under, or null if it isn't under one yet </returns>
    private static IdentifiableTypeGroup? List(GadgetDefinition gadget, GadgetRequest request)
    {
        Hook();

        IdentifiableTypeGroup? target = null;

        if (request.NewCategory != null)
        {
            target = EnsureCategory(request.NewCategory);
            
            if (target != null && !MenuReady)
            {
                GadgetCategories.AddMember(target, gadget);
                PendingListings.Add((gadget, string.Empty, target));
                return target;
            }
        }
        else if (!string.IsNullOrEmpty(request.Category))
        {
            target = MenuReady ? NativeGadgets.CategoryGroup(request.Category) : null;

            if (target == null)
            {
                PendingListings.Add((gadget, request.Category, null));
                return null;
            }
        }

        if (target != null) Move(gadget, target);

        return target;
    }

    private static void Move(GadgetDefinition gadget, IdentifiableTypeGroup target)
    {
        foreach (var other in NativeGadgets.CategoryGroups())
            if (other.Pointer != target.Pointer) Groups.Remove(gadget, other);

        GadgetCategories.AddMember(target, gadget);
        
        var gadgets = Groups.Find(Groups.Names.GadgetGroupName);
        if (gadgets != null) Groups.AddChild(gadgets, target);
    }

    private static void Hook()
    {
        if (hooked) return;
        hooked = true;

        GadgetCategoryStrip.Building += Flush;
    }

    /// <summary>
    /// Adds the tabs and listings to the gadget menu
    /// </summary>
    public static void Flush()
    {
        if (!MenuReady) return;

        foreach (var (spec, group) in PendingTabs.ToList())
        {
            if (GadgetCategories.Add(ContentNames.Sanitize(spec.Name), spec.Title, group, spec.Icon) != null)
                PendingTabs.Remove((spec, group));
        }

        foreach (var listing in PendingListings.ToList())
        {
            PendingListings.Remove(listing);

            var group = listing.Group ?? NativeGadgets.CategoryGroup(listing.Tab);
            if (group == null)
            {
                LogWarning($"Prism: '{listing.Gadget.name}' wasn't listed: the gadget menu has no tab '{listing.Tab}'");
                continue;
            }

            Move(listing.Gadget, group);
        }
    }

    private static IdentifiableTypeGroup? EnsureCategory(GadgetCategorySpec spec)
    {
        if (CreatedCategories.TryGetValue(spec.Name, out var existing) && existing != null) return existing;

        var group = GadgetCategories.CreateGroup(ContentNames.Sanitize(spec.Name) + "Group");
        if (group == null) return null;

        CreatedCategories[spec.Name] = group;

        PendingTabs.Add((spec, group));
        Flush();

        return group;
    }

    /// <summary>
    /// Gives the blueprint something to unlock
    /// </summary>
    private static void Document(GadgetDefinition gadget, GadgetRequest request)
    {
        PediaEntry? entry = request.PediaEntry;

        if (entry == null)
        {
            var info = request.Pedia != null && request.PediaPlaceholder != null ? request.Pedia with { Description = request.PediaPlaceholder } : request.Pedia;
            entry = PrismPediaCreatorV02.CreateFor(gadget, info, PediaCategoryKind.Blueprints, PediaFactSetKind.None);
        }

        if (entry != null) gadget._pediaLink = entry;
    }
}