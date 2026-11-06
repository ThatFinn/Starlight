using System;

namespace Starlight.Prism.Creators.Actions;

/// <summary> A toy the vacpack has taken hold of </summary>
/// <param name="Toy"> The toy's object </param>
/// <param name="Type"> Which toy it is </param>
public sealed record ToyHold(GameObject Toy, ToyDefinition Type);

/// <summary>
/// What a created toy does when something happens to it.
/// </summary>
/// <remarks> Give it to a toy with <c>ToyActions</c> on its <c>ToyRequest</c> </remarks>
public sealed record ToyActions
{
    /// <summary>
    /// Runs when the vacpack takes hold of one of these toys
    /// </summary>
    public Action<ToyHold>? CustomHoldAction { get; init; }
}