using System;
using System.Linq;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Pedia;

namespace Starlight.Prism.Lib;

/// <summary>
/// Keeps created Slimepedia entries unlocked across saving and loading
/// </summary>
public static class PediaSaveIds
{
    /// <summary>
    /// The persistence id prefix of every created entry; <c>Modded.test_slimeabcd</c>
    /// </summary>
    public const string Prefix = "Modded";

    /// <summary>
    /// The persistence id prefix of every created radiant slime entry
    /// </summary>
    public const string RadiantPrefix = "ModdedRadiant";

    /// <summary>
    /// The prefix older Prism versions used
    /// </summary>
    private const string LegacyPrefix = "Pink";

    /// <summary>
    /// Every created entry by each id it can be loaded under
    /// </summary>
    private static readonly Dictionary<string, PediaEntry> Ids = new(StringComparer.Ordinal);

    /// <summary>
    /// The suffixes older Prism versions gave a created type, by the type's pointer
    /// </summary>
    private static readonly Dictionary<IntPtr, List<string>> LegacySuffixes = new();

    /// <summary>
    /// Adds a Slimepedia suffix to a created thing & registers legacy suffixes
    /// </summary>
    /// <param name="stableName"> The name the type's reference id is built from </param>
    /// <param name="legacyName"> The name older versions built the suffix from </param>
    /// <param name="kind"> What the type is; <c>slime</c> </param>
    public static void SetSuffix(IdentifiableType type, string stableName, string legacyName, string kind)
    {
        ArgumentNullException.ThrowIfNull(type);

        type._pediaPersistenceSuffix = ContentNames.PediaPersistenceSuffix(stableName, kind);
        LegacySuffixes[type.Pointer] = LegacySuffixesOf(stableName, legacyName, kind);
    }

    /// <summary>
    /// The legacy suffixes
    /// </summary>
    internal static List<string> LegacySuffixesOf(string stableName, string legacyName, string kind) =>
        new[] { legacyName, stableName }
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => ContentNames.LegacyPediaPersistenceSuffix(name, kind))
            .Distinct(StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// The suffixes older versions gave a created thing
    /// </summary>
    internal static IReadOnlyList<string> LegacySuffixesOf(IdentifiableType? type) =>
        type != null && LegacySuffixes.TryGetValue(type.Pointer, out var suffixes) ? suffixes : Array.Empty<string>();

    /// <summary>
    /// Every combination of an old prefix and an old suffix
    /// </summary>
    internal static IEnumerable<string> Combine(IEnumerable<string?> prefixes, IEnumerable<string> suffixes)
    {
        var suffixList = suffixes.Where(suffix => !string.IsNullOrEmpty(suffix)).Distinct(StringComparer.Ordinal).ToList();

        foreach (var prefix in prefixes.Where(prefix => !string.IsNullOrEmpty(prefix)).Distinct(StringComparer.Ordinal))
            foreach (var suffix in suffixList)
                yield return $"{prefix}.{suffix}";
    }

    /// <summary>
    /// The legacy ids an item's entry has, with the prefix its template gave it and the one slimes used
    /// </summary>
    internal static IEnumerable<string> LegacyIdsOf(IdentifiableType subject, string? templatePrefix) =>
        Combine(new[] { templatePrefix, LegacyPrefix }, LegacySuffixesOf(subject));

    /// <summary>
    /// The legacy ids a radiant Slimepedia entry has
    /// </summary>
    internal static IEnumerable<string> RadiantLegacyIdsOf(PediaEntry entry, SlimeDefinition subject, string? templatePrefix)
    {
        var id = entry.PersistenceId;
        var suffix = subject._pediaPersistenceSuffix;
        var dot = id?.IndexOf('.') ?? -1;

        if (id == null || dot < 0 || string.IsNullOrEmpty(suffix) || string.IsNullOrEmpty(templatePrefix)) yield break;

        var rest = id[(dot + 1)..];
        if (!rest.EndsWith(suffix, StringComparison.Ordinal)) yield break;

        var marker = rest[..^suffix.Length];

        foreach (var legacy in LegacySuffixesOf(subject))
            yield return $"{templatePrefix}.{marker}{legacy}";
    }

    /// <summary>
    /// Registers a Slimepedia entry with legacy ids
    /// </summary>
    internal static void Register(PediaEntry entry, IEnumerable<string> legacyIds)
    {
        if (entry == null) return;

        try
        {
            Add(entry.PersistenceId, entry);

            foreach (var legacyId in legacyIds)
                Add(legacyId, entry);

            Apply(autoSaveDirector?._saveReferenceTranslation);
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to register the save ids of pedia entry '{entry.name}'", ex);
        }
    }

    private static void Add(string? id, PediaEntry entry)
    {
        if (string.IsNullOrEmpty(id)) return;

        if (Ids.TryGetValue(id, out var existing) && existing != null && existing.Pointer != entry.Pointer)
        {
            LogWarning($"Prism: Pedia entries '{existing.name}' and '{entry.name}' both use the save id '{id}', keeping '{existing.name}'");
            return;
        }

        Ids[id] = entry;
    }

    internal static void Apply(SaveReferenceTranslation? translation)
    {
        var lookup = translation?._pediaEntryLookup;
        if (lookup == null) return;

        foreach (var (id, entry) in Ids)
            if (entry != null && !lookup.ContainsKey(id))
                lookup.Add(id, entry);
    }
}

[HarmonyPatch(typeof(SaveReferenceTranslation), nameof(SaveReferenceTranslation.OnPediaEntryListChanged))]
internal static class PediaSaveIdsPatch
{
    public static void Postfix(SaveReferenceTranslation __instance)
    {
        try
        {
            PediaSaveIds.Apply(__instance);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to add the created pedia ids to the save lookup", ex);
        }
    }
}