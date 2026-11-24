using System;
using System.Linq;
using UObject = UnityEngine.Object;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Lib;
using UnityEngine.Localization;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Creators;

/// <summary>
/// Which category of the Slimepedia this is
/// </summary>
public enum PediaCategoryKind
{
    Slimes,
    RadiantSlimes,
    Resources,
    World,
    Ranch,
    Science,
    Toys,
    Weather,
    Blueprints,
    Tutorial,
}

/// <summary>
/// Which section of a Slimepedia entry this is
/// </summary>
public enum PediaSectionKind
{
    Slimeology,
    RancherRisks,
    Plortonomics,
    About,
    OnTheRanch,
    Instructions,
}

/// <summary>
/// Which factset this is
/// </summary>
public enum PediaFactSetKind
{
    None,
    Slime,
    Resource,
    Food,
}

/// <summary>
/// A Slimepedia section entry
/// </summary>
public readonly record struct PediaSection(PediaSectionKind Kind, LocalizedString Text);

/// <summary>
/// A request to create an ordinary Slimepedia entry
/// </summary>
public sealed class PediaRequest
{
    /// <summary>
    /// The created thing the entry is for
    /// </summary>
    /// <remarks> Its display name becomes the entry's title </remarks>
    public required IdentifiableType Subject { get; init; }

    /// <summary>
    /// The category the entry is in
    /// </summary>
    public required PediaCategoryKind Category { get; init; }

    /// <summary>
    /// The opening description
    /// </summary>
    public required LocalizedString Description { get; init; }

    /// <summary>
    /// Which facts panel to show
    /// </summary>
    public PediaFactSetKind FactSet { get; init; } = PediaFactSetKind.None;

    /// <summary>
    /// The entry's sections, in display order
    /// </summary>
    public IReadOnlyList<PediaSection> Sections { get; init; } = Array.Empty<PediaSection>();

    /// <summary>
    /// Runs when the player unlocks the entry
    /// </summary>
    public Action<PediaUnlock>? CustomUnlockAction { get; init; }

    /// <summary>
    /// Whether other mods can recolor the art of the created thing
    /// </summary>
    public bool CanBeRecolored { get; init; } = true;
}

/// <summary>
/// What the Slimepedia page of a created thing says
/// </summary>
public sealed record PediaInfo
{
    /// <summary>
    /// Whether other mods can recolor the art of the created thing
    /// </summary>
    public bool CanBeRecolored { get; init; } = true;

    /// <summary>
    /// The opening description
    /// </summary>
    /// <remarks> Null = "No information" </remarks>
    public LocalizedString? Description { get; init; }

    /// <summary>
    /// The entry's sections, in display order
    /// </summary>
    public IReadOnlyList<PediaSection> Sections { get; init; } = Array.Empty<PediaSection>();

    /// <summary>
    /// Which facts panel to show
    /// </summary>
    public PediaFactSetKind? FactSet { get; init; }

    /// <summary>
    /// The category the entry is in
    /// </summary>
    public PediaCategoryKind? Category { get; init; }

    /// <summary>
    /// Runs when the entry is unlocked
    /// </summary>
    public Action<PediaUnlock>? CustomUnlockAction { get; init; }
}

/// <summary>
/// A request for a Slimepedia page that documents no item
/// </summary>
/// <remarks> Examples: the world, ranch, science, weather or tutorial Slimepedia page </remarks>
public sealed class FixedPediaRequest
{
    /// <summary>
    /// The raw identifier
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The tab the page is filed in
    /// </summary>
    public required PediaCategoryKind Category { get; init; }

    /// <summary>
    /// The title of the entry
    /// </summary>
    /// <remarks> Display name </remarks>
    public required LocalizedString Title { get; init; }

    /// <summary>
    /// The opening description
    /// </summary>
    public required LocalizedString Description { get; init; }

    /// <summary>
    /// The Slimepedia page's icon
    /// </summary>
    /// <remarks> Null = template's icon </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// Which facts panel to show
    /// </summary>
    /// <remarks> None = template's factset </remarks>
    public PediaFactSetKind FactSet { get; init; } = PediaFactSetKind.None;

    /// <summary>
    /// The page's sections, in display order
    /// </summary>
    public IReadOnlyList<PediaSection> Sections { get; init; } = Array.Empty<PediaSection>();

    /// <summary>
    /// Runs when the player unlocks the page
    /// </summary>
    public Action<PediaUnlock>? CustomUnlockAction { get; init; }

    /// <summary>
    /// Whether other mods can recolor the page's art
    /// </summary>
    public bool CanBeRecolored { get; init; } = true;
}

/// <summary>
/// A request for a radiant slime Slimepedia entry
/// </summary>
public sealed class RadiantPediaRequest
{
    /// <summary>
    /// The slime this is the radiant Slimepedia entry of
    /// </summary>
    public required SlimeDefinition Subject { get; init; }

    /// <summary>
    /// The opening description
    /// </summary>
    public required LocalizedString Description { get; init; }

    /// <summary>
    /// Which facts panel to show
    /// </summary>
    public PediaFactSetKind FactSet { get; init; } = PediaFactSetKind.Slime;

    /// <summary>
    /// The entry's sections, in display order
    /// </summary>
    public IReadOnlyList<PediaSection> Sections { get; init; } = Array.Empty<PediaSection>();

    /// <summary>
    /// Runs when the player unlocks the entry
    /// </summary>
    public Action<PediaUnlock>? CustomUnlockAction { get; init; }

    /// <summary>
    /// Whether other mods can recolor the page's art
    /// </summary>
    public bool CanBeRecolored { get; init; } = true;
}

/// <summary>
/// The Slimepedia creator, used to create Slimepedia entries
/// </summary>
public static class PrismPediaCreatorV02
{
    /// <summary>
    /// Created radiant slime entries
    /// </summary>
    internal static readonly List<RadiantSlimePediaEntry> RadiantEntries = new();

    /// <summary>
    /// Created entries by their category
    /// </summary>
    internal static readonly Dictionary<PediaCategoryKind, List<PediaEntry>> PendingEntries = new();

    /// <summary>
    /// How many entries are pending
    /// </summary>
    public static int PendingCount
    {
        get
        {
            var total = 0;

            foreach (var entries in PendingEntries.Values) total += entries.Count;

            return total;
        }
    }

    /// <summary>
    /// Already registered content
    /// </summary>
    private static readonly HashSet<string> Documented = new(StringComparer.Ordinal);

    /// <summary>
    /// A custom credit line
    /// </summary>
    // private static LocalizedString? credit;

    /// <summary>
    /// The credit line of each mod by the mod's name
    /// </summary>
    private static readonly Dictionary<string, LocalizedString> Credits = new(StringComparer.Ordinal);

    /// <summary>
    /// A line crediting the entry for which mod added it
    /// </summary>
    /// <remarks> Added to the bottom of every entry </remarks>
    public static LocalizedString? Credit
    {
        get
        {
            // if (credit != null) return credit;

            var mod = ContentNames.SourceModName;
            if (string.IsNullOrWhiteSpace(mod)) return null;

            if (!Credits.TryGetValue(mod, out var line))
            {
                line = AddTranslation(Tr("prism.pedia.addedby", mod));
                Credits[mod] = line;
            }

            return line;
        }
        // set => credit = value;
    }

    /// <summary>
    /// Every created entry
    /// </summary>
    public static IEnumerable<PediaEntry> CreatedEntries => PendingEntries.Values.SelectMany(entries => entries);

    /// <summary>
    /// Whether this content already has a pedia entry
    /// </summary>
    public static bool HasEntry(IdentifiableType subject) =>
        subject != null && Documented.Contains(subject.name);

    /// <summary>
    /// Creates a Slimepedia entry for content that has none
    /// </summary>
    /// <param name="subject"> The content to document </param>
    /// <param name="category"> The tab to add the entry to </param>
    /// <param name="factSet"> Which facts panel to show </param>
    /// <param name="placeholder"> The description </param>
    /// <returns> The entry (new or existing) </returns>
    public static PediaEntry? EnsureEntry(
        IdentifiableType subject,
        PediaCategoryKind category,
        PediaFactSetKind factSet,
        LocalizedString placeholder)
    {
        ArgumentNullException.ThrowIfNull(subject);

        if (HasEntry(subject)) return Existing(subject);

        return Create(new PediaRequest
        {
            Subject     = subject,
            Category    = category,
            Description = placeholder,
            FactSet     = factSet,
            Sections    = Array.Empty<PediaSection>()
        });
    }

    /// <summary>
    /// The "No Information" text entry
    /// </summary>
    private static LocalizedString NoInformation => noInformation ??= AddTranslation(Tr("prism.pedia.noinformation"));

    private static LocalizedString? noInformation;

    /// <summary>
    /// Makes the page a request's <c>Pedia</c> asks for
    /// </summary>
    /// <remarks> Makes none if <paramref name="info"/> is null or the pedia is switched off </remarks>
    /// <param name="subject"> The content to document </param>
    /// <param name="info"> What the page says, or null for no page </param>
    /// <param name="defaultCategory"> The tab to use if <paramref name="info"/> names none </param>
    /// <param name="defaultFactSet"> The facts panel to use if <paramref name="info"/> names none </param>
    /// <returns> The page that was created or null </returns>
    public static PediaEntry? CreateFor(IdentifiableType subject, PediaInfo? info, PediaCategoryKind defaultCategory, PediaFactSetKind defaultFactSet)
    {
        if (subject != null && info is { CanBeRecolored: false }) subject.SetCanBeRecolored(false);

        if (subject == null || info == null || !StarlightEntryPoint.prismEnablePedia) return null;

        try
        {
            return Create(new PediaRequest
            {
                Subject            = subject,
                Category           = info.Category ?? defaultCategory,
                Description        = DescriptionOf(info),
                FactSet            = info.FactSet ?? defaultFactSet,
                Sections           = SectionsOf(info),
                CustomUnlockAction = info.CustomUnlockAction
            });
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to create a Slimepedia page for '{subject.name}'", ex);
            return null;
        }
    }

    /// <summary>
    /// Creates a radiant slime Slimepedia entry
    /// </summary>
    /// <returns> The page that was created or null </returns>
    internal static PediaEntry? CreateRadiantFor(SlimeDefinition subject, PediaInfo? info)
    {
        if (subject == null || info == null || !StarlightEntryPoint.prismEnablePedia) return null;

        return CreateRadiant(new RadiantPediaRequest
        {
            Subject            = subject,
            Description        = DescriptionOf(info),
            FactSet            = info.FactSet ?? PediaFactSetKind.Slime,
            Sections           = SectionsOf(info),
            CustomUnlockAction = info.CustomUnlockAction,
            CanBeRecolored     = info.CanBeRecolored
        });
    }

    /// <summary>
    /// The opening description of a ^Slimepedia page
    /// </summary>
    private static LocalizedString DescriptionOf(PediaInfo info) => info.Description ?? NoInformation;

    /// <summary>
    /// The sections of a Slimepedia page
    /// </summary>
    /// <remarks> No info -> "No Information" </remarks>
    private static IReadOnlyList<PediaSection> SectionsOf(PediaInfo info) =>
        info.Sections.Count == 0
            ? new[] { new PediaSection(PediaSectionKind.About, DescriptionOf(info)) }
            : info.Sections;

    /// <summary>
    /// The created content entry
    /// </summary>
    /// <returns> The created entry or null if none was made </returns>
    public static PediaEntry? EntryFor(IdentifiableType subject) =>
        subject != null && Entries.TryGetValue(subject.name, out var entry) && entry != null ? entry : null;

    /// <summary>
    /// The existing entry for a thing
    /// </summary>
    /// <returns> The existing entry or null if none was made </returns>
    private static PediaEntry? Existing(IdentifiableType subject) =>
        Entries.TryGetValue(subject.name, out var entry) && entry != null ? entry : null;

    /// <summary>
    /// Every Slimepedia entry that was created
    /// </summary>
    private static readonly Dictionary<string, PediaEntry> Entries = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a pedia entry from a <see cref="PediaRequest"/>
    /// </summary>
    /// <returns> The entry, or null if it couldn't be made </returns>
    public static PediaEntry? Create(PediaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Subject);

        if (!request.CanBeRecolored) request.Subject.SetCanBeRecolored(false);

        if (HasEntry(request.Subject) && Existing(request.Subject) is { } already)
        {
            LogWarning($"Prism: '{request.Subject.name}' already has a pedia entry, using it");
            return already;
        }

        var template = TemplateFor(request.Category) ?? GetOrAny<IdentifiablePediaEntry>("Pink");
        if (template == null)
        {
            LogWarning($"Prism: Can't create a pedia entry for '{request.Subject.name}': no pedia template entry exists");
            return null;
        }

        var entry = UObject.Instantiate(template);
        var templatePrefix = entry._persistenceId;
        FillInformation(entry, request.Subject.name, request.Description, request.Sections, request.CustomUnlockAction);
        entry._identifiableType = request.Subject;
        entry._title = request.Subject.localizedName;
        
        if (request.FactSet != PediaFactSetKind.None || entry._highlightSet == null)
            entry._highlightSet = FindHighlightSet(request.FactSet);

        Documented.Add(request.Subject.name);
        Entries[request.Subject.name] = entry;
        PediaSaveIds.Register(entry, PediaSaveIds.LegacyIdsOf(request.Subject, templatePrefix));

        Queue(request.Category, entry);
        RebuildCategory(request.Category);

        return entry;
    }

    /// <summary>
    /// A template Slimepedia entry for a specific category tab
    /// </summary>
    /// <returns> The entry, or null if the category tab has none yet </returns>
    public static IdentifiablePediaEntry? TemplateFor(PediaCategoryKind category)
    {
        try
        {
            var asset = GetAll<PediaCategory>().FirstOrDefault(candidate => candidate != null && candidate.name == NameFromCategory(category));
            if (asset?._items == null) return null;

            foreach (var item in asset._items)
            {
                var identifiable = item?.TryCast<IdentifiablePediaEntry>();
                if (identifiable == null || identifiable.IdentifiableType == null) continue;
                if (Entries.ContainsKey(identifiable.name)) continue;

                return identifiable;
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Creates a page that documents no item, from a <see cref="FixedPediaRequest"/>
    /// </summary>
    /// <returns> The entry, or null if it couldn't be created </returns>
    public static PediaEntry? CreateFixed(FixedPediaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = ContentNames.Unique(ContentNames.Sanitize(request.Name), "pedia");
        if (Entries.ContainsKey(name))
        {
            LogWarning($"Prism: Can't create the pedia page '{name}': it already exists");
            return null;
        }

        var template = FixedTemplateFor(request.Category);
        if (template == null)
        {
            LogWarning($"Prism: Can't create the pedia page '{name}': no template page for {request.Category}");
            return null;
        }

        var entry = UObject.Instantiate(template);
        var templatePrefix = entry._persistenceId;
        var stableName = ContentNames.Sanitize(request.Name);
        FillInformation(entry, name, request.Description, request.Sections, request.CustomUnlockAction);
        entry._title = request.Title;
        entry._persistenceSuffix = ContentNames.PediaPersistenceSuffix(stableName, "pedia");
        entry._isUnlockedInitially = false;

        entry._fullArt = null;

        if (request.Icon != null) entry._icon = request.Icon;
        if (request.FactSet != PediaFactSetKind.None) entry._highlightSet = FindHighlightSet(request.FactSet);
        if (!request.CanBeRecolored) entry.SetCanBeRecolored(false);

        Entries[name] = entry;
        Documented.Add(name);
        PediaSaveIds.Register(entry, PediaSaveIds.Combine(new[] { templatePrefix }, PediaSaveIds.LegacySuffixesOf(stableName, name, "pedia")));

        Queue(request.Category, entry);
        RebuildCategory(request.Category);

        return entry;
    }

    /// <summary>
    /// A template fixed Slimepedia entry for a specific category tab
    /// </summary>
    /// <returns> The page, or null if the category tab has none </returns>
    private static FixedPediaEntry? FixedTemplateFor(PediaCategoryKind category)
    {
        try
        {
            var asset = GetAll<PediaCategory>().FirstOrDefault(candidate => candidate != null && candidate.name == NameFromCategory(category));
            if (asset?._items == null) return null;

            foreach (var item in asset._items)
            {
                var fixedEntry = item?.TryCast<FixedPediaEntry>();
                if (fixedEntry != null && !Entries.ContainsKey(fixedEntry.name)) return fixedEntry;
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Creates a Slimepedia entry of a slime's radiant form from a <see cref="RadiantPediaRequest"/>
    /// </summary>
    /// <returns> The entry (new or existing), or null if it couldn't be created </returns>
    public static PediaEntry? CreateRadiant(RadiantPediaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Subject);

        var subject = request.Subject;

        if (!SupportRadiant.HasFlag()) return null;

        if (subject.IsLargo)
        {
            LogWarning($"Prism: Can't create a radiant pedia entry for '{subject.name}': largos share their base slime's entries");
            return null;
        }

        var entryName = "Radiant" + subject.name;
        var existing = RadiantEntries.FirstOrDefault(candidate => candidate != null && candidate.name == entryName);
        if (existing != null) return existing;

        var template = Get<RadiantSlimePediaEntry>("RadiantPink") ?? GetAny<RadiantSlimePediaEntry>();
        if (template == null)
        {
            LogWarning($"Prism: Can't create a radiant pedia entry for '{subject.name}': no radiant pedia entry loaded to copy yet");
            return null;
        }

        var entry = UObject.Instantiate(template);
        var templatePrefix = entry._persistenceId;
        FillInformation(entry, entryName, request.Description, request.Sections, request.CustomUnlockAction);
        entry._persistenceId = PediaSaveIds.RadiantPrefix;
        entry._slimeDefinition = subject;
        entry._title = subject.localizedName;
        entry._highlightSet = FindHighlightSet(request.FactSet);
        if (!request.CanBeRecolored) entry.SetCanBeRecolored(false);

        RadiantEntries.Add(entry);
        PediaSaveIds.Register(entry, PediaSaveIds.RadiantLegacyIdsOf(entry, subject, templatePrefix));

        Queue(PediaCategoryKind.RadiantSlimes, entry);
        RebuildCategory(PediaCategoryKind.RadiantSlimes);

        return entry;
    }

    /// <summary>
    /// Fills in common information
    /// </summary>
    private static void FillInformation(
        PediaEntry entry,
        string name,
        LocalizedString description,
        IReadOnlyList<PediaSection> sections,
        Action<PediaUnlock>? unlockAction)
    {
        entry.hideFlags = HideFlags.DontUnloadUnusedAsset;
        entry.name = name;
        entry._persistenceId = PediaSaveIds.Prefix;
        entry._description = description;
        entry._details = BuildDetails(WithCredit(sections));
        PediaEvents.Register(entry, unlockAction);
    }

    /// <summary>
    /// Adds the credit line to the end of an entry's sections
    /// </summary>
    private static IReadOnlyList<PediaSection> WithCredit(IReadOnlyList<PediaSection> sections)
    {
        if (Credit == null) return sections;

        return new List<PediaSection>(sections) { new(PediaSectionKind.About, Credit) };
    }

    /// <summary>
    /// Turns sections into the game's "detail blocks"
    /// </summary>
    private static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PediaEntryDetail> BuildDetails(
        IReadOnlyList<PediaSection> sections)
    {
        var details = new List<PediaEntryDetail>();

        foreach (var section in sections)
        {
            var sectionAsset = FindSection(section.Kind);
            if (sectionAsset == null) continue;

            details.Add(new PediaEntryDetail
            {
                Section     = sectionAsset,
                Text        = section.Text,
                TextGamepad = section.Text,
                TextPS4     = section.Text,
                Contents    = Array.Empty<PediaEntryDetailSubContent>().ToIl2CppArray()
            });
        }

        return details.ToIl2CppArray();
    }

    /// <summary>
    /// Rebuilds a category
    /// </summary>
    private static void RebuildCategory(PediaCategoryKind category)
    {
        try
        {
            var name = NameFromCategory(category);
            var asset = GetAll<PediaCategory>()
                .FirstOrDefault(candidate => candidate.name == name);

            asset?.GetTypedRuntimeCategory();
        }
        catch { }
    }

    /// <summary>
    /// The game's asset name for a category
    /// </summary>
    /// <remarks> Inverse of <see cref="CategoryFromName"/> </remarks>
    private static string NameFromCategory(PediaCategoryKind category) => category switch
    {
        PediaCategoryKind.Slimes        => "Slimes",
        PediaCategoryKind.RadiantSlimes => "Radiant Slimes",
        PediaCategoryKind.Resources     => "Resources",
        PediaCategoryKind.World         => "World",
        PediaCategoryKind.Ranch         => "Ranch",
        PediaCategoryKind.Science       => "Science",
        PediaCategoryKind.Toys          => "Toys",
        PediaCategoryKind.Weather       => "Weather",
        PediaCategoryKind.Blueprints    => "Blueprints",
        PediaCategoryKind.Tutorial      => "Tutorials",
        _ => string.Empty
    };

    /// <summary>
    /// Adds an entry to a pending entry
    /// </summary>
    private static void Queue(PediaCategoryKind category, PediaEntry entry)
    {
        if (!PendingEntries.TryGetValue(category, out var entries))
        {
            entries = new List<PediaEntry>();
            PendingEntries[category] = entries;
        }
        entries.Add(entry);
    }

    /// <summary>
    /// The game's sections
    /// </summary>
    private static PediaDetailSection? FindSection(PediaSectionKind kind) => Get<PediaDetailSection>(kind switch
    {
        PediaSectionKind.Slimeology     => "Slimeology",
        PediaSectionKind.RancherRisks   => "Rancher Risks",
        PediaSectionKind.Plortonomics   => "Plortonomics",
        PediaSectionKind.About          => "About",
        PediaSectionKind.OnTheRanch     => "How To Use",
        PediaSectionKind.Instructions   => "Instructions",
        _ => string.Empty
    });

    /// <summary>
    /// The game's facts panel asset of a fact set
    /// </summary>
    private static PediaHighlightSet? FindHighlightSet(PediaFactSetKind kind) => Get<PediaHighlightSet>(kind switch
    {
        PediaFactSetKind.Slime => "SlimeHighlights",
        PediaFactSetKind.Resource => "ResourceHighlights",

        // Misspelled in the game's assets
        PediaFactSetKind.Food => "FoodHightlights",
        _ => "TutorialPediaTemplate"
    });

    /// <summary>
    /// The category from a game asset name
    /// </summary>
    internal static PediaCategoryKind? CategoryFromName(string name) => name switch
    {
        "Slimes"            => PediaCategoryKind.Slimes,
        "Radiant Slimes"    => PediaCategoryKind.RadiantSlimes,
        "Resources"         => PediaCategoryKind.Resources,
        "World"             => PediaCategoryKind.World,
        "Ranch"             => PediaCategoryKind.Ranch,
        "Science"           => PediaCategoryKind.Science,
        "Toys"              => PediaCategoryKind.Toys,
        "Weather"           => PediaCategoryKind.Weather,
        "Blueprints"        => PediaCategoryKind.Blueprints,
        "Tutorials"         => PediaCategoryKind.Tutorial,
        _ => null
    };
}