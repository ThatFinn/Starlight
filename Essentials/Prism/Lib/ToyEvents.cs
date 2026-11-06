using System;
using Starlight.Prism.Creators.Actions;

namespace Starlight.Prism.Lib;

/// <summary>
/// What a created slime toy does when something happens to it
/// </summary>
internal static class ToyEvents
{
    private static readonly Dictionary<IntPtr, ToyActions> Registered = new();

    internal static void Register(IdentifiableType type, ToyActions? actions)
    {
        if (actions == null) return;

        Registered[type.Pointer] = actions;
    }

    /// <summary>
    /// Tells a toy's creator that the vacpack has taken hold of one
    /// </summary>
    internal static void NotifyHold(GameObject? held)
    {
        if (Registered.Count == 0 || held == null) return;

        var actor = held.GetComponent<IdentifiableActor>();
        var type = actor == null ? null : actor.identType;
        var toy = type?.TryCast<ToyDefinition>();
        if (toy == null || !Registered.TryGetValue(type!.Pointer, out var actions)) return;

        actions.CustomHoldAction?.Invoke(new ToyHold(held, toy));
    }
}