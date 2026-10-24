using System;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Starlight.Utils;

public static class Il2CppArrayEUtil
{
    /// <summary>
    /// Copies a managed list into a new Il2Cpp reference array
    /// </summary>
    public static Il2CppReferenceArray<T> ToIl2CppArray<T>(this IReadOnlyList<T> items)
        where T : Il2CppObjectBase
    {
        var result = new Il2CppReferenceArray<T>(items.Count);
        for (var i = 0; i < items.Count; i++)
            result[i] = items[i];
        return result;
    }
    
    /// <summary>
    /// Enumerates an Il2Cpp reference array
    /// </summary>
    public static IEnumerable<T> AsEnumerable<T>(this Il2CppReferenceArray<T>? array)
        where T : Il2CppObjectBase
    {
        if (array == null) yield break;
        foreach (var item in array)
            yield return item;
    }

    /// <summary>
    /// Whether an Il2Cpp string array contains a value
    /// </summary>
    public static bool Contains(this Il2CppStringArray? array, string item)
    {
        if (array == null) return false;
        foreach (var existing in array)
            if (string.Equals(existing, item, StringComparison.Ordinal)) return true;
        return false;
    }

    /// <summary>
    /// Returns a new array holding everything after the first <paramref name="count"/> entries
    /// </summary>
    public static Il2CppReferenceArray<T> SkipToNew<T>(this Il2CppReferenceArray<T>? array, int count)
        where T : Il2CppObjectBase
    {
        var length = array?.Length ?? 0;
        var remaining = Math.Max(0, length - Math.Max(0, count));

        var result = new Il2CppReferenceArray<T>(remaining);
        for (var i = 0; i < remaining; i++)
            result[i] = array![count + i];

        return result;
    }

    /// <summary>
    /// Returns a new array holding non-null entries of both arrays
    /// </summary>
    public static Il2CppReferenceArray<T> UnionToNew<T>(this Il2CppReferenceArray<T>? first, Il2CppReferenceArray<T>? second)
        where T : Il2CppObjectBase
        => first.AsEnumerable()
            .Concat(second.AsEnumerable())
            .Where(item => item != null)
            .DistinctBy(item => item.Pointer)
            .ToList()
            .ToIl2CppArray();
}