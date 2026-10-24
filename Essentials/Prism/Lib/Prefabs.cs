using System;
using UObject = UnityEngine.Object;

namespace Starlight.Prism.Lib;

/// <summary>
/// Creates prefabs
/// </summary>
public static class Prefabs
{
    /// <summary>
    /// Copies the <paramref name="source"/> object into a <paramref name="name"/>d inactive object
    /// </summary>
    /// <param name="name"> Object name for the copy </param>
    /// <param name="source"> The object to copy </param>
    public static GameObject Create(string name, GameObject source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return CreatePrefab(name, source);
    }

    /// <summary>
    /// Creates an empty <paramref name="name"/>d inactive object
    /// </summary>
    public static GameObject CreateEmpty(string name)
    {
        var created = new GameObject(name);
        created.transform.SetParent(PrefabHolder.transform, worldPositionStays: false);
        UObject.DontDestroyOnLoad(created);
        return created;
    }
}