using System;
using Starlight.Enums;

namespace Starlight.Storage;

/// <summary>
/// An action a mod wants a key for.
/// </summary>
public sealed class StarlightKeybind
{
    /// <summary>
    /// Id of the Keybind; sr2mp.push_to_talk
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of the Action; "Push To Talk"
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Id of the mod; com.author.modname
    /// </summary>
    public string Owner { get; init; } = string.Empty;

    /// <summary>
    /// The default keybind, and what <c>reset</c> goes back to
    /// </summary>
    public LKey Default { get; init; } = LKey.None;
    
    /// <summary>
    /// The action that will fire when the keybind is pressed
    /// </summary>
    public Action Action { get; init; } = null!;

    /// <summary>
    /// Whether it only fires once a save is loaded
    /// </summary>
    public bool InGameOnly { get; init; } = true;

    /// <summary>
    /// The key in use, whatever it was rebound to, or the default
    /// </summary>
    public LKey Key => Managers.StarlightKeybindManager.KeyOf(this);

    /// <inheritdoc/>
    public override string ToString() => $"{Name} - {Key}";
}