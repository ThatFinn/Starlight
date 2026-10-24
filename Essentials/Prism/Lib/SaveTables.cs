using System;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppMonomiPark.SlimeRancher;

namespace Starlight.Prism.Lib;

/// <summary>
/// New content for the save tables
/// </summary>
internal static class SaveTables
{
    /// <summary>
    /// Registers a reference id to a translation
    /// </summary>
    internal static void Register<T>(PersistenceIDTranslation<T>? translation, string referenceId, T value)
        where T : Il2CppObjectBase
    {
        if (translation == null) return;

        translation.RawLookupDictionary.TryAdd(referenceId, value);

        var lookup = translation.InstanceLookupTable;
        if (lookup?._primaryIndex == null || lookup._primaryIndex.Count == 0) return;

        if (!lookup._primaryIndex.Contains(referenceId))
            lookup._primaryIndex = lookup._primaryIndex.AddToNew(referenceId);
        for (var i = 0; i < lookup._primaryIndex.Count; i++)
        {
            if (!string.Equals(lookup._primaryIndex[i], referenceId, StringComparison.Ordinal)) continue;

            lookup._reverseIndex[referenceId] = i;
            return;
        }
    }

    internal static SaveReferenceTranslation? Translation => autoSaveDirector?._saveReferenceTranslation;
}