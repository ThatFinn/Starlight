using System;

namespace Starlight.Prism.Creators.Actions;

/// <summary> A gordo slime that has been fed </summary>
/// <param name="Gordo"> The gordo slime's object </param>
/// <param name="Type"> Which gordo slime it is </param>
/// <param name="Slime"> The slime it is a giant of </param>
/// <param name="Food"> What it was fed </param>
/// <param name="IsFavourite"> Whether that is one of the slime's favourite foods </param>
public sealed record GordoFeed(GameObject Gordo, IdentifiableType Type, SlimeDefinition Slime, IdentifiableType? Food, bool IsFavourite);

/// <summary> A gordo slime that is bursting </summary>
/// <param name="Gordo"> The gordo slimes's object. (removed afterwards) </param>
/// <param name="Type"> Which gordo slime it is </param>
/// <param name="Slime"> The slime it is a giant of </param>
/// <param name="Position"> Where it burst </param>
public sealed record GordoBurst(GameObject Gordo, IdentifiableType Type, SlimeDefinition Slime, Vector3 Position);

/// <summary>
/// What a created gordo slime does when something happens to it
/// </summary>
/// <remarks>
/// Give them to a gordo slime with <c>GordoRequest.Actions</c>
/// </remarks>
/// <example>
/// <code>
/// Actions = new GordoActions
/// {
///     CustomFavouriteFeedAction = feed => Log($"loved {feed.Food?.name}"),
///     CustomBurstAction = burst => Log($"burst at {burst.Position}")
/// }
/// </code>
/// </example>
public sealed record GordoActions
{
    /// <summary>
    /// Runs whenever one of these gordo slimes is fed
    /// </summary>
    public Action<GordoFeed>? CustomFeedAction { get; init; }

    /// <summary>
    /// Runs when one of these gordo slimes is fed one of its slime's favourite foods;
    /// Runs after <see cref="CustomFeedAction"/>
    /// </summary>
    public Action<GordoFeed>? CustomFavouriteFeedAction { get; init; }

    /// <summary>
    /// Runs when one of these gordo slimes bursts
    /// </summary>
    public Action<GordoBurst>? CustomBurstAction { get; init; }
}
