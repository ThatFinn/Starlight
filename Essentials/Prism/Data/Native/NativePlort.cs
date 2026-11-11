namespace Starlight.Prism.Data.Native;

/// <summary>
/// Resolves <see cref="PrismNativePlort"/> values to identifiable types
/// </summary>
public static class NativePlorts
{
    /// <summary>
    /// Reference id of a native plort; <c>IdentifiableType.PinkPlort</c>.
    /// </summary>
    public static string ReferenceId(this PrismNativePlort plort) => $"IdentifiableType.{plort}Plort";

    /// <summary>
    /// Finds the identifiable type for a native plort
    /// </summary>
    public static IdentifiableType? Find(this PrismNativePlort plort) =>
        LookupEUtil.GetIdentifiableTypeByRefID(plort.ReferenceId());
}