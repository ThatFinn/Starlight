using System.Linq;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.Components;
using Il2CppMonomiPark.SlimeRancher.UI.Pedia;
using Starlight.Storage;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Starlight.Prism.Patches;

/// <summary>
/// The pictures creators supplied for Slimepedia pages of created things
/// </summary>
internal static class PageArt
{
    private static readonly Dictionary<System.IntPtr, Sprite> Supplied = new();
    private static readonly Dictionary<System.IntPtr, Sprite> SuppliedRadiant = new();

    private sealed record Pending(Il2CppSystem.Object Owner, string Name, string Guid, List<ColorShift> Shifts);

    private static readonly Dictionary<System.IntPtr, Pending> Recolors = new();

    private static bool prewarming;
    
    private static readonly Dictionary<System.IntPtr, List<System.Action<Sprite>>> Recoloring = new();

    /// <summary>
    /// Whether any picture was supplied at all
    /// </summary>
    internal static bool HasAny => Supplied.Count > 0 || SuppliedRadiant.Count > 0 || Recolors.Count > 0;

    /// <summary>
    /// Remembers the picture for the page of a created thing
    /// </summary>
    internal static void Register(IdentifiableType type, Sprite? art) => Remember(type, art);

    private static void Remember(Il2CppSystem.Object owner, Sprite? art)
    {
        if (owner == null || art == null) return;

        Supplied[owner.Pointer] = art.Hold()!;
    }

    /// <summary>
    /// Remembers the picture for the page of a slime's radiant form
    /// </summary>
    internal static void RegisterRadiant(SlimeDefinition slime, Sprite? art)
    {
        if (slime == null || art == null) return;

        SuppliedRadiant[slime.Pointer] = art.Hold()!;
    }

    /// <summary>
    /// The page picture supplied for the content
    /// </summary>
    internal static Sprite? For(Il2CppSystem.Object? owner) =>
        owner != null && Supplied.TryGetValue(owner.Pointer, out var art) && art != null ? art : null;

    /// <summary>
    /// The page picture supplied for what a page documents
    /// </summary>
    internal static Sprite? For(PediaEntry? entry)
    {
        if (entry == null) return null;

        var slime = entry.TryCast<RadiantSlimePediaEntry>()?.SlimeDefinition;
        if (slime != null && SuppliedRadiant.TryGetValue(slime.Pointer, out var form) && form != null) return form;

        return For(OwnerOf(entry));
    }

    /// <summary>
    /// What a page's art belongs to
    /// </summary>
    /// <remarks> The thing for a page of a thing, so its page, the fabricator and the shop share the art; Otherwise the page itself </remarks>
    internal static Il2CppSystem.Object? OwnerOf(PediaEntry? entry)
    {
        if (entry == null) return null;

        if (entry.TryCast<RadiantSlimePediaEntry>() != null) return entry;

        return entry.TryCast<IdentifiablePediaEntry>()?.IdentifiableType ?? (Il2CppSystem.Object) entry;
    }

    /// <summary>
    /// What the art of a full art source belongs to
    /// </summary>
    internal static Il2CppSystem.Object? OwnerOf(IFullArtAndIconSource? source)
    {
        if (source == null) return null;

        var entry = source.TryCast<PediaEntry>();
        if (entry != null) return OwnerOf(entry);

        return source.TryCast<IdentifiableType>()
            ?? source.TryCast<Il2CppMonomiPark.SlimeRancher.UI.Fabricator.GadgetFabricatableItem>()?.Definition
            ?? source.TryCast<Il2CppMonomiPark.SlimeRancher.Shop.Runtime.ShopRuntimeItem>()?.Asset?.TryCast<IdentifiableType>();
    }

    /// <summary>
    /// Whether a page has full art of its own
    /// </summary>
    internal static bool HasOwnFullArt(PediaEntry entry)
    {
        var art = entry.FullArtReference;
        return art != null && art.RuntimeKeyIsValid();
    }

    /// <summary>
    /// Registers a recolor
    /// </summary>
    /// <param name="type"> The created thing </param>
    /// <param name="art"> The native picture </param>
    /// <param name="from"> The native color </param>
    /// <param name="to"> The color it becomes </param>
    internal static void RegisterRecolor(IdentifiableType type, AssetReferenceT<Sprite>? art, Color from, Color to) =>
        RegisterRecolor(type, art, new ColorShift(from, to));

    /// <summary>
    /// Registers a recolor using a <see cref="ColorShift"/>
    /// </summary>
    /// <param name="type"> The created thing </param>
    /// <param name="art"> The native picture </param>
    /// <param name="shift"> How its colors change </param>
    internal static void RegisterRecolor(IdentifiableType type, AssetReferenceT<Sprite>? art, ColorShift shift)
    {
        var guid = art?.AssetGUID;
        if (type == null || string.IsNullOrEmpty(guid)) return;

        Recolors[type.Pointer] = new Pending(type, type.name, guid, new List<ColorShift> { shift });
    }

    /// <summary>
    /// Adds a color shift to a created thing's art
    /// </summary>
    /// <returns> Whether it was recolored or not </returns>
    internal static bool AddRecolor(IdentifiableType type, ColorShift shift)
    {
        if (type == null) return false;
        if (!shift.Changes) return true;

        var queued = !Recolors.ContainsKey(type.Pointer) && For(type) == null;
        if (!Queue(type, type.name, type.FullArtReference, shift)) return false;
        
        if (queued) type.ClearFullArt();

        Prewarm();
        return true;
    }

    /// <summary>
    /// Adds a color shift to a page's art
    /// </summary>
    /// <returns> Whether it was recolored </returns>
    internal static bool AddRecolor(PediaEntry entry, ColorShift shift)
    {
        var owner = OwnerOf(entry);
        if (owner == null) return false;

        if (owner.Pointer != entry.Pointer) return AddRecolor(owner.Cast<IdentifiableType>(), shift);
        if (!shift.Changes) return true;

        var slime = entry.TryCast<RadiantSlimePediaEntry>()?.SlimeDefinition;
        if (slime != null && SuppliedRadiant.TryGetValue(slime.Pointer, out var form) && form != null)
        {
            SuppliedRadiant[slime.Pointer] = (form.Recolor(shift) ?? form).Hold()!;
            return true;
        }

        if (!Queue(entry, entry.name, entry.FullArtReference, shift)) return false;

        Prewarm();
        return true;
    }

    /// <summary>
    /// Recolors the art thats made already, or adds the shift to the recolor waiting
    /// </summary>
    /// <returns> False when there is no art to recolor </returns>
    private static bool Queue(Il2CppSystem.Object owner, string name, AssetReferenceT<Sprite>? art, ColorShift shift)
    {
        if (For(owner) is { } made)
        {
            Remember(owner, made.Recolor(shift) ?? made);
            return true;
        }

        if (Recolors.TryGetValue(owner.Pointer, out var pending))
        {
            pending.Shifts.Add(shift);
            return true;
        }

        var guid = art?.AssetGUID;
        if (string.IsNullOrEmpty(guid)) return false;

        Recolors[owner.Pointer] = new Pending(owner, name, guid, new List<ColorShift> { shift });
        return true;
    }

    /// <summary>
    /// Recolors everything waiting to be recolored
    /// </summary>
    internal static void Prewarm()
    {
        if (prewarming || Recolors.Count == 0) return;

        prewarming = true;
        StartCoroutine(PrewarmAll());
    }

    private static System.Collections.IEnumerator PrewarmAll()
    {
        while (true)
        {
            var next = Recolors.Values.FirstOrDefault(p => p.Owner != null && !Recoloring.ContainsKey(p.Owner.Pointer));
            if (next == null) break;

            Recolor(next.Owner, _ => { });

            while (Recoloring.ContainsKey(next.Owner.Pointer)) yield return null;

            if (Recolors.ContainsKey(next.Owner.Pointer)) Recolors.Remove(next.Owner.Pointer);

            yield return null;
        }

        prewarming = false;
    }

    /// <summary>
    /// Recolors a Slimepedia page picture
    /// </summary>
    /// <param name="owner"> What the art belongs to, see <see cref="OwnerOf(PediaEntry)"/> </param>
    /// <param name="ready"> Called with the picture once it is made </param>
    internal static void Recolor(Il2CppSystem.Object owner, System.Action<Sprite> ready)
    {
        if (owner == null || !Recolors.TryGetValue(owner.Pointer, out var recolor)) return;

        if (Recoloring.TryGetValue(owner.Pointer, out var waiting))
        {
            waiting.Add(ready);
            return;
        }

        Recoloring[owner.Pointer] = new List<System.Action<Sprite>> { ready };
        StartCoroutine(MakeRecolored(recolor));
    }

    internal static bool HasRecolor(Il2CppSystem.Object? owner) => owner != null && Recolors.ContainsKey(owner.Pointer);

    private static System.Collections.IEnumerator MakeRecolored(Pending pending)
    {
        var owner  = pending.Owner;
        var handle = Addressables.LoadAssetAsync<Sprite>(new AssetReferenceT<Sprite>(pending.Guid).RuntimeKey);
        while (!handle.IsDone) yield return null;

        Sprite? made = null;
        var keepLoaded = false;

        try
        {
            var native = handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded ? handle.Result : null;
            if (native == null)
            {
                LogWarning($"Prism: Couldn't load the picture to recolor for '{pending.Name}'");
            }
            else
            {
                made = native;
                foreach (var shift in pending.Shifts) made = made?.Recolor(shift);

                keepLoaded = made != null && made.Pointer == native.Pointer;
            }
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to recolor the picture for '{pending.Name}'", ex);
        }

        if (!keepLoaded) Addressables.Release(handle);

        Recoloring.Remove(owner.Pointer, out var waiting);

        if (made == null) yield break;

        Recolors.Remove(owner.Pointer);
        Remember(owner, made);

        foreach (var ready in waiting ?? new List<System.Action<Sprite>>())
        {
            try { ready(made); }
            catch (Exception ex) { LogError($"Prism: Failed to show the recolored art for '{pending.Name}'", ex); }
        }
    }

    /// <summary>
    /// A new reference to the same picture
    /// </summary>
    internal static AssetReferenceT<Sprite>? OwnFullArt(AssetReferenceT<Sprite>? source)
    {
        var guid = source?.AssetGUID;
        return string.IsNullOrEmpty(guid) ? null : new AssetReferenceT<Sprite>(guid);
    }

    /// <summary>
    /// Gives the type the picture of another
    /// </summary>
    internal static void KeepFullArtFrom(this IdentifiableType type, IdentifiableType source)
    {
        var own = OwnFullArt(source.FullArtReference);
        if (own == null) return;

        type._fullArt = own;
        type._requiresFullArt = source.RequiresFullArt;
    }

    /// <summary>
    /// Clears the art of a copy inherited from its template
    /// </summary>
    internal static void ClearFullArt(this IdentifiableType type)
    {
        if (type == null) return;

        type._requiresFullArt = false;
        type._fullArt = null;
    }

    /// <summary>
    /// Draws a sprite on a page's image
    /// </summary>
    internal static void Show(Image image, Sprite sprite)
    {
        if (image.overrideSprite == null || image.overrideSprite.Pointer != sprite.Pointer) image.overrideSprite = sprite;
        image.enabled = true;
    }
}

/// <summary>
/// Draws a created entry's picture on the pedia's page, or its icon while it has none
/// </summary>
[PrismPatch()]
[HarmonyPatch(typeof(PediaEntryIconAndHighlights), nameof(PediaEntryIconAndHighlights.SetPediaEntry))]
internal static class PediaFullArtFallbackPatch
{
    public static void Postfix(PediaEntryIconAndHighlights __instance, PediaEntry entry)
    {
        if (__instance == null || entry == null) return;

        try
        {
            var picture = PageArt.For(entry);
            if (picture == null && (entry.RequiresFullArt || PageArt.HasOwnFullArt(entry))) return;

            var image = __instance._primaryIcon;
            if (image == null) return;

            var owner = picture == null ? PageArt.OwnerOf(entry) : null;
            if (owner != null)
            {
                PageArt.Recolor(owner, made =>
                {
                    if (__instance == null || image == null || __instance._currentEntry == null || __instance._currentEntry.Pointer != entry.Pointer) return;
                    PageArt.Show(image, made);
                });

                picture = PageArt.For(entry);
            }

            picture ??= entry.Icon;
            if (picture == null) return;

            PageArt.Show(image, picture);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to put a created entry's icon on its pedia page", ex);
        }
    }
}

/// <summary>
/// Sets the picture of a created thing as the full art source
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(UIImageDynamicLoader), nameof(UIImageDynamicLoader.SetFullArtSource))]
internal static class PageArtLoaderPatch
{
    public static void Postfix(UIImageDynamicLoader __instance, IFullArtAndIconSource source)
    {
        if (!PageArt.HasAny || __instance == null || source == null) return;

        try
        {
            var owner = PageArt.OwnerOf(source);
            var art   = PageArt.For(source.TryCast<PediaEntry>()) ?? PageArt.For(owner);

            var image = __instance.GetComponent<Image>();
            if (image == null) return;

            if (art != null)
            {
                PageArt.Show(image, art);
                return;
            }

            if (!PageArt.HasRecolor(owner)) return;

            var shown = source.Cast<Il2CppSystem.Object>().Pointer;

            PageArt.Recolor(owner!, made =>
            {
                if (__instance == null || image == null) return;

                var current = __instance._iconSource;
                if (current == null || current.Cast<Il2CppSystem.Object>().Pointer != shown) return;

                PageArt.Show(image, made);
            });
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to put the art of a created thing on its pedia page", ex);
        }
    }
}

[PrismPatch,FeatureFlagDependentPatch(SupportRadiant)]
[HarmonyPatch(typeof(PediaRadiantSlimeScreen),nameof(PediaRadiantSlimeScreen.SetCategory))]
internal static class PediaRadiantSlimeScreenPatch
{
    public static void Prefix(PediaRadiantSlimeScreen __instance, IPediaRuntimeCategory<PediaEntry> category)
    {
        try
        {
            var buttons = __instance._buttons;
            if (buttons == null || buttons.Length < 2) return;

            var items = category?.TryCast<PediaRuntimeCategory>()?._items;
            if (items == null || items.Count <= buttons.Length) return;

            var origin  = Position(buttons[0]);
            var columns = 1;

            while (columns < buttons.Length && Mathf.Approximately(Position(buttons[columns]).y, origin.y))
                columns++;

            var stepX = Position(buttons[1]).x - origin.x;
            var stepY = columns < buttons.Length ? origin.y - Position(buttons[columns]).y : stepX;

            var list = buttons.ToNetList();
            var prefab = buttons[0];

            for (var i = list.Count; i < items.Count; i++)
            {
                var button = GameObject.Instantiate(prefab.gameObject, prefab.transform.parent).GetComponent<PediaEntryIconButton>();
                button.name = $"{prefab.name} ({i})";
                button.transform.SetAsLastSibling();
                button.GetComponent<RectTransform>().anchoredPosition = origin + new Vector2(i % columns * stepX, -(i / columns) * stepY);
                list.Add(button);
            }

            __instance._buttons = list.ToIl2CppArray();

            var navigation = __instance._gridNavigation;
            if (navigation == null || navigation.NumColumns <= 0) return;

            var rows = (items.Count + navigation.NumColumns - 1) / navigation.NumColumns;
            if (navigation.NumRows < rows) navigation.SetSize(navigation.NumColumns, rows);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to add buttons to the radiant slime screen", ex);
        }
    }

    private static Vector2 Position(PediaEntryIconButton button) => button.GetComponent<RectTransform>().anchoredPosition;
}