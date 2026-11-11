using System.Linq;

namespace Starlight.Prism.Data.Native;

/// <summary>
/// The base game's fruits
/// </summary>
public enum NativeFruit
{
    Pogo,
    Cuberry,
    Mango,
    Pomegranite,
    Pear,
    Polaricherry,
    Unstable,
}

/// <summary>
/// Resolves <see cref="NativeFruit"/> values to identifiable types
/// </summary>
public static class NativeFruits
{
    /// <summary>
    /// The default template for created fruits
    /// </summary>
    public const NativeFruit DefaultTemplate = NativeFruit.Pogo;

    /// <summary>
    /// Reference id of a native fruit; <c>IdentifiableType.PogoFruit</c>.
    /// </summary>
    public static string ReferenceId(this NativeFruit fruit) => $"IdentifiableType.{fruit}Fruit";

    /// <summary>
    /// Finds the identifiable type for a native fruit
    /// </summary>
    public static IdentifiableType? Find(this NativeFruit fruit) =>
        LookupEUtil.GetIdentifiableTypeByRefID(fruit.ReferenceId());

    /// <summary>
    /// Resolves the fruit used as a template
    /// </summary>
    public static IdentifiableType? ResolveTemplate(NativeFruit preferred = DefaultTemplate) =>
        preferred.Find() ?? LookupEUtil.fruitFoodTypes.FirstOrDefault(fruit => fruit.prefab != null);
}