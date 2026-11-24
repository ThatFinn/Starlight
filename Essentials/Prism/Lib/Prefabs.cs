using System;
using Il2CppInterop.Runtime;
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
    /// Points every component field under <paramref name="root"/> that holds <paramref name="from"/> at <paramref name="to"/>
    /// </summary>
    /// <returns> How many fields were changed </returns>
    public static int Retarget(GameObject root, IdentifiableType from, IdentifiableType to)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        var identifiableType = Il2CppType.Of<IdentifiableType>();
        var changed          = 0;

        foreach (var component in root.GetComponentsInChildren<Component>(includeInactive: true))
        {
            if (component == null) continue;

            for (var type = component.GetIl2CppType(); type != null; type = type.BaseType)
            {
                foreach (var field in type.GetFields(DeclaredFields))
                {
                    if (!identifiableType.IsAssignableFrom(field.FieldType)) continue;
                    if (field.GetValue(component)?.Pointer != from.Pointer) continue;

                    field.SetValue(component, to);
                    changed++;
                }
            }
        }

        return changed;
    }

    private const Il2CppSystem.Reflection.BindingFlags DeclaredFields =
        Il2CppSystem.Reflection.BindingFlags.Instance
        | Il2CppSystem.Reflection.BindingFlags.Public
        | Il2CppSystem.Reflection.BindingFlags.NonPublic
        | Il2CppSystem.Reflection.BindingFlags.DeclaredOnly;

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