using System;
using System.Linq;

namespace Starlight.Prism.Data.Native;

/// <summary>
/// The base game's toys
/// </summary>
public static class NativeToys
{
    /// <summary>
    /// Every toy the game has registered
    /// </summary>
    public static IEnumerable<IdentifiableType> All
    {
        get
        {
            var director = GameContext.Instance?.LookupDirector;
            var group = director?._toyDefinitions;

            var fromGroup = group == null
                ? Array.Empty<IdentifiableType>()
                : LookupEUtil.GetGroupMembers(group.name).ToArray();
            
            return fromGroup.Length > 0
                ? fromGroup
                : GetAll<ToyDefinition>().Where(toy => toy != null && !string.IsNullOrEmpty(toy.ReferenceId));
        }
    }
    
    private static readonly string[] PreferredTemplate = { "BeachBall", "PuzzleCube" };

    /// <summary>
    /// A template toy to build others on
    /// </summary>
    public static IdentifiableType? Template()
    {
        var toys = All.Where(toy => toy != null && toy.prefab != null).ToList();
        if (toys.Count == 0) return null;

        foreach (var wanted in PreferredTemplate)
        {
            var match = toys.FirstOrDefault(toy =>
                toy.name.Contains(wanted, StringComparison.OrdinalIgnoreCase));

            if (match != null) return match;
        }

        return toys[0];
    }
}