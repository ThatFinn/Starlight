using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.Interaction;

namespace Starlight.Gadgets;

[HarmonyPatch]
public static class GadgetPagination
{
    /// <summary>
    /// The most dots a view will be grown to
    /// </summary>
    public const int MaxPageDots = 12;

    /// <summary>
    /// How many dots the largest grown view has, or -1 if none was grown
    /// </summary>
    public static int PageDots { get; private set; } = -1;

    /// <summary>
    /// Grows the dots before the view sets a new page count
    /// </summary>
    [HarmonyPatch(typeof(PaginatedView), nameof(PaginatedView.SetNumberOfPages))]
    [HarmonyPrefix]
    private static void BeforeSetNumberOfPages(PaginatedView __instance, int numPages) => Ensure(__instance, numPages);

    /// <summary>
    /// Grows the dots before the view moves to a page
    /// </summary>
    [HarmonyPatch(typeof(PaginatedView), nameof(PaginatedView.SetPage), typeof(int), typeof(InteractionCause))]
    [HarmonyPrefix]
    private static void BeforeSetPage(PaginatedView __instance) => Ensure(__instance, __instance?._pagination?._numPages ?? 0);

    /// <summary>
    /// Grows the dots of a view
    /// </summary>
    private static void Ensure(PaginatedView view, int pages)
    {
        try
        {
            Grow(view?._pagination, pages);
        }
        catch (Exception exception)
        {
            LogError($"Gadgets: page dots could not be grown: {exception}");
        }
    }

    /// <summary>
    /// Clones dots until there is one for every page, up to <see cref="MaxPageDots"/>
    /// </summary>
    private static void Grow(PaginationControl control, int pages)
    {
        var dots = control?._pips;

        if (control is null || dots is null || dots.Length == 0) return;
        if (pages <= dots.Length) return;

        var wanted = Math.Min(pages, MaxPageDots);

        if (wanted <= dots.Length)
        {
            LogWarning($"Gadgets: a view requested {pages} pages with {dots.Length} dots and a cap of {MaxPageDots}, returning.");
            return;
        }

        var template = dots[dots.Length - 1];
        if (template == null) return;

        var parent = template.transform.parent;
        var grown = new Il2CppReferenceArray<PaginationPip>(wanted);

        for (var i = 0; i < dots.Length; i++) grown[i] = dots[i];

        var filled = dots.Length;

        for (var i = dots.Length; i < wanted; i++)
        {
            var clone = Object.Instantiate(template.gameObject, parent);
            clone.name = $"{template.gameObject.name} ({i})";

            var dot = clone.GetComponent<PaginationPip>();

            if (dot == null)
            {
                LogWarning("Gadgets: a cloned dot had no PaginationPip, destroying it");
                Object.Destroy(clone);
                break;
            }

            grown[i] = dot;
            filled = i + 1;
        }

        if (filled < wanted)
        {
            var trimmed = new Il2CppReferenceArray<PaginationPip>(filled);
            for (var i = 0; i < filled; i++) trimmed[i] = grown[i];
            grown = trimmed;
        }

        control._pips = grown;
        PageDots = Math.Max(PageDots, grown.Length);
    }
}