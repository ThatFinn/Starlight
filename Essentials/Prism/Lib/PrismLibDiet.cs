using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Wrappers;

namespace Starlight.Prism.Lib;
/// <summary>
/// A library of helper functions for dealing with slime diets
/// </summary>
public static class PrismLibDiet
{
    internal static readonly Dictionary<SlimeDefinition, Dictionary<SlimeDiet.EatMapEntry, bool>> CustomEatmaps = new ();

    /// <summary>
    /// Creates a new <see cref="SlimeDiet.EatMapEntry"/>
    /// </summary>
    /// <param name="driver">The emotion that drives the slime to eat this food</param>
    /// <param name="minDrive">The minimum drive the slime needs to eat this food</param>
    /// <param name="produce">What the slime produces after eating this food</param>
    /// <param name="eat">The food the slime eats</param>
    /// <param name="becomes">What the slime becomes after eating this food</param>
    /// <returns>The new <see cref="SlimeDiet.EatMapEntry"/></returns>
    public static SlimeDiet.EatMapEntry CreateEatmapEntry(SlimeEmotions.Emotion driver, float minDrive, IdentifiableType produce, IdentifiableType eat, IdentifiableType becomes)
        => new SlimeDiet.EatMapEntry
        {
            EatsIdent = eat,
            ProducesIdent = produce,
            BecomesIdent = becomes,
            Driver = driver,
            MinDrive = minDrive
        };

    /// <summary>
    /// Creates a new <see cref="SlimeDiet.EatMapEntry"/>
    /// </summary>
    /// <param name="driver">The emotion that drives the slime to eat this food</param>
    /// <param name="mindrive">The minimum drive the slime needs to eat this food</param>
    /// <param name="produce">What the slime produces after eating this food</param>
    /// <param name="eat">The food the slime eats</param>
    /// <returns>The new <see cref="SlimeDiet.EatMapEntry"/></returns>
    public static SlimeDiet.EatMapEntry CreateEatmapEntry(SlimeEmotions.Emotion driver, float mindrive, IdentifiableType produce, IdentifiableType eat)
        => new SlimeDiet.EatMapEntry
        {
            EatsIdent = eat,
            ProducesIdent = produce,
            Driver = driver,
            MinDrive = mindrive
        };

    internal static bool _CarefulCheck(Il2CppSystem.Collections.Generic.List<SlimeDiet.EatMapEntry> list,
        SlimeDiet.EatMapEntry eatmap)
    {
        if (list == null) return false;
        if (eatmap == null) return false;
        foreach (var entry in list)
        {
            if (eatmap.BecomesIdent != entry.BecomesIdent) continue;
            if (eatmap.EatsIdent != entry.EatsIdent) continue;
            if (eatmap.Driver != entry.Driver) continue;
            if (!Mathf.Approximately(eatmap.MinDrive, entry.MinDrive)) continue;
            if (eatmap.ProducesIdent != entry.ProducesIdent) continue;
            return false;
        }
        return true;
    }
    /// <summary>
    /// Adds an <see cref="SlimeDiet.EatMapEntry"/> to a slime's diet
    /// </summary>
    /// <param name="prismSlime">The slime to add the eatmap to</param>
    /// <param name="eatmap">The eatmap to add</param>
    /// <param name="beCareful">Whether to check if the eatmap already exists</param>
    public static void AddEatmapToSlime(this PrismSlime prismSlime, SlimeDiet.EatMapEntry eatmap, bool beCareful = false)
    {
        var eatmaps = new Dictionary<SlimeDiet.EatMapEntry,bool>();
        if (CustomEatmaps.TryGetValue(prismSlime, out var dict))
            eatmaps = dict;
        else CustomEatmaps.Add(prismSlime, eatmaps);
        eatmaps.Add(eatmap,beCareful);
        if(beCareful)
            if(!_CarefulCheck(prismSlime.GetSlimeDiet().EatMap,eatmap))
                return;
        prismSlime.GetSlimeDiet().EatMap.Add(eatmap);
    }

    /// <summary>
    /// Refreshes the eat map of a slime
    /// </summary>
    /// <param name="prismSlime">The slime to refresh</param>
    public static void RefreshEatMap(this PrismSlime prismSlime)
    {
        prismSlime.GetSlimeDiet().RefreshEatMap(gameContext.SlimeDefinitions, prismSlime);
    }

    /// <summary>
    /// Adds an item to the slime's produce list
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableType">The item to add</param>
    public static void AddEatProduction(this PrismSlime prismSlime, IdentifiableType identifiableType)
    {
        prismSlime.GetSlimeDiet().ProduceIdents = prismSlime.GetSlimeDiet().ProduceIdents.AddToNew(identifiableType);
    }
    /// <summary>
    /// Sets an item in the slime's produce list
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableType">The item to set</param>
    /// <param name="index">The index to set the item at</param>
    public static void SetEatProduction(this PrismSlime prismSlime, IdentifiableType identifiableType, int index)
    {
        prismSlime.GetSlimeDiet().ProduceIdents[index] = identifiableType;
    }
    /// <summary>
    /// Removes an item from the slime's produce list
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableType">The item to remove</param>
    /// <param name="index">The index of the item to remove</param>
    public static void RemoveEatProduction(this PrismSlime prismSlime, IdentifiableType identifiableType, int index)
    {
        prismSlime.GetSlimeDiet().ProduceIdents = prismSlime.GetSlimeDiet().ProduceIdents.RemoveToNew(identifiableType);
    }

    /// <summary>
    /// Adds a favorite food to the slime
    /// To make it edible, add it as an additional food as well
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableType">The favorite food to add</param>
    public static void AddFavoriteFood(this PrismSlime prismSlime, IdentifiableType identifiableType)
    {
        prismSlime.GetSlimeDiet().FavoriteIdents = prismSlime.GetSlimeDiet().FavoriteIdents.AddToNew(identifiableType);
    }
    /// <summary>
    /// Sets a favorite food of the slime
    /// To make it edible, add it as an additional food as well
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableType">The favorite food to set</param>
    /// <param name="index">The index to set the favorite food at</param>
    public static void SetFavoriteFood(this PrismSlime prismSlime, IdentifiableType identifiableType, int index)
    {
        prismSlime.GetSlimeDiet().FavoriteIdents[index] = identifiableType;
    }
    /// <summary>
    /// Removes a favorite food from the slime
    /// To make it not edible, remove it as an additional food as well
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableType">The favorite food to remove</param>
    public static void RemoveFavoriteFood(this PrismSlime prismSlime, IdentifiableType identifiableType)
    {
        prismSlime.GetSlimeDiet().FavoriteIdents = prismSlime.GetSlimeDiet().FavoriteIdents.RemoveToNew(identifiableType);
    }
    /// <summary>
    /// Sets the amount of plorts produced when the slime eats a favorite food
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="count">The amount of plorts to produce</param>
    public static void SetFavoriteFoodProductionCount(this PrismSlime prismSlime, int count)
    {
        prismSlime.GetSlimeDiet().FavoriteProductionCount = count;
    }
    
    /// <summary>
    /// Adds an additional food to the slime
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableType">The food to add</param>
    public static void AddAdditionalFood(this PrismSlime prismSlime, IdentifiableType identifiableType)
    {
        prismSlime.GetSlimeDiet().AdditionalFoodIdents = prismSlime.GetSlimeDiet().AdditionalFoodIdents.AddToNew(identifiableType);
    }
    /// <summary>
    /// Sets an additional food of the slime
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableType">The food to set</param>
    /// <param name="index">The index to set the food at</param>
    public static void SetAdditionalFood(this PrismSlime prismSlime, IdentifiableType identifiableType, int index)
    {
        prismSlime.GetSlimeDiet().AdditionalFoodIdents[index] = identifiableType;
    }
    /// <summary>
    /// Removes an additional food from the slime
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableType">The food to remove</param>
    public static void RemoveAdditionalFood(this PrismSlime prismSlime, IdentifiableType identifiableType)
    {
        prismSlime.GetSlimeDiet().AdditionalFoodIdents = prismSlime.GetSlimeDiet().AdditionalFoodIdents.RemoveToNew(identifiableType);
    }

    /// <summary>
    /// Adds a food group to the slime
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableTypeGroup">The food group to add</param>
    public static void AddFoodGroup(this PrismSlime prismSlime, IdentifiableTypeGroup identifiableTypeGroup)
    {
        identifiableTypeGroup._isFood = true;
        prismSlime.GetSlimeDiet().MajorFoodIdentifiableTypeGroups = prismSlime.GetSlimeDiet().MajorFoodIdentifiableTypeGroups.AddToNew(identifiableTypeGroup);
    }
    /// <summary>
    /// Sets a food group of the slime
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableTypeGroup">The food group to set</param>
    /// <param name="index">The index to set the food group at</param>
    public static void SetFoodGroup(this PrismSlime prismSlime, IdentifiableTypeGroup identifiableTypeGroup, int index)
    {
        prismSlime.GetSlimeDiet().MajorFoodIdentifiableTypeGroups[index] = identifiableTypeGroup;
    }
    /// <summary>
    /// Removes a food group from the slime
    /// </summary>
    /// <param name="prismSlime">The slime to modify</param>
    /// <param name="identifiableTypeGroup">The food group to remove</param>
    public static void RemoveFoodGroup(this PrismSlime prismSlime, IdentifiableTypeGroup identifiableTypeGroup)
    {
        prismSlime.GetSlimeDiet().MajorFoodIdentifiableTypeGroups = prismSlime.GetSlimeDiet().MajorFoodIdentifiableTypeGroups.RemoveToNew(identifiableTypeGroup);
    }

    internal static SlimeDiet CreateNewDiet() => new SlimeDiet
    {
        ProduceIdents = new Il2CppReferenceArray<IdentifiableType>(0),
        FavoriteProductionCount = 2,
        EatMap = new Il2CppSystem.Collections.Generic.List<SlimeDiet.EatMapEntry>(0),
        FavoriteIdents = new Il2CppReferenceArray<IdentifiableType>(0),
        AdditionalFoodIdents = new Il2CppReferenceArray<IdentifiableType>(0),
        MajorFoodIdentifiableTypeGroups = new Il2CppReferenceArray<IdentifiableTypeGroup>(0),
        BecomesOnTarrifyIdentifiableType = PrismNativeBaseSlime.Tarr.GetPrismBaseSlime(),
        EdiblePlortIdentifiableTypeGroup = LookupEUtil.allIdentifiableTypeGroups["EdiblePlortFoodGroup"],
        StableResourceIdentifiableTypeGroup = LookupEUtil.allIdentifiableTypeGroups["StableResourcesGroup"],
        UnstableResourceIdentifiableTypeGroup = LookupEUtil.allIdentifiableTypeGroups["UnstableResourcesGroup"],
        UnstablePlort = PrismNativePlort.Unstable.GetPrismPlort()
    };
}