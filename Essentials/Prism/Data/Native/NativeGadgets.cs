using System;
using System.Linq;
using Starlight.Gadgets;

namespace Starlight.Prism.Data.Native;

/// <summary>
/// The base game's gadgets and gadget menu tabs
/// </summary>
public static class NativeGadgets
{
    /// <summary>
    /// Every gadget the game has registered
    /// </summary>
    public static IEnumerable<GadgetDefinition> All =>
        GetAll<GadgetDefinition>().Where(gadget => gadget != null && !string.IsNullOrEmpty(gadget.ReferenceId) && !Creators.PrismGadgetCreatorV02.IsCreated(gadget));

    /// <summary>
    /// The first gadget of a kind, such as a teleporter or a warp depo
    /// </summary>
    /// <remarks>
    /// Prefabs and non-variants of other gadgets are preferred
    /// </remarks>
    public static GadgetDefinition? Template(GadgetDefinition.Types type) =>
        All.Where(gadget => gadget.Type == type && gadget.prefab != null)
            .OrderBy(gadget => gadget.IsVariant)
            .FirstOrDefault();

    /// <summary>
    /// A gadget by it's object name
    /// </summary>
    public static GadgetDefinition? Find(string name) =>
        All.FirstOrDefault(gadget => string.Equals(gadget.name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The group a gadget menu tab list
    /// </summary>
    public static IdentifiableTypeGroup? CategoryGroup(string name)
    {
        var categories = GadgetCategories.FindConfiguration()?.Categories;
        if (categories == null) return null;

        for (var i = 0; i < categories.Count; i++)
        {
            var category = categories[i];
            var group = category?._gadgetSubGroup;
            if (group == null) continue;

            if (string.Equals(category!.name, name, StringComparison.OrdinalIgnoreCase)
                || string.Equals(group.name, name, StringComparison.OrdinalIgnoreCase))
                return group;
        }

        return null;
    }

    /// <summary>
    /// Every group a gadget menu tab lists
    /// </summary>
    public static IReadOnlyList<IdentifiableTypeGroup> CategoryGroups()
    {
        var result = new List<IdentifiableTypeGroup>();
        var categories = GadgetCategories.FindConfiguration()?.Categories;
        if (categories == null) return result;

        for (var i = 0; i < categories.Count; i++)
        {
            var group = categories[i]?._gadgetSubGroup;
            if (group != null) result.Add(group);
        }

        return result;
    }
}