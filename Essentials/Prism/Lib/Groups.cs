using System;

namespace Starlight.Prism.Lib;

/// <summary>
/// The game's groups
/// </summary>
public static class Groups
{
    public static class Names
    {
        /// <summary>
        /// Every IdentifiableType
        /// </summary>
        public const string AllIdentifiableTypes = "IdentifiableTypesGroup";

        /// <summary>
        /// All plorts
        /// </summary>
        public const string Plort = "PlortGroup";
        
        /// <summary>
        /// Every edible plort
        /// </summary>
        /// <remarks> Excludes gold & shadow plorts </remarks>
        public const string EdiblePlortFood = "EdiblePlortFoodGroup";
        
        /// <summary>
        /// Every plort the explorer drone can pick up
        /// </summary>
        public const string PlortDroneExplorer = "PlortGroupDroneExplorer";

        /// <summary>
        /// General slime food
        /// </summary>
        /// <remarks>
        /// This just indicates something is edible,
        /// not that all slimes can eat it 
        /// </remarks>
        public const string Food = "FoodGroup";
        
        /// <summary>
        /// Specific fruit slime food
        /// </summary>
        public const string Fruit = "FruitGroup";
        
        /// <summary>
        /// Specific vegetable slime food
        /// </summary>
        public const string Veggie = "VeggieGroup";
        
        /// <summary>
        /// Specific meat slime food
        /// </summary>
        /// <remarks>
        /// Hen, BriarHen, StonyHen, SeaHen, ThunderHen, PaintedHen, CandiedHen, Rooster, ElderHen, ElderRooster, UnstableMeat
        /// </remarks>
        public const string Meat = "MeatGroup";
        
        // This is not implemented
        //public const string GingerFood = "GingerFoodGroup";

        /// <summary>
        /// Which slot the quantum cloud stores a food in
        /// </summary>
        public const string FruitQuantumCloud  = "FruitQuantumCloudGroup";
        /// <inheritdoc cref="FruitQuantumCloud"/>
        public const string VeggieQuantumCloud = "VeggieQuantumCloudGroup";

        /// <summary>
        /// Grown "common" chickens
        /// </summary>
        /// <remarks>
        /// BriarHen, StonyHen, SeaHen
        /// </remarks>
        public const string CommonHen = "CommonHenGroup";

        /// <summary>
        /// Grown "rare" chickens
        /// </summary>
        /// <remarks>
        /// ThunderHen, PaintedHen, CandiedHen
        /// </remarks>
        public const string RareHen = "RareHenGroup";

        /// <summary>
        /// Baby chickens
        /// </summary>
        /// <remarks>
        /// Chick, BriarChick, StonyChick, SeaChick, ThunderChick, PaintedChick, CandiedChick
        /// </remarks>
        public const string Chick = "ChickGroup";

        /// <summary>
        /// All grown and baby chickens
        /// </summary>
        public const string Chickens = "ChickenAndChickGroup";

        /// <summary>
        /// All vacuumable base (non-largo) slimes
        /// </summary>
        /// <remarks> This excludes gold, shadow & lucky (?) </remarks>
        public const string VaccableBaseSlime = "VaccableBaseSlimeGroup";
        
        /// <summary>
        /// Every base (non-largo) slime
        /// </summary>
        public const string SmallSlime = "SmallSlimeGroup";
        
        /// <summary>
        /// Every slime edible for a tarr slime
        /// </summary>
        public const string EdibleSlime = "EdibleSlimeGroup";
        
        /// <summary>
        /// All slimes that dissolve when in shallow water
        /// </summary>
        /// <remarks> Not the slime sea, general water (?) </remarks>
        public const string SlimesSinkInShallowWater = "SlimesSinkInShallowWaterGroup";
    }
    
    private static readonly Dictionary<string, IdentifiableTypeGroup> Cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Every currently loaded <see cref="IdentifiableTypeGroup"/>
    /// </summary>
    public static Dictionary<string, IdentifiableTypeGroup> AllGroups
    {
        get
        {
            var result = new Dictionary<string, IdentifiableTypeGroup>(StringComparer.Ordinal);

            foreach (var group in GetAll<IdentifiableTypeGroup>())
            {
                if (group == null || string.IsNullOrEmpty(group.name)) continue;
                result[group.name] = group;
                Cache[group.name] = group;
            }

            return result;
        }
    }

    /// <summary>
    /// Finds an <see cref="IdentifiableTypeGroup"/> by name
    /// </summary>
    /// <returns> The group, or null if the game has no group with that name </returns>
    public static IdentifiableTypeGroup? Find(string groupName)
    {
        if (Cache.TryGetValue(groupName, out var cached) && cached != null) return cached;

        var found = Get<IdentifiableTypeGroup>(groupName);
        if (found != null) Cache[groupName] = found;

        return found;
    }

    /// <summary>
    /// Adds a <paramref name="type"/> to a named <see cref="IdentifiableTypeGroup"/>
    /// </summary>
    /// <returns> True if the type is a member of the group(s) </returns>
    public static bool Add(IdentifiableType type, string groupName)
    {
        var group = Find(groupName);
        if (group == null)
        {
            LogWarning($"Prism: Can't add '{type.name}' to group '{groupName}': no such group");
            return false;
        }

        return Add(type, group);
    }

    /// <summary>
    /// Adds a <paramref name="type"/> to an <see cref="IdentifiableTypeGroup"/>
    /// </summary>
    /// <returns> True if the type is a member of the group </returns>
    public static bool Add(IdentifiableType type, IdentifiableTypeGroup group)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(group);

        if (!group.IsMember(type))
            group._memberTypes.Add(type);

        var runtime = group.GetRuntimeObject();
        if (runtime != null && !runtime.IsMember(type))
            runtime._memberTypes.Add(type);

        return true;
    }

    /// <summary>
    /// Adds the <paramref name="type"/> to several <see cref="IdentifiableTypeGroup"/>s at once
    /// </summary>
    /// <inheritdoc cref="Add(IdentifiableType,string)"/>
    public static void AddMultiple(IdentifiableType type, params string[] groupNames)
    {
        foreach (var groupName in groupNames)
            Add(type, groupName);
    }

    /// <inheritdoc cref="AddMultiple(IdentifiableType,string[])"/>
    public static void AddMultiple(IdentifiableType type, params IdentifiableTypeGroup[] groups)
    {
        foreach (var group in groups)
            Add(type, group);
    }

    /// <summary>
    /// Removes the <paramref name="type"/> out of a named <see cref="IdentifiableTypeGroup"/>
    /// </summary>
    /// <returns> True if the type is not a member of the group(s) </returns>
    public static bool Remove(IdentifiableType type, string groupName)
    {
        ArgumentNullException.ThrowIfNull(type);

        var group = Find(groupName);
        if (group == null) return true;

        return Remove(type, group);
    }

    /// <summary>
    /// Removes the <paramref name="type"/> out of an <see cref="IdentifiableTypeGroup"/>
    /// </summary>
    /// <returns> True if the type is not a member of the group </returns>
    public static bool Remove(IdentifiableType type, IdentifiableTypeGroup group)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(group);

        if (group.IsMember(type))
            group._memberTypes.Remove(type);

        var runtime = group.GetRuntimeObject();
        if (runtime != null && runtime.IsMember(type))
            runtime._memberTypes.Remove(type);

        return !group.IsMember(type);
    }

    /// <summary>
    /// Removes the <paramref name="type"/> from several <see cref="IdentifiableTypeGroup"/>s at once
    /// </summary>
    /// <inheritdoc cref="Remove(IdentifiableType,string)"/>
    public static void RemoveAll(IdentifiableType type, params string[] groupNames)
    {
        foreach (var groupName in groupNames)
            Remove(type, groupName);
    }

    /// <inheritdoc cref="RemoveAll(IdentifiableType,string[])"/>
    public static void RemoveAll(IdentifiableType type, params IdentifiableTypeGroup[] groups)
    {
        foreach (var group in groups)
            Remove(type, group);
    }

    /// <summary>
    /// Adds the <paramref name="type"/> into every group the <paramref name="template"/> is in
    /// </summary>
    /// <param name="type"> The new content </param>
    /// <param name="template"> The native content it was copied from </param>
    /// <returns> How many groups were added (excludes already existing ones) </returns>
    public static int CopyGroups(IdentifiableType type, IdentifiableType template)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(template);

        var joined = 0;

        foreach (var group in GetAll<IdentifiableTypeGroup>())
        {
            if (group == null || string.IsNullOrEmpty(group.name)) continue;
            if (!group.IsMember(template) || group.IsMember(type)) continue;

            if (Add(type, group)) joined++;
        }

        return joined;
    }
}