using System;

namespace Starlight.Prism.Lib;

/// <summary>
/// Makes created content survive saving and loading
/// </summary>
public static class SaveRegistry
{
    /// <summary>
    /// Every identifiable type registered using Prism
    /// </summary>
    private static readonly Dictionary<string, IdentifiableType> Registered = new(StringComparer.Ordinal);

    /// <summary>
    /// Reference ids that were not registered
    /// </summary>
    private static readonly HashSet<string> MissingPersistenceIds = new(StringComparer.Ordinal);

    /// <summary>
    /// True if this type was registered
    /// </summary>
    internal static bool Contains(IdentifiableType type) => Registered.ContainsValue(type);

    /// <summary>
    /// Registers an identifiable type
    /// </summary>
    /// <param name="referenceId"> Persistence id, built with <see cref="ContentNames"/> </param>
    /// <param name="legacyIds"> Ids older saves used for this type </param>
    /// <inheritdoc cref="Register(IdentifiableType,string)"/>
    public static bool Register(IdentifiableType type, string referenceId, params string[] legacyIds)
    {
        if (!Register(type, referenceId)) return false;

        foreach (var legacyId in legacyIds)
            RegisterLegacyId(legacyId, referenceId, type);

        return true;
    }

    /// <returns>
    /// False if a different type already holds the id
    /// </returns>
    /// <inheritdoc cref="Register(IdentifiableType,string,string[])"/>
    public static bool Register(IdentifiableType type, string referenceId)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (string.IsNullOrWhiteSpace(referenceId))
            referenceId = type.ReferenceId;
        
        if (Registered.TryGetValue(referenceId, out var existing) && existing.Pointer != type.Pointer)
        {
            LogError($"Prism: Can't register '{type.name}' as '{referenceId}': '{existing.name}' already has that id");
            return false;
        }

        Registered[referenceId] = type;

        var config = autoSaveDirector._configuration;
        if (!config._identifiableTypes.IsMember(type))
            lookupDirector.AddIdentifiableTypeToGroup(type, config._identifiableTypes);

        lookupDirector._identifiableTypeByRefId.TryAdd(referenceId, type);

        RegisterSaveReference(referenceId, type);
        return true;
    }

    /// <summary>
    /// Registers a legacy ID
    /// </summary>
    private static void RegisterLegacyId(string legacyId, string referenceId, IdentifiableType type)
    {
        if (string.IsNullOrWhiteSpace(legacyId)) return;
        if (string.Equals(legacyId, referenceId, StringComparison.Ordinal)) return;
        
        if (lookupDirector._identifiableTypeByRefId.ContainsKey(legacyId)) return;

        lookupDirector._identifiableTypeByRefId.TryAdd(legacyId, type);
        autoSaveDirector._saveReferenceTranslation?._identifiableTypeLookup.TryAdd(legacyId, type);
    }
    
    private static void RegisterSaveReference(string referenceId, IdentifiableType type)
    {
        var translation = autoSaveDirector._saveReferenceTranslation;
        if (translation == null) return;

        translation._identifiableTypeLookup.TryAdd(referenceId, type);

        RegisterPersistenceId(translation._identifiableTypeToPersistenceId, referenceId);
        
        if (SlimeDefinition.IsSlimeDefinition(type))
        {
            var definition = type.Cast<SlimeDefinition>();
            slimeDefinitions._slimeDefinitionsByIdentifiable.TryAdd(type, definition);
            if (!slimeDefinitions.Slimes.Contains(definition))
                slimeDefinitions.Slimes = slimeDefinitions.Slimes.AddToNew(definition);
        }

        type.referenceId = referenceId;
    }
    
    private static void RegisterPersistenceId(
        Il2CppMonomiPark.SlimeRancher.Persist.PersistenceIdLookupTable<IdentifiableType> persistence,
        string referenceId)
    {
        if (persistence._primaryIndex == null || persistence._primaryIndex.Count == 0)
        {
            MissingPersistenceIds.Add(referenceId);
            return;
        }

        MissingPersistenceIds.Remove(referenceId);

        if (!persistence._primaryIndex.Contains(referenceId))
            persistence._primaryIndex = persistence._primaryIndex.AddToNew(referenceId);
        
        var position = -1;
        for (var i = 0; i < persistence._primaryIndex.Count; i++)
        {
            if (!string.Equals(persistence._primaryIndex[i], referenceId, StringComparison.Ordinal)) continue;
            position = i;
            break;
        }

        if (position < 0) return;
        
        persistence._reverseIndex[referenceId] = position;
    }

    /// <summary>
    /// Re-registers everything Prism created
    /// </summary>
    public static void RefreshAll()
    {
        foreach (var (referenceId, type) in Registered)
        {
            try
            {
                RegisterSaveReference(referenceId, type);
            }
            catch (Exception ex)
            {
                LogError($"Prism: Failed to refresh save reference '{referenceId}'", ex);
            }
        }
    }

    /// <summary>
    /// Checks that all created content can be saved correctly
    /// </summary>
    /// <returns> The reference ids that can't be saved </returns>
    /// <remarks> This is a debug feature </remarks>
    public static IReadOnlyList<string> Verify()
    {
        var unsaveable = new List<string>();

        foreach (var (referenceId, type) in Registered)
        {
            if (type == null)
            {
                unsaveable.Add($"{referenceId} (the content itself is gone)");
                continue;
            }

            if (!IsRegistered(type)) unsaveable.Add(referenceId);
        }

        foreach (var referenceId in MissingPersistenceIds)
            if (!unsaveable.Contains(referenceId)) unsaveable.Add(referenceId);

        if (unsaveable.Count == 0)
        {
            Log($"Prism: All {Registered.Count} created types can be saved.");
            return unsaveable;
        }

        LogError($"Prism: {unsaveable.Count} of {Registered.Count} created types can't be saved:");

        foreach (var referenceId in unsaveable)
            LogError($"Prism:   {referenceId}");

        return unsaveable;
    }

    /// <summary>
    /// True if the type is registered correctly
    /// </summary>
    public static bool IsRegistered(IdentifiableType type)
    {
        try
        {
            autoSaveDirector._saveReferenceTranslation.GetPersistenceId(type);
            return true;
        }
        catch
        {
            return false;
        }
    }
}