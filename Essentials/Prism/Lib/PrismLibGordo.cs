using Starlight.Prism.Wrappers;

namespace Starlight.Prism.Lib;
/// <summary>
/// A library of helper functions for dealing with gordos
/// </summary>
public static class PrismLibGordo
{
    /// <summary>
    /// Sets the required bait for a gordo
    /// </summary>
    /// <param name="gordo">The gordo to set the bait for</param>
    /// <param name="baitType">The bait to set</param>
    public static void SetRequiredBait(this PrismGordo gordo, IdentifiableType baitType)
    {
        if (gordo == null) return;
        GordoBaitDict.Remove(baitType.ReferenceId);
        GordoBaitDict.Add(baitType.ReferenceId, gordo);
    }
    internal static readonly Dictionary<string, PrismGordo> GordoBaitDict = new ();
}