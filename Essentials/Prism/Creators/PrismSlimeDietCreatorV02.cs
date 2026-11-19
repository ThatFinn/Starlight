using System;
using System.Linq;
using CppList = Il2CppSystem.Collections.Generic;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Starlight.Prism.Lib;
using Starlight.Prism.Data.Native;

namespace Starlight.Prism.Creators;

/// <summary>
/// An entry for something a slime eats
/// </summary>
/// <param name="Food"> What is eaten </param>
/// <param name="Produces"> What is produced, normally a plort, or null if it produces nothing </param>
/// <param name="Becomes"> What the slime turns into, or null for an ordinary meal </param>
/// <param name="IsFavourite"> Whether this is a favourite, which produces extra and is highlighted in the Slimepedia </param>
/// <param name="FavouriteProductionCount"> How much a favourite produces </param>
public readonly record struct DietEntry(
    IdentifiableType Food,
    IdentifiableType? Produces,
    IdentifiableType? Becomes,
    bool IsFavourite,
    int FavouriteProductionCount);

/// <summary>
/// The diet creator, used to modify what slimes eat
/// </summary>
public static class PrismSlimeDietCreatorV02
{
    /// <summary>
    /// Entries added by Prism, by the reference id of the slime
    /// </summary>
    private static readonly Dictionary<string, List<DietEntry>> CustomEntries = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a fresh diet to work with
    /// </summary>
    /// <remarks> Keeps the food groups of the slime it is based on, but none of its entries </remarks>
    /// <param name="basedOn"> The native slime being derived from, or null for a bare diet </param>
    public static SlimeDiet Create(SlimeDefinition? basedOn = null)
    {
        var inheritedGroups = basedOn?.Diet?.MajorFoodIdentifiableTypeGroups.AsEnumerable()
            .Where(group => group != null)
            .ToList();

        return new SlimeDiet
        {
            ProduceIdents = Array.Empty<IdentifiableType>().ToIl2CppArray(),
            FavoriteIdents = Array.Empty<IdentifiableType>().ToIl2CppArray(),
            AdditionalFoodIdents = Array.Empty<IdentifiableType>().ToIl2CppArray(),
            MajorFoodIdentifiableTypeGroups = (inheritedGroups ?? new List<IdentifiableTypeGroup>()).ToIl2CppArray(),
            EatMap = new CppList.List<SlimeDiet.EatMapEntry>(),
            FavoriteProductionCount = basedOn?.Diet?.FavoriteProductionCount ?? 2,
            BecomesOnTarrifyIdentifiableType = PrismNativeBaseSlime.Tarr.Find(),
            EdiblePlortIdentifiableTypeGroup = Groups.Find(Groups.Names.EdiblePlortFood),
            StableResourceIdentifiableTypeGroup = Groups.Find("StableResourcesGroup"),
            UnstableResourceIdentifiableTypeGroup = Groups.Find("UnstableResourcesGroup"),
            UnstablePlort = PrismNativePlort.Unstable.Find()
        };
    }

    /// <summary>
    /// Adds one food to a slime's diet
    /// </summary>
    public static void AddEntry(SlimeDefinition definition, DietEntry entry)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(entry.Food);

        var diet = definition.Diet;
        if (diet == null)
        {
            LogWarning($"Prism: Can't add '{entry.Food.name}' to '{definition.name}': it has no diet");
            return;
        }
        
        if (!diet.AdditionalFoodIdents.AsEnumerable().Contains(entry.Food))
            diet.AdditionalFoodIdents = diet.AdditionalFoodIdents.AddToNew(entry.Food);
        
        if (entry.IsFavourite && !diet.FavoriteIdents.AsEnumerable().Contains(entry.Food))
            diet.FavoriteIdents = diet.FavoriteIdents.AddToNew(entry.Food);
        
        if (entry.Produces != null && !diet.ProduceIdents.AsEnumerable().Contains(entry.Produces))
            diet.ProduceIdents = diet.ProduceIdents.AddToNew(entry.Produces);

        Remember(definition, entry);

        var existing = FindEntryFor(diet.EatMap, entry.Food);
        if (existing == null)
        {
            diet.EatMap.Add(BuildEatMapEntry(entry, diet));
            return;
        }
        
        Merge(existing, entry, diet);
    }

    private static void Merge(SlimeDiet.EatMapEntry existing, DietEntry entry, SlimeDiet diet)
    {
        if (entry.IsFavourite && !existing.IsFavorite)
        {
            existing.IsFavorite = true;
            existing.FavoriteProductionCount = entry.FavouriteProductionCount > 0
                ? entry.FavouriteProductionCount
                : diet.FavoriteProductionCount;
        }

        if (existing.ProducesIdent == null) existing.ProducesIdent = entry.Produces;
        if (existing.BecomesIdent == null) existing.BecomesIdent = entry.Becomes;
    }

    /// <summary>
    /// Builds the game's eat-map entry
    /// </summary>
    private static SlimeDiet.EatMapEntry BuildEatMapEntry(DietEntry entry, SlimeDiet diet)
    {
        var favouriteCount = entry.FavouriteProductionCount > 0
            ? entry.FavouriteProductionCount
            : diet.FavoriteProductionCount;

        return new SlimeDiet.EatMapEntry
        {
            EatsIdent = entry.Food,
            ProducesIdent = entry.Produces,
            BecomesIdent = entry.Becomes,
            IsFavorite = entry.IsFavourite,
            ProductionCount = 1,
            FavoriteProductionCount = favouriteCount,
            
            Driver = SlimeEmotions.Emotion.HUNGER,
            MinDrive = 1f
        };
    }

    /// <summary>
    /// Rebuilds a slime's eat map from its food lists and restores Prism's entries
    /// </summary>
    public static void Refresh(SlimeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        definition.Diet?.RefreshEatMap(slimeDefinitions, definition);
        ReapplyAfterRefresh(definition);
    }

    /// <summary>
    /// Re-adds Prism's recorded entries
    /// </summary>
    public static void ReapplyAfterRefresh(SlimeDefinition definition)
    {
        if (!CustomEntries.TryGetValue(definition.ReferenceId, out var entries)) return;

        var diet = definition.Diet;
        if (diet?.EatMap == null) return;

        foreach (var entry in entries)
        {
            if (ContainsEntryFor(diet.EatMap, entry.Food)) continue;
            diet.EatMap.Add(BuildEatMapEntry(entry, diet));
        }
    }

    /// <summary>
    /// Whether the eat map already has an entry for this food
    /// </summary>
    private static bool ContainsEntryFor(CppList.List<SlimeDiet.EatMapEntry> eatMap, IdentifiableType food) =>
        FindEntryFor(eatMap, food) != null;

    /// <summary>
    /// The eat map's entry for this food, or null if it has none
    /// </summary>
    private static SlimeDiet.EatMapEntry? FindEntryFor(
        CppList.List<SlimeDiet.EatMapEntry> eatMap,
        IdentifiableType food)
    {
        foreach (var existing in eatMap)
        {
            if (existing?.EatsIdent == null) continue;
            if (existing.EatsIdent.Pointer == food.Pointer) return existing;
        }
        return null;
    }

    /// <summary>
    /// Records an entry
    /// </summary>
    private static void Remember(SlimeDefinition definition, DietEntry entry)
    {
        if (!CustomEntries.TryGetValue(definition.ReferenceId, out var entries))
        {
            entries = new List<DietEntry>();
            CustomEntries[definition.ReferenceId] = entries;
        }

        var index = entries.FindIndex(recorded => recorded.Food.Pointer == entry.Food.Pointer);
        if (index < 0)
        {
            entries.Add(entry);
            return;
        }

        var existing = entries[index];
        entries[index] = existing with
        {
            IsFavourite = existing.IsFavourite || entry.IsFavourite,
            FavouriteProductionCount = Math.Max(existing.FavouriteProductionCount, entry.FavouriteProductionCount),
            Produces = existing.Produces ?? entry.Produces,
            Becomes = existing.Becomes ?? entry.Becomes
        };
    }

    /// <summary>
    /// Rebuilds every slime's diet
    /// </summary>
    /// <remarks> It also restores Prism's entries </remarks>
    public static void RefreshAllSlimes()
    {
        try
        {
            slimeDefinitions.RefreshDefinitions();
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to refresh slime definitions", ex);
            return;
        }

        foreach (var referenceId in CustomEntries.Keys.ToList())
        {
            var definition = LookupEUtil.GetBaseSlimeByRefID(referenceId);
            if (definition != null) ReapplyAfterRefresh(definition);
        }
    }
}