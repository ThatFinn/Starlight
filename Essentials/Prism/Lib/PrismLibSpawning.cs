using System;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Starlight.Prism.Data.Spawning;

namespace Starlight.Prism.Lib;
/// <summary>
/// A library of helper functions for dealing with spawning
/// </summary>
public static class PrismLibSpawning
{

    //internal static List<Action<SpawnResource>> onResourceGrowerAwake = new();
    //public static Dictionary<string, GameObject> gordos = new Dictionary<string, GameObject>();
    //public static Dictionary<string, ResourceGrowerDefinition> resourceGrowerDefinitions = new();

    internal static readonly List<Action<DirectedActorSpawner>> ExecuteOnSpawnerAwake = new ();

    /// <summary>
    /// Makes an Identifiable able to spawn.
    /// </summary>
    /// <param name="ident">The object's type</param>
    /// <param name="activeTime">The time of day the object will spawn in.</param>
    /// <param name="locations">Which zones to spawn in.</param>
    /// <param name="weight">The chance to spawn.</param>
    /// <param name="spawnerTargets">Which spawner type to inject into.</param>
    public static void MakeSpawnable(IdentifiableType ident, PrismSpawnerActiveTime activeTime,
        PrismSpawnLocations[] locations, float weight, PrismSpawnerType spawnerTargets) =>
        MakeSpawnable(ident, activeTime, locations, weight, spawnerTargets,
            null, Array.Empty<string>());

    /// <summary>
    /// Makes an Identifiable able to spawn.
    /// </summary>
    /// <param name="ident">The object's type</param>
    /// <param name="activeTime">The time of day the object will spawn in.</param>
    /// <param name="zones">Which zones to spawn in.</param>
    /// <param name="weight">The chance to spawn.</param>
    /// <param name="spawnerTargets">Which spawner type to inject into.</param>
    /// <param name="requiredConditions">Required conditions on when not to inject into a spawner.</param>
    public static void MakeSpawnable(IdentifiableType ident, PrismSpawnerActiveTime activeTime,
        PrismSpawnLocations[] zones, float weight, PrismSpawnerType spawnerTargets,
        PrismSpawnConditions[] requiredConditions) =>
        MakeSpawnable(ident, activeTime, zones, weight, spawnerTargets, requiredConditions,
            Array.Empty<string>());

    /// <summary>
    /// Makes an Identifiable able to spawn.
    /// </summary>
    /// <param name="ident">The object's type</param>
    /// <param name="activeTime">The time of day the object will spawn in.</param>
    /// <param name="locations">Which zones to spawn in.</param>
    /// <param name="weight">The chance to spawn.</param>
    /// <param name="spawnerType">Which spawner type to inject into.</param>
    /// <param name="requiredConditions">Required conditions on when not to inject into a spawner.</param>
    /// <param name="customZoneSceneNames">An array of custom scene names.</param>
    /// <exception cref="InvalidOperationException">Occurs when an invalid SpawningMode is used. Should never happen.</exception>
    public static void MakeSpawnable(IdentifiableType ident, PrismSpawnerActiveTime activeTime,
        PrismSpawnLocations[] locations, float weight, PrismSpawnerType spawnerType,
        PrismSpawnConditions[] requiredConditions, params string[] customZoneSceneNames)
    {

        if (locations == null||locations.Length==0) return;
        
        var list = GetSceneNamesFromSpawnerZones(locations);

        foreach (var custom in customZoneSceneNames)
            list.Add(custom);

        ExecuteOnSpawnerAwake.Add(spawner =>
        {
            if (ContainsZoneName(spawner.gameObject.scene.name, list))
            {
                if (spawnerType==PrismSpawnerType.Slime)
                    if (spawner.TryCast<DirectedSlimeSpawner>() != null)
                        AddSpawningSettings(spawner, ident, weight, activeTime, requiredConditions);

                if (spawnerType==PrismSpawnerType.Animal)
                    if (spawner.TryCast<DirectedAnimalSpawner>() != null)
                        AddSpawningSettings(spawner, ident, weight, activeTime, requiredConditions);
            }
        });

    }

    private static List<string> GetSceneNamesFromSpawnerZones(PrismSpawnLocations[] locations)
    {
        List<string> names = new();
        if (locations == null||locations.Length==0) return names;

        if (locations.Contains(PrismSpawnLocations.RainbowFields)) names.Add("zoneFields");
        if (locations.Contains(PrismSpawnLocations.EmberValley)) names.Add("zoneGorge");
        if (locations.Contains(PrismSpawnLocations.StarlightStand)) names.Add("zoneStrand");
        if (locations.Contains(PrismSpawnLocations.PowderfallBluffs)) names.Add("zoneBluffs");
        if (locations.Contains(PrismSpawnLocations.LabyrinthWaterworks)) names.Add("LabStrand");
        if (locations.Contains(PrismSpawnLocations.LabyrinthLavadepths)) names.Add("LabValley");
        if (locations.Contains(PrismSpawnLocations.LabyrinthDreamland)) names.Add("Dreamland");
        if (locations.Contains(PrismSpawnLocations.LabyrinthHub)) names.Add("Hub");
        if (locations.Contains(PrismSpawnLocations.LabyrinthTerrarium)) names.Add("zoneLabyrinthTerrarium");
        return names;
    }

    internal static List<string> GetDefNamesFromSpawnerZones(PrismSpawnLocations[] locations)
    {
        List<string> names = new();
        if (locations == null||locations.Length==0) return names;

        if (locations.Contains(PrismSpawnLocations.RainbowFields)) names.Add("fields");
        if (locations.Contains(PrismSpawnLocations.EmberValley)) names.Add("gorge");
        if (locations.Contains(PrismSpawnLocations.StarlightStand)) names.Add("strand");
        if (locations.Contains(PrismSpawnLocations.PowderfallBluffs)) names.Add("bluffs");
        if (locations.Contains(PrismSpawnLocations.LabyrinthWaterworks)) names.Add("waterworks");
        if (locations.Contains(PrismSpawnLocations.LabyrinthLavadepths)) names.Add("lavadepths");
        if (locations.Contains(PrismSpawnLocations.LabyrinthDreamland)) names.Add("dream");
        if (locations.Contains(PrismSpawnLocations.LabyrinthHub)) names.Add("hub");

        return names;
    }

    internal static bool ContainsZoneName(string sceneName, List<string> zoneNames)
    {
        foreach (var zoneName in zoneNames)
            if (sceneName.Contains(zoneName))
                return true;

        return false;
    }

    internal static bool IsInZone(string[] zoneNames)
    {
        if (sceneContext == null) return false;
        if (sceneContext.Player == null) return false;
        var tracker = sceneContext.Player.GetComponent<PlayerZoneTracker>();
        if (tracker == null) return false;
        var zone = tracker.GetCurrentZone();
        if (zone == null) return false;
        foreach (var name in zoneNames)
        {
            if (zone.name.ToLower().Contains(name.ToLower())) return true;
        }

        return false;
    }
    /// <summary>
    /// Adds spawning settings to a spawner
    /// </summary>
    /// <param name="spawner">The spawner to add the settings to</param>
    /// <param name="ident">The identifiable type to spawn</param>
    /// <param name="weight">The weight of the spawn</param>
    /// <param name="activeTime">The time of day the spawn is active</param>
    /// <param name="requiredConditions">The required conditions for the spawn</param>
    public static void AddSpawningSettings(DirectedActorSpawner spawner, IdentifiableType ident, float weight,
        PrismSpawnerActiveTime activeTime, PrismSpawnConditions[] requiredConditions)
    {

        DirectedActorSpawner.SpawnConstraint constraint = null;
        float consWeight = weight; // only used if a new constraint needs to be made.
        var timeWindow = new DirectedActorSpawner.TimeWindow();
        switch (activeTime)
        {
            case PrismSpawnerActiveTime.DuringDay:
                timeWindow.TimeMode = DirectedActorSpawner.TimeMode.DAY;
                break;
            case PrismSpawnerActiveTime.DuringNight:
                timeWindow.TimeMode = DirectedActorSpawner.TimeMode.NIGHT;
                break;
            default:
                timeWindow.TimeMode = DirectedActorSpawner.TimeMode.ANY;
                break;
                
        }
        foreach (var cons in spawner.Constraints)
        {
            //Ignore duplicates that way
            var allConditions = Enum.GetValues<PrismSpawnConditions>();
            if (requiredConditions != null)
                if(requiredConditions.Length!=0)
                    foreach (var con in allConditions)
                    {
                        if (requiredConditions.Contains(con))
                        {
                            switch (con)
                            {
                                case PrismSpawnConditions.RequiresAnglerSlime:
                                    bool hasAngler = false;
                                    foreach (var m in cons.Slimeset.Members)
                                    {
                                        if (m.IdentType.name.ToLower() == "angler")
                                        {
                                            hasAngler = true;
                                            break;
                                        }
                                    }

                                    if (!hasAngler)
                                        return;

                                    break;
                                case PrismSpawnConditions.RequiresAnglerLargo:
                                    bool hasAnglerL = false;
                                    foreach (var m in cons.Slimeset.Members)
                                    {
                                        if (m.IdentType.name.ToLower().Contains("angler") &&
                                            m.IdentType.Cast<SlimeDefinition>().IsLargo)
                                        {
                                            hasAnglerL = true;
                                            break;
                                        }
                                    }

                                    if (!hasAnglerL)
                                        return;

                                    break;

                                case PrismSpawnConditions.RequiresLargosOnly:
                                    bool hasNonLargo = false;
                                    foreach (var m in cons.Slimeset.Members)
                                    {
                                        if (!m.IdentType.Cast<SlimeDefinition>().IsLargo)
                                        {
                                            hasNonLargo = true;
                                            break;
                                        }
                                    }

                                    if (hasNonLargo)
                                        return;

                                    break;

                                case PrismSpawnConditions.RequiresOnlyPinkOrModdedSlimes:
                                    bool hasNonPink = false;
                                    foreach (var m in cons.Slimeset.Members)
                                    {
                                        if (m.IdentType.name.ToLower() != "pink" &&
                                            !SaveRegistry.Contains(m.IdentType))
                                        {
                                            hasNonPink = true;
                                            break;
                                        }
                                    }

                                    if (hasNonPink)
                                        return;

                                    break;
                            }
                        }
                    }

            //consWeight = ((consWeight / 2) + (cons.Weight / 2)) * 2;
            /*if (cons.Window.TimeMode == timeWindow.TimeMode)
            {
                constraint = cons;
                break;
            }*/
        }

        if (constraint == null)
        {
            constraint = new DirectedActorSpawner.SpawnConstraint();
            constraint.Window = timeWindow;
            constraint.Feral = false;
            constraint.Weight = consWeight;
            constraint.Slimeset = new SlimeSet();
            constraint.Slimeset.Members = new Il2CppReferenceArray<SlimeSet.Member>(0);
            spawner.Constraints = spawner.Constraints.AddToNew(constraint);
        }

        var member = new SlimeSet.Member()
        {
            _prefab = ident.prefab,
            IdentType = ident,
            Weight = weight
        };
        constraint.Slimeset.Members = constraint.Slimeset.Members.AddToNew(member);
    }

    /* public static void SetResourceGrower(
         IdentifiableType ident,
         float chance,
         int minAmount,
         string spawnerObjectName,
         SpawnLocations zones,
         params string[] blacklistInSpawnerName)
     {
         onResourceGrowerAwake.Add(spawner =>
         {
             var id = spawnerObjectName;
             var i = 0;
             foreach (var blacklist in blacklistInSpawnerName)
             {
                 id += $"-blacklist{i}_{blacklist}";
                 i++;
             }

             id += $"-zones_{(int)zones}";
             var scenes = GetSceneNamesFromSpawnerZones(zones);

             bool inZone = scenes.Any(scene => spawner.gameObject.scene.name.Contains(scene));

             if (!inZone) return;

             if (!spawner.gameObject.name.Contains(spawnerObjectName)) return;

             bool containsBlacklistedWord =
                 blacklistInSpawnerName.Any(name => spawner.gameObject.name.Contains(name));

             if (containsBlacklistedWord) return;

             resourceGrowerDefinitions.TryGetValue(id, out var spawnerDefinition);

             if (spawnerDefinition == null)
             {
                 spawnerDefinition = Object.Instantiate(spawner.ResourceGrowerDefinition);
                 spawnerDefinition.name = id;
                 Object.DontDestroyOnLoad(spawnerDefinition);
             }

             var _ = spawnerDefinition._resources.FirstOrDefault(x => x.ResourceIdentifiableType.name == ident.name);
             if (_ == null)
                 spawnerDefinition._resources = spawnerDefinition._resources.AddToNew(
                     new ResourceSpawnerDefinition.WeightedResourceEntry()
                     {
                         MinimumAmount = minAmount,
                         ResourceIdentifiableType = ident,
                         Weight = chance
                     });

             spawner._resourceGrowerDefinition = spawnerDefinition;

             resourceGrowerDefinitions.TryAdd(id, spawnerDefinition);
         });
     }





     public static void CreateGordoSpawnLocation(IdentifiableType ident, SpawnLocations zone, Vector3 position, Vector3 rotation, string cottonIdentifier)
     {
         var sceneName = GetSceneNamesFromSpawnerZones(zone)[0];

         if (!onSceneLoaded.TryGetValue(sceneName, out var actions))
         {
             actions = new List<Action>();
             onSceneLoaded.Add(sceneName, actions);
         }

         actions.Add(() =>
         {
             if (gordos.TryGetValue(cottonIdentifier, out var obj))
                 if (obj == null)
                     gordos[cottonIdentifier] =
                         InstantiationHelpers.InstantiateDynamic(ident.prefab, position, Quaternion.Euler(rotation));
                 else
                     return;
             else
                 gordos.Add(cottonIdentifier,
                     InstantiationHelpers.InstantiateDynamic(ident.prefab, position, Quaternion.Euler(rotation)));

         });
     }
 */
}