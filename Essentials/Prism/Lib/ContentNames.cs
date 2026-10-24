using System;
using System.Linq;

namespace Starlight.Prism.Lib;

/// <summary>
/// Identifiers & Reference ID conversion
/// </summary>
public static class ContentNames
{
    /// <summary>
    /// Prefix of every Prism-created reference id
    /// </summary>
    public const string ModdedPrefix = "Modded";

    /// <summary>
    /// The id of the mod whose content is being created
    /// </summary>
    public static string? SourceModId { get; set; }

    /// <summary>
    /// Four letters identifying the current mod
    /// </summary>
    public static string SourceModTag => TagFor(SourceModId);

    private static readonly char[] Separators = { '_', '-', ' ', '.' };

    /// <summary>
    /// Names that were already claimed
    /// </summary>
    private static readonly Dictionary<string, string> ClaimedNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Names that were already claimed, per kind
    /// </summary>
    private static readonly Dictionary<string, HashSet<string>> ClaimedNamesByKind = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Sanitizes a name
    /// </summary>
    public static string Unique(string raw, string owner)
    {
        ArgumentNullException.ThrowIfNull(raw);

        var key = owner + "\0" + raw;
        if (ClaimedNames.TryGetValue(key, out var alreadyIssued)) return alreadyIssued;

        var name = Sanitize(raw);

        if (!ClaimedNamesByKind.TryGetValue(owner, out var claimed))
        {
            claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ClaimedNamesByKind[owner] = claimed;
        }

        // Claimed by another piece of content of the same kind
        if (!claimed.Add(name))
        {
            name += key.ToLetterHash();
            claimed.Add(name);
        }

        ClaimedNames[key] = name;
        return name;
    }

    /// <summary>
    /// Drops kind words, so they are not duplicate
    /// </summary>
    public static string WithoutKindSuffix(string raw, string kind)
    {
        ArgumentNullException.ThrowIfNull(raw);

        if (string.IsNullOrEmpty(kind)) return raw;

        var trimmed = raw.TrimEnd();
        if (!trimmed.EndsWith(kind, StringComparison.OrdinalIgnoreCase)) return raw;

        var head = trimmed[..^kind.Length].TrimEnd(Separators);

        // A name that is only its kind stays as is :P
        return head.Length == 0 ? raw : head;
    }

    /// <summary>
    /// Sanitizes identifiers
    /// </summary>
    public static string Sanitize(string raw)
    {
        ArgumentNullException.ThrowIfNull(raw);
        
        var words = raw
            .Split(Separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => new string(word.Where(c => c.IsAsciiLetter()).ToArray()))
            .Where(word => word.Length > 0)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]);

        var name = string.Concat(words);
        if (name.Length == 0) name = "UnnamedImport";
        
        var keptCharacters = raw.Count(c => c.IsAsciiLetter() || Separators.Contains(c));
        var lostCharacters = raw.Length != keptCharacters;

        return lostCharacters ? name + raw.ToLetterHash() : name;
    }

    /// <summary>
    /// The tag a given mod's content carries
    /// </summary>
    public static string TagFor(string modId) =>
        string.IsNullOrEmpty(modId) ? string.Empty : modId.ToLetterHash();

    /// <summary>
    /// True if the <paramref name="name"/> is already the required format
    /// </summary>
    public static bool IsValid(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.All(c => c.IsAsciiLetter());

    /// <summary>
    /// The name a reference id is built from, sanitized but without the kind suffix
    /// </summary>
    public static string StableName(string raw, string kind) => Sanitize(WithoutKindSuffix(raw, kind));

    /// <summary>
    /// Builds a reference id ending with the mod's tag; <c>IdentifiableType.ModdedTestPlortABCD</c>
    /// </summary>
    public static string ReferenceId(string prefix, string stableName, string kind) =>
        $"{prefix}.{ModdedPrefix}{stableName}{kind}{SourceModTag}";

    /// <summary>
    /// The id this content had before ids carried a mod tag; <c>IdentifiableType.ModdedTestPlort</c>
    /// </summary>
    public static string LegacyReferenceId(string prefix, string name, string kind) =>
        $"{prefix}.{ModdedPrefix}{name}{kind}";

    /// <summary>
    /// Reference id for a created slime; <c>SlimeDefinition.ModdedTestABCD</c>
    /// </summary>
    public static string SlimeReferenceId(string sanitizedName) =>
        ReferenceId("SlimeDefinition", sanitizedName, string.Empty);

    /// <summary>
    /// Unity object name for a created plort
    /// </summary>
    public static string PlortObjectName(string sanitizedName) => sanitizedName + "Plort";

    /// <summary>
    /// Reference id for a created plort
    /// </summary>
    public static string PlortReferenceId(string sanitizedName) =>
        ReferenceId("IdentifiableType", sanitizedName, "Plort");

    /// <summary>
    /// Unity object name for a created fruit
    /// </summary>
    public static string FruitObjectName(string sanitizedName) => sanitizedName + "Fruit";

    /// <summary>
    /// Reference id for a created fruit
    /// </summary>
    public static string FruitReferenceId(string sanitizedName) =>
        ReferenceId("IdentifiableType", sanitizedName, "Fruit");

    /// <summary>
    /// Suffix the pedia persists the "seen" state under
    /// </summary>
    public static string PediaPersistenceSuffix(string sanitizedName, string kind) =>
        $"{ModdedPrefix.ToLowerInvariant()}{sanitizedName.ToLowerInvariant()}_{kind}{SourceModTag.ToLowerInvariant()}";
}