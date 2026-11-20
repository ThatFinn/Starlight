using System;
using System.Linq;
using UObject = UnityEngine.Object;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.Ranch;
using Starlight.Prism.Lib;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

/// <summary>
/// The three shades the vac's body shader paints one colour channel of its mask with
/// </summary>
/// <param name="Dark"> The darkest shade </param>
/// <param name="Light"> The lightest shade </param>
/// <param name="Spec"> Its highlight; alpha is the smoothness </param>
public readonly record struct VacPaletteColor(Color Dark, Color Light, Color Spec);

/// <summary>
/// A request to create a vac palette
/// </summary>
public record VacPaletteRequest
{
    /// <summary>
    /// The name of the vac palette
    /// </summary>
    /// <remarks> Identifier </remarks>
    public required string Name { get; init; }

    /// <summary>
    /// The display name
    /// </summary>
    public required LocalizedString DisplayName { get; init; }

    /// <summary>
    /// The Slimepedia description of it
    /// </summary>
    public required LocalizedString Description { get; init; }

    /// <summary>
    /// The palette to copy
    /// </summary>
    /// <remarks> Null = the vac's default palette </remarks>
    public VacPalette? Template { get; init; }

    /// <summary>
    /// Its icon in the Slimepedia and the palette picker
    /// </summary>
    /// <remarks> Null = the template's icon </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// The material for the vac's body
    /// </summary>
    /// <remarks> Null = the template's body material </remarks>
    public Material? BodyMaterial { get; init; }

    /// <summary>
    /// The material for the vac's glass
    /// </summary>
    /// <remarks> Null = the template's glass material </remarks>
    public Material? GlassMaterial { get; init; }

    /// <summary>
    /// Attachments to add to the vac's body
    /// </summary>
    /// <remarks> Null = the template's body attachments </remarks>
    public GameObject? BodyAttachment { get; init; }

    /// <summary>
    /// Attachments to add to the vac's handle
    /// </summary>
    /// <remarks> Null = the template's handle attachments </remarks>
    public GameObject? HandleAttachment { get; init; }

    /// <summary>
    /// Whether the template's models are left off
    /// </summary>
    /// <remarks> The attachments are still there </remarks>
    public bool WithoutTemplateAttachments { get; init; }

    public VacPaletteColor? Red { get; init; }
    public VacPaletteColor? Green { get; init; }
    public VacPaletteColor? Blue { get; init; }
    public VacPaletteColor? Black { get; init; }
    public VacPaletteColor? Magenta { get; init; }
    public VacPaletteColor? Yellow { get; init; }
    public VacPaletteColor? Cyan { get; init; }
    public VacPaletteColor? White { get; init; }
}

/// <summary>
/// A vac palette Prism created
/// </summary>
public record CreatedVacPalette(VacPalette Palette, PediaEntry PediaEntry, string ReferenceId, string ObjectName);

/// <summary>
/// The Vac Palette creator, used to create vac palettes
/// </summary>
public static class PrismVacPaletteCreatorV02
{
    private const string Kind = "VacPalette";

    // What the vac's parts are called
    private const string BodySlot   = "VacBodySlot";
    private const string GlassSlot  = "VacGlassSlot";
    private const string HandleSlot = "VacHandleSlot";

    // The colour mask's channels
    private static readonly string[] Channels = { "Red", "Green", "Blue", "Black", "Magenta", "Yellow", "Cyan", "White" };

    private static readonly Dictionary<string, VacPalette> Created = new(StringComparer.Ordinal);

    private static readonly Dictionary<string, string> LegacyGuids = new(StringComparer.Ordinal);

    private static bool IsCreated(VacPalette? palette) =>
        palette != null && Created.Values.Any(created => created.Pointer == palette.Pointer);

    /// <summary>
    /// The palettes of the base game
    /// </summary>
    public static IEnumerable<VacPalette> Native
    {
        get
        {
            var listed = (lookupDirector._configuration?.AllPalettes?.items?.ToArray() ?? Array.Empty<Palette>())
                .Select(palette => palette?.TryCast<VacPalette>())
                .Where(palette => palette != null && !IsCreated(palette))
                .ToList();
            
            if (listed.Count > 0) return listed!;

            return GetAll<VacPalette>().Where(palette => palette != null && !IsCreated(palette)).ToList();
        }
    }

    /// <summary>
    /// The default vac palette
    /// </summary>
    public static VacPalette? Default =>
        GetAny<VacPaletteTarget>()?.DefaultVacPalette ?? Native.FirstOrDefault(palette => palette.name == "Default");

    /// <summary>
    /// Creates a vac palette from a <see cref="VacPaletteRequest"/>
    /// </summary>
    public static CreatedVacPalette? Create(VacPaletteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsGameReady)
        {
            LogError($"Prism: Can't create vac palette '{request.Name}': the game isn't ready");
            return null;
        }

        var template = request.Template ?? Default ?? Native.FirstOrDefault(candidate => candidate.PediaEntry != null);
        if (template?.PediaEntry == null)
        {
            LogError($"Prism: Can't create vac palette '{request.Name}': the game has no vac palette with a template pedia entry");
            return null;
        }

        var stripped = ContentNames.WithoutKindSuffix(request.Name, Kind);
        var name = ContentNames.Unique(stripped, Kind);
        var objectName = name + Kind;
        var guid = ContentNames.ReferenceId(string.Empty, ContentNames.StableName(request.Name, Kind), Kind).TrimStart('.');

        if (Created.ContainsKey(guid))
        {
            LogError($"Prism: Can't create vac palette '{objectName}' as '{guid}': another palette already has that id");
            return null;
        }

        var entry = BuildEntry(template.PediaEntry, request, name, objectName);
        if (entry == null) return null;

        var palette = UObject.Instantiate(template);
        palette.hideFlags = HideFlags.DontUnloadUnusedAsset;
        palette.name = objectName;
        
        palette.Guid = guid;
        palette._pediaEntry = entry;
        palette._materialSlots = BuildSlots(template, request, objectName);

        Created[guid] = palette;
        LegacyGuids[guid] = ContentNames.LegacyReferenceId(string.Empty, ContentNames.StableName(request.Name, Kind), Kind).TrimStart('.');
        Register(palette, template.PediaEntry);

        return new CreatedVacPalette(palette, entry, palette.ReferenceId, objectName);
    }

    /// <summary>
    /// Re-registers all vac palettes
    /// </summary>
    internal static void Refresh()
    {
        foreach (var palette in Created.Values.ToList())
            Register(palette, null);
    }

    private static PediaEntry? BuildEntry(PediaEntry templateEntry, VacPaletteRequest request, string name, string objectName)
    {
        var entry = UObject.Instantiate(templateEntry);
        entry.hideFlags    = HideFlags.DontUnloadUnusedAsset;
        entry.name         = objectName;
        entry._title       = request.DisplayName;
        entry._description = request.Description;

        var fixedEntry = entry.TryCast<FixedPediaEntry>();
        if (fixedEntry == null)
        {
            LogError($"Prism: Can't create vac palette '{objectName}': the template's pedia entry is a {templateEntry.GetType().Name}, which has no persistence id to change");
            UObject.Destroy(entry);
            return null;
        }

        var templatePrefix = fixedEntry._persistenceId;
        var stableName = ContentNames.StableName(request.Name, Kind);
        var kind = Kind.ToLowerInvariant();

        fixedEntry._persistenceId = PediaSaveIds.Prefix;
        fixedEntry._persistenceSuffix = ContentNames.PediaPersistenceSuffix(stableName, kind);

        fixedEntry._fullArt = null;
        if (request.Icon != null) fixedEntry._icon = request.Icon;

        PediaSaveIds.Register(entry, PediaSaveIds.Combine(new[] { templatePrefix }, PediaSaveIds.LegacySuffixesOf(stableName, name, kind)));

        return entry;
    }

    private static Il2CppReferenceArray<VacPaletteSlotMaterial> BuildSlots(VacPalette template, VacPaletteRequest request, string objectName)
    {
        var slots = template._materialSlots?.ToArray()
            .Where(slot => slot != null)
            .Select(slot => slot.Clone())
            .ToList() ?? new List<VacPaletteSlotMaterial>();

        if (request.WithoutTemplateAttachments)
            foreach (var slot in slots) slot.Attachment = null;

        if (request.BodyMaterial != null || request.BodyAttachment != null || ColorsOf(request).Any(color => color.HasValue))
        {
            var body = SlotFor(slots, BodySlot);
            if (body == null)
                LogWarning($"Prism: Vac palette '{objectName}': the game has no vac body slot, so its body is left as the template's");
            else
            {
                if (request.BodyMaterial != null) body.Material = request.BodyMaterial;
                if (request.BodyAttachment != null) body.Attachment = request.BodyAttachment;
                body.Material = Painted(body.Material, request, objectName);
            }
        }

        if (request.GlassMaterial != null)
        {
            var glass = SlotFor(slots, GlassSlot);
            if (glass == null) LogWarning($"Prism: Vac palette '{objectName}': the game has no vac glass slot, so its glass is left as the template's");
            else glass.Material = request.GlassMaterial;
        }

        if (request.HandleAttachment != null)
        {
            var handle = SlotFor(slots, HandleSlot);
            if (handle == null) LogWarning($"Prism: Vac palette '{objectName}': the game has no vac handle slot, so the handle gets nothing");
            else handle.Attachment = request.HandleAttachment;
        }

        return new Il2CppReferenceArray<VacPaletteSlotMaterial>(slots.ToArray());
    }

    /// <summary>
    /// The palette's slot for a part of the vac
    /// </summary>
    private static VacPaletteSlotMaterial? SlotFor(List<VacPaletteSlotMaterial> slots, string slotName)
    {
        var slot = slots.FirstOrDefault(candidate => candidate.Slot != null && candidate.Slot.name == slotName);
        if (slot != null) return slot;

        var identifier = Native
            .SelectMany(palette => palette._materialSlots?.ToArray() ?? Array.Empty<VacPaletteSlotMaterial>())
            .Select(candidate => candidate?.Slot)
            .FirstOrDefault(candidate => candidate != null && candidate.name == slotName);
        if (identifier == null) return null;

        slot = new VacPaletteSlotMaterial { Slot = identifier };
        slots.Add(slot);
        return slot;
    }

    private static VacPaletteColor?[] ColorsOf(VacPaletteRequest request) =>
        new[] { request.Red, request.Green, request.Blue, request.Black, request.Magenta, request.Yellow, request.Cyan, request.White };

    /// <summary>
    /// A copy of the material with adjusted colours
    /// </summary>
    private static Material? Painted(Material? material, VacPaletteRequest request, string objectName)
    {
        var colors = ColorsOf(request);
        if (!colors.Any(color => color.HasValue)) return material;

        if (material == null)
        {
            LogWarning($"Prism: Vac palette '{objectName}': the body has no material to paint, so its colours are left out");
            return null;
        }

        var painted = new Material(material) { name = objectName, hideFlags = HideFlags.DontUnloadUnusedAsset };

        for (var channel = 0; channel < colors.Length; channel++)
        {
            if (colors[channel] is not { } color) continue;
            
            if (!painted.HasProperty($"_Color{channel}0"))
            {
                LogWarning($"Prism: Vac palette '{objectName}': the body material's shader has no {Channels[channel]} channel, so that colour is left out");
                continue;
            }

            painted.SetColor($"_Color{channel}0", color.Dark);
            painted.SetColor($"_Color{channel}1", color.Light);
            painted.SetColor($"_Color{channel}2", color.Spec);
        }

        return painted;
    }

    private static void Register(VacPalette palette, PediaEntry? templateEntry)
    {
        try
        {
            var list = lookupDirector._configuration?.AllPalettes;
            if (list != null && !list.items.Contains(palette))
                list.items.Add(palette);

            SaveTables.Register(SaveTables.Translation?._paletteTranslation, palette.ReferenceId, palette);

            if (LegacyGuids.TryGetValue(palette.ReferenceId, out var legacyGuid))
                SaveTables.RegisterLegacy(SaveTables.Translation?._paletteTranslation, legacyGuid, palette);

            if (templateEntry != null) AddToCategories(palette.PediaEntry, templateEntry);
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to register vac palette '{palette.name}'", ex);
        }
    }

    /// <summary>
    /// Adds the Vac Palette to the Slimepedia
    /// </summary>
    private static void AddToCategories(PediaEntry entry, PediaEntry templateEntry)
    {
        foreach (var category in GetAll<PediaCategory>())
        {
            if (!category.Contains(templateEntry)) continue;

            if (!category._items.Contains(entry))
                category._items = category._items.AddToNew(entry);

            category.GetTypedRuntimeCategory()?.AddDynamicItem(entry);
        }
    }
}