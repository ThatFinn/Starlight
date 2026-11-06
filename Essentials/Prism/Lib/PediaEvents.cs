using System;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Lib;

/// <summary>
/// Slimepedia Events
/// </summary>
internal static class PediaEvents
{
    private static readonly Dictionary<IntPtr, Action<PediaUnlock>> Registered = new();

    /// <summary>
    /// Set while Prism unlocks its own entries
    /// </summary>
    internal static bool Silent { get; set; }

    internal static void Register(PediaEntry entry, Action<PediaUnlock>? action)
    {
        if (action == null) return;

        Registered[entry.Pointer] = action;
    }

    /// <summary>
    /// Tells an entry's creator that it has been unlocked
    /// </summary>
    internal static void NotifyUnlock(PediaEntry? entry, bool showPopup)
    {
        if (Silent || Registered.Count == 0 || entry == null) return;
        if (!Registered.TryGetValue(entry.Pointer, out var action)) return;

        action(new PediaUnlock(entry, showPopup));
    }
}