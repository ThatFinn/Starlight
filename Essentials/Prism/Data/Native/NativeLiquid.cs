using Starlight.Prism.Lib;

namespace Starlight.Prism.Data.Native;

/// <summary>
/// The base game's liquids
/// </summary>
public static class NativeLiquids
{
    /// <summary>
    /// The water liquid definition
    /// </summary>
    public static LiquidDefinition? Water()
    {
        var water = LookupEUtil.GetIdentifiableTypeByRefID("LiquidDefinition.Water")?.TryCast<LiquidDefinition>();
        if (water != null) return water;

        var group = Groups.Find(Groups.Names.Liquid);
        if (group?._memberTypes == null) return null;

        foreach (var member in group._memberTypes)
        {
            var liquid = member?.TryCast<LiquidDefinition>();
            if (liquid != null && liquid.prefab != null) return liquid;
        }

        return null;
    }
}