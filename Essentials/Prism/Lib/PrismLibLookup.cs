using Starlight.Prism.Data.Native;
using Starlight.Prism.Wrappers;

namespace Starlight.Prism.Lib;
/// <summary>
/// A library of helper functions for looking up IdentifiableTypeGroups and checking if an IdentifiableType is in a group
/// </summary>
public static class PrismLibLookup
{
    
    
    /// <summary>
    /// The group of all identifiable types
    /// </summary>
    public static IdentifiableTypeGroup identifiableGroup => LookupEUtil.allIdentifiableTypeGroups["IdentifiableTypesGroup"]; 
    /// <summary>
    /// The group of all vaccable types
    /// </summary>
    public static IdentifiableTypeGroup vaccableGroup => LookupEUtil.allIdentifiableTypeGroups["VaccableNonLiquids"]; 
    /// <summary>
    /// The group of all toy types
    /// </summary>
    public static IdentifiableTypeGroup toyGroup => LookupEUtil.allIdentifiableTypeGroups["ToyGroup"]; 
    /// <summary>
    /// The group of all gadget types
    /// </summary>
    public static IdentifiableTypeGroup gadgetGroup => LookupEUtil.allIdentifiableTypeGroups["GadgetGroup"]; 
    /// <summary>
    /// The group of all liquid types
    /// </summary>
    public static IdentifiableTypeGroup liquidGroup => LookupEUtil.allIdentifiableTypeGroups["LiquidGroup"]; 
    /// <summary>
    /// The group of all slime types
    /// </summary>
    public static IdentifiableTypeGroup slimeGroup => LookupEUtil.allIdentifiableTypeGroups["SlimesGroup"]; 
    /// <summary>
    /// The group of all largo types
    /// </summary>
    public static IdentifiableTypeGroup largoGroup => LookupEUtil.allIdentifiableTypeGroups["LargoGroup"]; 
    /// <summary>
    /// The group of all plort types
    /// </summary>
    public static IdentifiableTypeGroup plortGroup => LookupEUtil.allIdentifiableTypeGroups["PlortGroup"]; 
    /// <summary>
    /// The group of all food types
    /// </summary>
    public static IdentifiableTypeGroup foodGroup => LookupEUtil.allIdentifiableTypeGroups["FoodGroup"]; 
    /// <summary>
    /// The group of all meat food types
    /// </summary>
    public static IdentifiableTypeGroup meatFoodGroup => LookupEUtil.allIdentifiableTypeGroups["MeatGroup"]; 
    /// <summary>
    /// The group of all veggie food types
    /// </summary>
    public static IdentifiableTypeGroup veggieFoodGroup => LookupEUtil.allIdentifiableTypeGroups["VeggieGroup"]; 
    /// <summary>
    /// The group of all fruit food types
    /// </summary>
    public static IdentifiableTypeGroup fruitFoodGroup => LookupEUtil.allIdentifiableTypeGroups["FruitGroup"]; 
    /// <summary>
    /// The group of all nectar food types
    /// </summary>
    public static IdentifiableTypeGroup nectarFoodGroup => LookupEUtil.allIdentifiableTypeGroups["NectarFoodGroup"]; 
    /// <summary>
    /// The group of all chick food types
    /// </summary>
    public static IdentifiableTypeGroup chickFoodGroup => LookupEUtil.allIdentifiableTypeGroups["ChickGroup"]; 
    /// <summary>
    /// The group of all craft types
    /// </summary>
    public static IdentifiableTypeGroup craftGroup => LookupEUtil.allIdentifiableTypeGroups["CraftGroup"]; 
    
    
    
    /// <summary>
    /// Adds an IdentifiableType to a group
    /// </summary>
    /// <param name="type">The type to add</param>
    /// <param name="groupName">The name of the group to add to</param>
    public static void Prism_AddToGroup(this IdentifiableType type, string groupName)
    {
        var group = LookupEUtil.allIdentifiableTypeGroups[groupName];
        if (group.IsMember(type)) return;
        group._memberTypes.Add(type);
        if (group.GetRuntimeObject().IsMember(type)) return;
        group.GetRuntimeObject()._memberTypes.Add(type);
    }
    /// <summary>
    /// Adds an IdentifiableTypeGroup to a group
    /// </summary>
    /// <param name="group">The group to add</param>
    /// <param name="groupName">The name of the group to add to</param>
    public static void Prism_AddToGroup(this IdentifiableTypeGroup group, string groupName)
    {
        var group2 = LookupEUtil.allIdentifiableTypeGroups[groupName];
        if (group2._memberGroups.Contains(group)) return;
        group2._memberGroups.Add(group);
        if (group2.GetRuntimeObject()._memberGroups.Contains(group)) return;
        group2.GetRuntimeObject()._memberGroups.Add(group);
    }
    /// <summary>
    /// Adds a PrismIdentifiableTypeGroup to a group
    /// </summary>
    /// <param name="group">The group to add</param>
    /// <param name="groupName">The name of the group to add to</param>
    public static void AddToGroup(this PrismIdentifiableTypeGroup group, string groupName)
    {
        var group2 = LookupEUtil.allIdentifiableTypeGroups[groupName];
        if (group2._memberGroups.Contains(group.Group)) return;
        group2._memberGroups.Add(group.Group);
        if (group2.GetRuntimeObject()._memberGroups.Contains(group.Group)) return;
        group2.GetRuntimeObject()._memberGroups.Add(group.Group);
    }
    /// <summary>
    /// Checks if an IdentifiableType is in a group
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <param name="groupName">The name of the group to check</param>
    /// <returns>Whether or not the type is in the group</returns>
    public static bool Prism_IsInImmediateGroup(this IdentifiableType type, string groupName)
    {
        var group = LookupEUtil.allIdentifiableTypeGroups[groupName];
        if (group == null) return false;
        return group._memberTypes.Contains(type);
    }
    /// <summary>
    /// Checks if a PrismSlime is in a group
    /// </summary>
    /// <param name="prismSlime">The slime to check</param>
    /// <param name="groupName">The name of the group to check</param>
    /// <returns>Whether or not the slime is in the group</returns>
    public static bool IsInImmediateGroup(this PrismSlime prismSlime, string groupName)
    {
        var group = LookupEUtil.allIdentifiableTypeGroups[groupName];
        if (group == null) return false;
        return group._memberTypes.Contains(prismSlime.SlimeDefinition);
    }
    /// <summary>
    /// Checks if a PrismPlort is in a group
    /// </summary>
    /// <param name="prismPlort">The plort to check</param>
    /// <param name="groupName">The name of the group to check</param>
    /// <returns>Whether or not the plort is in the group</returns>
    public static bool IsInImmediateGroup(this PrismPlort prismPlort, string groupName)
    {
        var group = LookupEUtil.allIdentifiableTypeGroups[groupName];
        if (group == null) return false;
        return group._memberTypes.Contains(prismPlort.IdentifiableType);
    }
    /// <summary>
    /// Checks if a largo combination exists
    /// It checks both ways
    /// </summary>
    /// <param name="slime1">The first slime</param>
    /// <param name="slime2">The second slime</param>
    /// <returns>Whether or not the largo combination exists</returns>
    public static bool DoesLargoComboExist(this PrismBaseSlime slime1, PrismBaseSlime slime2)
    {
        if (slime1 == null || slime2 == null) return false;
        var first = slime1.GetSlimeDefinition();
        var second = slime2.GetSlimeDefinition();
        if (first != null && second != null && Starlight.Prism.Creators.PrismLargoCreatorV02.Find(first, second) != null)
            return true;

        try
        {
            if (gameContext.SlimeDefinitions.GetLargoByBaseSlimes(slime1, slime2) != null)
                return true;
        } catch { }
        try
        {
            if (gameContext.SlimeDefinitions.GetLargoByBaseSlimes(slime2, slime1) != null)
                return true;
        } catch { }
        return false;
    }
    /// <summary>
    /// Gets the reference ID of a native base slime
    /// </summary>
    /// <param name="nativeBaseSlime">The native base slime to get the reference ID of</param>
    /// <returns>The reference ID of the native base slime</returns>
    public static string GetReferenceID(this PrismNativeBaseSlime nativeBaseSlime)
    {
        if (RefIDTranslationPrismNativeBaseSlime.ContainsKey(nativeBaseSlime))
            return RefIDTranslationPrismNativeBaseSlime[nativeBaseSlime];
        return null;
    }
    /// <summary>
    /// Gets the reference ID of a native plort
    /// </summary>
    /// <param name="nativePlort">The native plort to get the reference ID of</param>
    /// <returns>The reference ID of the native plort</returns>
    public static string GetReferenceID(this PrismNativePlort nativePlort)
    {
        return RefIDTranslationPrismNativePlort.GetValueOrDefault(nativePlort);
    }
    /// <summary>
    /// Gets the reference ID of a native liquid
    /// </summary>
    /// <param name="nativeLiquid">The native liquid to get the reference ID of</param>
    /// <returns>The reference ID of the native liquid</returns>
    public static string GetReferenceID(this PrismNativeLiquid nativeLiquid)
    {
        return RefIDTranslationPrismNativeLiquid.GetValueOrDefault(nativeLiquid);
    }
    internal static readonly Dictionary<PrismNativeBaseSlime, string> RefIDTranslationPrismNativeBaseSlime =
        new Dictionary<PrismNativeBaseSlime, string>()
        {
            { PrismNativeBaseSlime.Pink, "SlimeDefinition.Pink" },
            { PrismNativeBaseSlime.Cotton, "SlimeDefinition.Cotton" },
            { PrismNativeBaseSlime.Phosphor, "SlimeDefinition.Phosphor" },
            { PrismNativeBaseSlime.Tabby, "SlimeDefinition.Tabby" },
            { PrismNativeBaseSlime.Angler, "SlimeDefinition.Angler" },
            { PrismNativeBaseSlime.Rock, "SlimeDefinition.Rock" },
            { PrismNativeBaseSlime.Honey, "SlimeDefinition.Honey" },
            { PrismNativeBaseSlime.Boom, "SlimeDefinition.Boom" },
            { PrismNativeBaseSlime.Puddle, "SlimeDefinition.Puddle" },
            { PrismNativeBaseSlime.Fire, "SlimeDefinition.Fire" },
            { PrismNativeBaseSlime.Batty, "SlimeDefinition.Batty" },
            { PrismNativeBaseSlime.Crystal, "SlimeDefinition.Crystal" },
            { PrismNativeBaseSlime.Hunter, "SlimeDefinition.Hunter" },
            { PrismNativeBaseSlime.Flutter, "SlimeDefinition.Flutter" },
            { PrismNativeBaseSlime.Ringtail, "SlimeDefinition.Ringtail" },
            { PrismNativeBaseSlime.Saber, "SlimeDefinition.Saber" },
            { PrismNativeBaseSlime.Yolky, "SlimeDefinition.Yolky" },
            { PrismNativeBaseSlime.Tangle, "SlimeDefinition.Tangle" },
            { PrismNativeBaseSlime.Dervish, "SlimeDefinition.Dervish" },
            { PrismNativeBaseSlime.Twin, "SlimeDefinition.Twin" },
            { PrismNativeBaseSlime.Sloomber, "SlimeDefinition.Sloomber" },
            { PrismNativeBaseSlime.Shadow, "SlimeDefinition.Shadow" },
            { PrismNativeBaseSlime.Hyper, "SlimeDefinition.Hyper" },
            { PrismNativeBaseSlime.Gold, "SlimeDefinition.Gold" },
            { PrismNativeBaseSlime.Lucky, "SlimeDefinition.Lucky" },
            { PrismNativeBaseSlime.Tarr, "SlimeDefinition.Tarr" },
        };

    internal static readonly Dictionary<PrismNativePlort, string> RefIDTranslationPrismNativePlort =
        new Dictionary<PrismNativePlort, string>()
        {
            { PrismNativePlort.Pink, "IdentifiableType.PinkPlort" },
            { PrismNativePlort.Cotton, "IdentifiableType.CottonPlort" },
            { PrismNativePlort.Phosphor, "IdentifiableType.PhosphorPlort" },
            { PrismNativePlort.Tabby, "IdentifiableType.TabbyPlort" },
            { PrismNativePlort.Angler, "IdentifiableType.AnglerPlort" },
            { PrismNativePlort.Rock, "IdentifiableType.RockPlort" },
            { PrismNativePlort.Honey, "IdentifiableType.HoneyPlort" },
            { PrismNativePlort.Boom, "IdentifiableType.BoomPlort" },
            { PrismNativePlort.Puddle, "IdentifiableType.PuddlePlort" },
            { PrismNativePlort.Fire, "IdentifiableType.FirePlort" },
            { PrismNativePlort.Batty, "IdentifiableType.BattyPlort" },
            { PrismNativePlort.Crystal, "IdentifiableType.CrystalPlort" },
            { PrismNativePlort.Hunter, "IdentifiableType.HunterPlort" },
            { PrismNativePlort.Flutter, "IdentifiableType.FlutterPlort" },
            { PrismNativePlort.Ringtail, "IdentifiableType.RingtailPlort" },
            { PrismNativePlort.Saber, "IdentifiableType.SaberPlort" },
            { PrismNativePlort.Yolky, "IdentifiableType.YolkyPlort" },
            { PrismNativePlort.Tangle, "IdentifiableType.TanglePlort" },
            { PrismNativePlort.Dervish, "IdentifiableType.DervishPlort" },
            { PrismNativePlort.Twin, "IdentifiableType.TwinPlort" },
            { PrismNativePlort.Sloomber, "IdentifiableType.SloomberPlort" },
            { PrismNativePlort.Shadow, "IdentifiableType.ShadowPlort" },
            { PrismNativePlort.Prisma, "IdentifiableType.StablePlort" },
            { PrismNativePlort.Hyper, "IdentifiableType.HyperPlort" },
            { PrismNativePlort.Gold, "IdentifiableType.GoldPlort" },
            { PrismNativePlort.Unstable, "IdentifiableType.UnstablePlort" }
        };
    
    internal static readonly Dictionary<PrismNativeLiquid, string> RefIDTranslationPrismNativeLiquid =
        new Dictionary<PrismNativeLiquid, string>()
        {
            { PrismNativeLiquid.Water, "LiquidDefinition.Water" }
        };
}