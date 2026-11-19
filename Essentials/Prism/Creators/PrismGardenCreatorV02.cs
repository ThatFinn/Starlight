using System;
using System.Linq;
using UObject = UnityEngine.Object;
using Starlight.Prism.Lib;

namespace Starlight.Prism.Creators;

/// <summary>
/// Makes created food plantable in garden plots
/// </summary>
public static class PrismGardenCreatorV02
{
    /// <summary>
    /// Food registered as plantable
    /// </summary>
    private static readonly Dictionary<IdentifiableType, string> Plantable = new();

    /// <summary>
    /// Upgraded-plot deluxe plants
    /// </summary>
    private static readonly Dictionary<IdentifiableType, GameObject> DeluxePrefabs = new();

    /// <summary>
    /// The grower each plantable food grows through
    /// </summary>
    private static readonly Dictionary<IdentifiableType, ResourceGrowerDefinition> Growers = new();

    /// <summary>
    /// Registers food as plantable
    /// </summary>
    /// <param name="modelledOn"> Reference id of the native crop (fruit or veggie) whose growth rate and yield to copy </param>
    public static void MakePlantable(IdentifiableType food, string modelledOn)
    {
        ArgumentNullException.ThrowIfNull(food);

        if (Growers.ContainsKey(food)) return;

        var native = FindNativeGrower(modelledOn);
        if (native == null)
        {
            LogWarning($"Prism: '{food.name}' isn't plantable: the game has no resource grower to copy yet");
            return;
        }

        Plantable[food] = modelledOn;
        Growers[food] = BuildGrower(native, food);
    }

    /// <summary>
    /// Finds a game's grower for a native crop in the save configuration
    /// </summary>
    private static ResourceGrowerDefinition? FindNativeGrower(string modelledOn)
    {
        var items = autoSaveDirector?._configuration?.ResourceGrowers?.items;
        if (items == null) return null;

        foreach (var grower in items)
            if (grower?._primaryResourceType?.ReferenceId == modelledOn) return grower;

        // Any grower works, only growth rate and yield differ
        foreach (var grower in items)
            if (grower != null) return grower;

        return null;
    }

    /// <summary>
    /// Adds every registered food to one garden plot's accepted-crop table
    /// </summary>
    internal static void ApplyTo(GardenCatcher catcher)
    {
        if (catcher?.Plantable == null || Plantable.Count == 0) return;

        foreach (var (food, modelledOn) in Plantable)
        {
            if (catcher._plantableDict != null && catcher._plantableDict.ContainsKey(food)) continue;
            if (!Growers.TryGetValue(food, out var grower)) continue;
            
            var planted = grower._prefab;
            if (planted == null) continue;

            if (catcher._plantableDict != null)
                catcher._plantableDict[food] = planted;

            var deluxe = DeluxeFor(catcher, food, modelledOn, grower);
            if (deluxe != null && catcher._deluxeDict != null)
                catcher._deluxeDict[food] = deluxe;
        }
    }

    /// <summary>
    /// The upgraded plot's version of a crop
    /// </summary>
    private static GameObject? DeluxeFor(
        GardenCatcher catcher,
        IdentifiableType food,
        string modelledOn,
        ResourceGrowerDefinition grower)
    {
        if (DeluxePrefabs.TryGetValue(food, out var cached)) return cached;

        var slot = FindSlot(catcher, modelledOn);
        if (slot?.DeluxePlantedPrefab == null) return null;

        var deluxe = BuildPlantedPrefab(slot.DeluxePlantedPrefab, food, grower, "DeluxeCrop");
        if (deluxe != null) DeluxePrefabs[food] = deluxe;

        return deluxe;
    }

    /// <summary>
    /// Finds this plot's entry for the native crop being modeled on
    /// </summary>
    private static GardenCatcher.PlantSlot? FindSlot(GardenCatcher catcher, string modelledOn)
    {
        foreach (var slot in catcher.Plantable)
        {
            if (slot?.IdentType == null) continue;
            if (slot.IdentType.ReferenceId == modelledOn) return slot;
        }
        
        foreach (var slot in catcher.Plantable)
            if (slot?.IdentType != null && slot.PlantedPrefab != null) return slot;

        return null;
    }

    /// <summary>
    /// Copies the plant a native crop grows as and points it at the created food's grower
    /// </summary>
    /// <returns> The plant, or null if it has no SpawnResource </returns>
    private static GameObject? BuildPlantedPrefab(
        GameObject template,
        IdentifiableType food,
        ResourceGrowerDefinition grower,
        string variant)
    {
        var clone = Prefabs.Create($"{food.name}{variant}", template);

        var spawner = clone.GetComponent<SpawnResource>()
                   ?? clone.GetComponentInChildren<SpawnResource>(includeInactive: true);

        if (spawner == null)
        {
            LogWarning($"Prism: Crop for '{food.name}' has no SpawnResource, it was left unplantable");
            UObject.Destroy(clone);
            return null;
        }

        spawner._resourceGrowerDefinition = grower;
        return clone;
    }

    /// <summary>
    /// Copies a native crop's grower, retargets it at the created food, and registers it
    /// </summary>
    private static ResourceGrowerDefinition BuildGrower(ResourceGrowerDefinition template, IdentifiableType food)
    {
        var grower = UObject.Instantiate(template);
        grower.hideFlags = HideFlags.DontUnloadUnusedAsset;
        grower.name = food.name + "Grower";
        grower._primaryResourceType = food;
        
        var nativePlant = template._prefab;
        var planted = nativePlant == null ? null : BuildPlantedPrefab(nativePlant, food, grower, "Crop");
        if (planted != null) grower._prefab = planted;
        
        grower._persistenceId = $"{BuildInfo.Name}.{food.ReferenceId.AfterLast('.')}.Grower";

        grower._resources = new[]
        {
            new ResourceSpawnerDefinition.WeightedResourceEntry
            {
                ResourceIdentifiableType = food,
                Weight = 1f,
                MinimumAmount = 1
            }
        }.ToIl2CppArray();

        RegisterGrower(grower);
        return grower;
    }

    /// <summary>
    /// Registers a grower
    /// </summary>
    private static void RegisterGrower(ResourceGrowerDefinition grower)
    {
        try
        {
            var config = autoSaveDirector._configuration;
            if (config?.ResourceGrowers?.items != null && !config.ResourceGrowers.items.Contains(grower))
                config.ResourceGrowers.items.Add(grower);

            SaveTables.Register(SaveTables.Translation?._resourceGrowerTranslation, grower._persistenceId, grower);
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to register garden grower '{grower.name}'", ex);
        }
    }

    /// <summary>
    /// Refreshes every grower registration
    /// </summary>
    internal static void RefreshGrowers()
    {
        foreach (var grower in Growers.Values.ToList())
            RegisterGrower(grower);
    }
}

/// <summary>
/// Adds registered crops to each garden plot as it awakes
/// </summary>
[HarmonyPatch(typeof(GardenCatcher), nameof(GardenCatcher.Awake))]
internal static class GardenCatcherPatch
{
    public static void Postfix(GardenCatcher __instance)
    {
        if (!StarlightEntryPoint.prismEnableGardens) return;

        try
        {
            PrismGardenCreatorV02.ApplyTo(__instance);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to add created crops to a garden plot", ex);
        }
    }
}