
namespace Starlight.Prism.Patches;

/// <summary>
/// Keeps face material objects Prism created from being destroyed
/// </summary>
public static class FaceMaterialOwnership
{
    /// <summary>
    /// Instance ids of the materials Prism created for slime faces
    /// </summary>
    private static readonly HashSet<int> Owned = new();

    /// <summary>
    /// Marks an object as Prism's
    /// </summary>
    public static void Claim(UnityEngine.Object? material)
    {
        if (material == null) return;

        Owned.Add(material.GetInstanceID());
    }

    /// <summary>
    /// True if Prism created this object and it must not be destroyed
    /// </summary>
    public static bool IsOwned(UnityEngine.Object? candidate)
    {
        if (Owned.Count == 0 || candidate == null) return false;

        return Owned.Contains(candidate.GetInstanceID());
    }
}

// TODO: this needs to be improved :p

/// <summary>
/// Refuses the game's attempt to free a face material Prism owns
/// </summary>
[HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), typeof(UnityEngine.Object))]
internal static class FaceMaterialProtectionPatch
{
    public static bool Prefix(UnityEngine.Object obj)
    {
        try
        {
            return !FaceMaterialOwnership.IsOwned(obj);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to check whether a destroyed object was Prism's", ex);
            return true;
        }
    }
}