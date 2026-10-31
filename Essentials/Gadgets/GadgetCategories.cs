using System;
using Il2CppMonomiPark.SlimeRancher.UI.Fabricator;
using Il2CppMonomiPark.SlimeRancher.UI.Gadget;
using Il2CppMonomiPark.SlimeRancher.UI.Pedia;
using UnityEngine.Localization;

namespace Starlight.Gadgets;

/// <summary>
/// Adds tabs to the game's gadget menu
/// </summary>
public static class GadgetCategories
{
    /// <summary>
    /// Creates and registers an empty group
    /// </summary>
    /// <returns> The group, or null if it could not be created </returns>
    public static IdentifiableTypeGroup CreateGroup(string name)
    {
        try
        {
            var group = ScriptableObject.CreateInstance<IdentifiableTypeGroup>();
            group.hideFlags |= HideFlags.HideAndDontSave;
            group.name = name;
            
            group._memberTypes = new Il2CppSystem.Collections.Generic.List<IdentifiableType>();
            group._memberGroups = new Il2CppSystem.Collections.Generic.List<IdentifiableTypeGroup>();
            group.AllowedCategories = new Il2CppSystem.Collections.Generic.List<IdentifiableCategory>();
            
            LookupDirector.GetIfReady()?.RegisterIdentifiableTypeGroup(group);
            return group;
        }
        catch (Exception exception)
        {
            LogError($"Gadgets: could not create group '{name}': {exception}");
            return null;
        }
    }

    /// <summary>
    /// Removes a member out of an <see cref="IdentifiableGroup"/>
    /// </summary>
    /// <returns> True when it was previously in the <see cref="IdentifiableGroup"/> and was successfully removed </returns>
    public static bool RemoveMember(IdentifiableTypeGroup group, IdentifiableType type)
    {
        if (group == null || type == null) return false;

        try
        {
            var removed = group._memberTypes?.Remove(type) ?? false;

            removed |= group.GetRuntimeObject()?._memberTypes?.Remove(type) ?? false;

            return removed;
        }
        catch (Exception exception)
        {
            LogWarning($"Gadgets: '{type.name}' could not be removed from '{group.name}': {exception.Message}");
            return false;
        }
    }

    /// <summary>
    /// Adds an <see cref="IdentifiableType"/> to an <see cref="IdentifiableGroup"/>
    /// </summary>
    /// <param name="defer">
    /// True to skip the lookup rebuild,
    /// leaving it for <see cref="RebuildLookups"/> after everything was added.
    /// </param>
    /// <remarks>
    /// Do NOT forget to rebuild the lookup!!
    /// </remarks>
    public static bool AddMember(IdentifiableTypeGroup group, IdentifiableType type, bool defer = false)
    {
        if (group == null || type == null) return false;

        var lookup = LookupDirector.GetIfReady();
        if (lookup == null) return false;

        try
        {
            var alreadyAdded = false;
            if (group._memberTypes != null)
            {
                for (var i = 0; i < group._memberTypes.Count; i++)
                {
                    if (group._memberTypes[i] != null && group._memberTypes[i].Pointer == type.Pointer) { alreadyAdded = true; break; }
                }
            }

            if (!alreadyAdded) group._memberTypes?.Add(type);
            
            if (!defer)
            {
                lookup.AddIdentifiableTypeToGroup(type, group);
                return true;
            }

            group.GetRuntimeObject()?._memberTypes?.Add(type);
            return true;
        }
        catch (Exception exception)
        {
            LogError($"Gadgets: could not add '{type.name}' to '{group.name}': {exception}");
            return false;
        }
    }

    /// <summary>
    /// Rebuilds the <see cref="IdentifiableType"/> lookups
    /// </summary>
    public static void RebuildLookups()
    {
        var lookup = LookupDirector.GetIfReady();
        if (lookup == null) return;

        try
        {
            lookup._initialized = false;
            lookup.InitializeLookups();
        }
        catch (Exception exception)
        {
            LogError($"Gadgets: IdentifiableType lookups could not be rebuilt: {exception}");
        }
    }

    /// <summary>
    /// Adds a tab to the gadget menu
    /// </summary>
    /// <param name="name"> Internal name for the category. </param>
    /// <param name="title"> The label for the category, null leaves the template's label. </param>
    /// <param name="group"> The group whose members the tab lists. </param>
    /// <param name="icon"> The icon for the category, null leaves the template's icon, </param>
    /// <returns> The category, or null if it could not be added. </returns>
    public static GadgetCategory Add(string name, LocalizedString title, IdentifiableTypeGroup group, Sprite icon = null)
    {
        if (group == null) return null;

        var configuration = FindConfiguration();
        if (configuration?.Categories == null)
        {
            LogWarning("Gadgets: there is no gadget menu to add a category to.");
            return null;
        }

        var template = FindTemplate(configuration);
        if (template == null)
        {
            LogWarning("Gadgets: there is no category template to clone from.");
            return null;
        }

        try
        {
            var category = Object.Instantiate(template);
            category.hideFlags |= HideFlags.HideAndDontSave;
            category.name = name;
            category._gadgetSubGroup = group;

            if (title != null) category._title = title;
            
            if (icon != null) category._icon = icon;

            configuration.Categories.Add(category);

            RegisterFabricatorTab(name, title, group, icon);
            return category;
        }
        catch (Exception exception)
        {
            LogError($"Gadgets: could not add gadget category '{name}': {exception}");
            return null;
        }
    }
    
    /// <summary>
    /// How many category tabs the gadget menu ships with room for, or -1 while the menu has not been built.
    /// </summary>
    public static int ButtonCapacity
    {
        get
        {
            try
            {
                var lists = Resources.FindObjectsOfTypeAll<NewGadgetCategoryList>();
                if (lists == null) return -1;

                foreach (var list in lists)
                {
                    var buttons = list?._buttons;
                    if (buttons != null && buttons.Length > 0) return buttons.Length;
                }
            }
            catch (Exception exception)
            {
                LogWarning($"Gadgets: the gadget menu button capacity could not be measured: {exception.Message}");
            }

            return -1;
        }
    }

    /// <summary>
    /// The most items one category can hold
    /// </summary>
    /// <returns> The maximum, or -1 while the capacity cannot be measured. </returns>
    public static int MaxItemsPerCategory
    {
        get
        {
            try
            {
                var grids = Resources.FindObjectsOfTypeAll<NewGadgetItemGrid>();
                if (grids == null) return -1;

                foreach (var grid in grids)
                {
                    var perPage = grid?._buttons?.Length ?? 0;
                    var dots = grid?._paginatedView?._pagination?._pips?.Length ?? 0;
                    
                    if (perPage > 0 && dots > 0) return perPage * Math.Max(dots, GadgetPagination.MaxPageDots);
                }
            }
            catch (Exception exception)
            {
                LogWarning($"Gadgets: the category's limit could not be measured: {exception.Message}");
            }

            return -1;
        }
    }

    /// <summary>
    /// The gadget menu configuration, or null when none exists
    /// </summary>
    public static GadgetMenuConfiguration FindConfiguration()
    {
        try
        {
            var found = Resources.FindObjectsOfTypeAll<GadgetMenuConfiguration>();
            if (found == null || found.Length == 0) return null;

            foreach (var configuration in found)
            {
                if (configuration != null && configuration.Categories != null && configuration.Categories.Count > 0)
                    return configuration;
            }

            return found[0];
        }
        catch (Exception exception)
        {
            LogWarning($"Gadgets: the gadget menu could not be found: {exception.Message}");
            return null;
        }
    }

    /// <summary>
    /// Finds a template category
    /// </summary>
    private static GadgetCategory FindTemplate(GadgetMenuConfiguration configuration)
    {
        var wanted = Il2CppInterop.Runtime.Il2CppType.Of<GadgetCategory>();

        for (var i = 0; i < configuration.Categories.Count; i++)
        {
            var category = configuration.Categories[i];
            if (category == null || category._gadgetSubGroup == null) continue;

            try
            {
                if (category.GetIl2CppType().Equals(wanted)) return category;
            }
            catch (Exception exception)
            {
                LogWarning($"Gadgets: could not read type of '{category.name}': {exception.Message}");
            }
        }

        return null;
    }

    private sealed record Tab(string Name, LocalizedString Title, IdentifiableTypeGroup Group, Sprite Icon);

    private static readonly List<Tab> FabricatorTabs = new();

    internal static void RegisterFabricatorTab(string name, LocalizedString title, IdentifiableTypeGroup group, Sprite icon)
    {
        if (group == null) return;

        FabricatorTabs.Add(new Tab(name, title, group, icon));
        MirrorToFabricator();
    }
    
    public static int MirrorToFabricator()
    {
        if (FabricatorTabs.Count == 0) return 0;

        var added = 0;

        try
        {
            foreach (var configuration in Resources.FindObjectsOfTypeAll<FabricateMenuConfiguration>())
            {
                if (configuration == null || configuration.Categories == null || configuration.Categories.Count == 0) continue;

                var template = FindFabricatorTemplate(configuration);
                if (template == null) continue;

                foreach (var tab in FabricatorTabs)
                {
                    if (FabricatorLists(configuration, tab.Group)) continue;

                    var category = Object.Instantiate(template);
                    category.hideFlags |= HideFlags.HideAndDontSave;
                    category.name = tab.Name;
                    category._gadgetSubGroup = tab.Group;

                    if (tab.Title != null) category._title = tab.Title;
                    if (tab.Icon != null) category._icon = tab.Icon;

                    configuration.Categories.Add(category);
                    added++;
                }
            }
        }
        catch (Exception exception)
        {
            LogError($"Gadget Category: the fabricator's categories could not be extended: {exception}");
        }

        return added;
    }

    private static bool FabricatorLists(FabricateMenuConfiguration configuration, IdentifiableTypeGroup group)
    {
        for (var i = 0; i < configuration.Categories.Count; i++)
        {
            var listed = configuration.Categories[i]?.TryCast<GadgetFabricateCategory>();
            if (listed != null && listed._gadgetSubGroup != null && listed._gadgetSubGroup.Pointer == group.Pointer) return true;
        }

        return false;
    }

    private static GadgetFabricateCategory FindFabricatorTemplate(FabricateMenuConfiguration configuration)
    {
        for (var i = 0; i < configuration.Categories.Count; i++)
        {
            var category = configuration.Categories[i]?.TryCast<GadgetFabricateCategory>();
            if (category != null && category._gadgetSubGroup != null) return category;
        }

        return null;
    }
}

[HarmonyPatch(typeof(PediaBlueprintsScreen), "Start")]
internal static class PediaBlueprintsMirrorPatch
{
    private static void Prefix() => GadgetCategories.MirrorToFabricator();
}

[HarmonyPatch(typeof(FabricatorCategoryList), "FindCategoriesToShow")]
internal static class FabricatorCategoryListMirrorPatch
{
    private static void Prefix() => GadgetCategories.MirrorToFabricator();
}

[HarmonyPatch(typeof(FabricatorContent), "Start")]
internal static class FabricatorContentMirrorPatch
{
    private static void Prefix() => GadgetCategories.MirrorToFabricator();
}