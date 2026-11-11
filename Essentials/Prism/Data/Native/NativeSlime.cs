namespace Starlight.Prism.Data.Native;

/// <summary>
/// Resolves <see cref="PrismNativeBaseSlime"/> values to slime definitions
/// </summary>
public static class NativeSlimes
{
    /// <summary>
    /// Reference id of a native slime; <c>SlimeDefinition.Pink</c>.
    /// </summary>
    public static string ReferenceId(this PrismNativeBaseSlime slime) => $"SlimeDefinition.{slime}";

    /// <summary>
    /// Finds the slime definition for a native slime
    /// </summary>
    public static SlimeDefinition? Find(this PrismNativeBaseSlime slime) =>
        LookupEUtil.GetBaseSlimeByRefID(slime.ReferenceId());

    /// <summary>
    /// Resolves a native slime
    /// </summary>
    /// <param name="slime"> The requested wanted </param>
    /// <param name="usedFallback"> Set to true if Pink was used as a fallback </param>
    public static SlimeDefinition? ResolveOrFallback(this PrismNativeBaseSlime slime, out bool usedFallback)
    {
        usedFallback = false;

        var found = slime.Find();
        if (found != null) return found;

        if (slime == PrismNativeBaseSlime.Pink) return null;

        usedFallback = true;
        return PrismNativeBaseSlime.Pink.Find();
    }
}