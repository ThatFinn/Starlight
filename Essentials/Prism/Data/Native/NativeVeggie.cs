using System.Linq;

namespace Starlight.Prism.Data.Native;

/// <summary>
/// The base game's veggies
/// </summary>
public enum NativeVeggie
{
    Carrot,
    Beet,
    Ginger,
    Lettuce,
    Ocaoca,
    Onion,
    Parsnip,
    Tater,
    Unstable,
}

/// <summary>
/// Resolves <see cref="NativeVeggie"/> values to identifiable types
/// </summary>
public static class NativeVeggies
{
    /// <summary>
    /// The default template for created veggies
    /// </summary>
    public const NativeVeggie DefaultTemplate = NativeVeggie.Carrot;

    /// <summary>
    /// Reference id of a native veggie; <c>IdentifiableType.CarrotVeggie</c>.
    /// </summary>
    public static string ReferenceId(this NativeVeggie veggie) => $"IdentifiableType.{veggie}Veggie";

    /// <summary>
    /// Finds the identifiable type for a native veggie
    /// </summary>
    public static IdentifiableType? Find(this NativeVeggie veggie) =>
        LookupEUtil.GetIdentifiableTypeByRefID(veggie.ReferenceId());

    /// <summary>
    /// Resolves the veggie used as a template
    /// </summary>
    public static IdentifiableType? ResolveTemplate(NativeVeggie preferred = DefaultTemplate) =>
        preferred.Find() ?? LookupEUtil.veggieFoodTypes.FirstOrDefault(veggie => veggie.prefab != null);
}