using Starlight.Prism.Lib;

namespace Starlight.Prism.Data.Native;

/// <summary>
/// The base game's chickens
/// </summary>
public static class NativeChicken
{
    /// <summary>
    /// The ordinary grown chicken
    /// </summary>
    public static IdentifiableType? CommonHen() => FromGroup(Groups.Names.CommonHen, "Hen");
    
    /// <summary>
    /// The rare grown chicken
    /// </summary>
    // public static IdentifiableType? RareHen() => FromGroup(Groups.Names.RareHen, "Hen");

    /// <summary>
    /// The ordinary young chicken
    /// </summary>
    public static IdentifiableType? Chick() => FromGroup(Groups.Names.Chick, "Chickadoo", "Chick");
    
    private static IdentifiableType? FromGroup(string groupName, params string[] preferredNames)
    {
        foreach (var name in preferredNames)
        {
            var named = LookupEUtil.GetIdentifiableTypeByRefID($"IdentifiableType.{name}");
            if (named != null) return named;
        }

        var group = Groups.Find(groupName);
        if (group?._memberTypes == null) return null;

        foreach (var member in group._memberTypes)
            if (member != null) return member;

        return null;
    }
}