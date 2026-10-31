using System;

namespace Starlight.Prism.Lib;

/// <summary>
/// The radiant rotation so slimes can spawn as radiant slimes
/// </summary>
public static class RadiantRotation
{
    /// <summary>
    /// How often a Prism slime spawns as radiant
    /// </summary>
    public const int DefaultWeight = 1500;

    /// <summary>
    /// Every Prism slime that can spawn as radiant
    /// </summary>
    internal static readonly List<SlimeDefinition> Slimes = new();

    private static readonly Dictionary<SlimeDefinition, int> Weights = new();

    /// <summary>
    /// Registers a Slime
    /// </summary>
    internal static void Register(SlimeDefinition definition, int weight, bool inRotation)
    {
        Weights[definition] = Math.Max(1, weight);

        if (inRotation && !Slimes.Contains(definition)) Slimes.Add(definition);
    }

    /// <summary>
    /// The radiant weight of a slime
    /// </summary>
    internal static int WeightOf(SlimeDefinition definition) =>
        Weights.TryGetValue(definition, out var weight) ? weight : DefaultWeight;
}