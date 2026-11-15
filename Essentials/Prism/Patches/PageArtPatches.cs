using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.Components;
using Il2CppMonomiPark.SlimeRancher.UI.Pedia;
using Starlight.Prism.Creators;
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

    private static readonly Dictionary<System.IntPtr, (string Guid, Color From, Color To)> Recolours = new();
    private static readonly HashSet<System.IntPtr> Recolouring = new();

    /// <summary>
    /// Whether any picture was supplied at all
    /// </summary>
    internal static bool HasAny => Supplied.Count > 0 || SuppliedRadiant.Count > 0;

    /// <summary>
    /// Remembers the picture for the page of a created thing
    /// </summary>
    internal static void Register(IdentifiableType type, Sprite? art)
    {
        if (type == null || art == null) return;

        Supplied[type.Pointer] = art.Hold()!;
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
    internal static Sprite? For(IdentifiableType? type) =>
        type != null && Supplied.TryGetValue(type.Pointer, out var art) && art != null ? art : null;

    /// <summary>
    /// The page picture supplied for what a page documents
    /// </summary>
    internal static Sprite? For(PediaEntry? entry)
    {
        if (entry == null) return null;

        var radiant = entry.TryCast<RadiantSlimePediaEntry>();
        if (radiant != null)
            return radiant.SlimeDefinition != null && SuppliedRadiant.TryGetValue(radiant.SlimeDefinition.Pointer, out var form) && form != null ? form : null;

        return For(entry.TryCast<IdentifiablePediaEntry>()?.IdentifiableType);
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
    /// Registers a recolour
    /// </summary>
    /// <param name="type"> The created thing </param>
    /// <param name="art"> The native picture </param>
    /// <param name="from"> The native colour </param>
    /// <param name="to"> The colour it becomes </param>
    internal static void RegisterRecolour(IdentifiableType type, AssetReferenceT<Sprite>? art, Color from, Color to)
    {
        var guid = art?.AssetGUID;
        if (type == null || string.IsNullOrEmpty(guid)) return;

        Recolours[type.Pointer] = (guid, from, to);
    }

    /// <summary>
    /// Recolours a Slimepedia page picture
    /// </summary>
    /// <param name="type"> The created thing </param>
    /// <param name="ready"> Called with the picture once it is made </param>
    internal static void Recolour(IdentifiableType type, System.Action<Sprite> ready)
    {
        if (type == null || !Recolours.TryGetValue(type.Pointer, out var recolour) || !Recolouring.Add(type.Pointer)) return;

        StartCoroutine(MakeRecoloured(type, recolour.Guid, recolour.From, recolour.To, ready));
    }

    private static System.Collections.IEnumerator MakeRecoloured(IdentifiableType type, string guid, Color from, Color to, System.Action<Sprite> ready)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(new AssetReferenceT<Sprite>(guid).RuntimeKey);
        while (!handle.IsDone) yield return null;

        Sprite? made = null;
        var keepLoaded = false;

        try
        {
            var native = handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded ? handle.Result : null;
            if (native == null)
            {
                LogWarning($"Prism: Couldn't load the picture to recolour for '{type.name}'");
            }
            else
            {
                made = native.Recolour(from, to);
                
                keepLoaded = made != null && made.Pointer == native.Pointer;
            }
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to recolour the picture for '{type.name}'", ex);
        }

        if (!keepLoaded) Addressables.Release(handle);

        if (made == null)
        {
            Recolouring.Remove(type.Pointer);
            yield break;
        }

        Recolours.Remove(type.Pointer);
        Register(type, made);

        try { ready(made); }
        catch (Exception ex) { LogError($"Prism: Failed to show the recoloured picture for '{type.name}'", ex); }
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
        if (image.sprite == null || image.sprite.Pointer != sprite.Pointer) image.sprite = sprite;
        if (image.overrideSprite != null && image.overrideSprite.Pointer != sprite.Pointer) image.overrideSprite = sprite;
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

            var type = picture == null ? entry.TryCast<IdentifiablePediaEntry>()?.IdentifiableType : null;
            if (type != null)
            {
                PageArt.Recolour(type, made =>
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
/// Sets the picture a created thing's creator supplied as the full art source
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(UIImageDynamicLoader), nameof(UIImageDynamicLoader.SetFullArtSource))]
internal static class PageArtLoaderPatch
{
    public static void Postfix(UIImageDynamicLoader __instance, IFullArtAndIconSource source)
    {
        // The shop and the fabricator load their icons through here as well
        if (!PageArt.HasAny || __instance == null || source == null) return;

        try
        {
            var art = PageArt.For(source.TryCast<PediaEntry>());
            if (art == null) return;

            var image = __instance.GetComponent<Image>();
            if (image != null) PageArt.Show(image, art);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to put the picture of a created thing on its pedia page", ex);
        }
    }
}

[PrismPatch,FeatureFlagDependentPatch(SupportRadiant)]
[HarmonyPatch(typeof(PediaRadiantSlimeScreen),nameof(PediaRadiantSlimeScreen.SetCategory))]
internal static class PediaRadiantSlimeScreenPatch
{
    public static void Prefix(PediaRadiantSlimeScreen __instance)
    {
        var oldButtonList = __instance._buttons.ToNetList();
        var newButtonList = __instance._buttons.ToNetList();
        
        var created = new List<RadiantSlimePediaEntry>();
        foreach (var pair in PrismShortcuts.PrismRadiantSlimePediaEntries)
            if (!pair.Value.GetPrismBaseSlime().GetIsNative()&&pair.Value.GetPrismBaseSlime().GetSlimeAppearanceRadiant()!=null)
                created.Add(pair.Value.GetRadiantSlimePediaEntry());
        created.AddRange(PrismPediaCreatorV02.RadiantEntries);
        foreach (var radiantEntry in created)
        {
            var contains = false;
            foreach (var entry in oldButtonList)
                try
                {
                    if (entry._entry==radiantEntry)
                    {
                        contains = true;
                        break;
                    }
                }catch { }
            if (!contains)
            {
                var prefab = oldButtonList[0];
                var instance = GameObject.Instantiate(prefab.gameObject, prefab.transform.parent).GetComponent<PediaEntryIconButton>();
                instance._entry = radiantEntry;
                newButtonList.Add(instance);
            }
        }
        //This is a workaround until the game does it on its own
        if (oldButtonList.Count != newButtonList.Count)
        {
            var gridLayout = newButtonList[0].transform.parent.gameObject.AddComponent<GridLayoutGroup>();
            ExecuteInTicks(() =>
            {
                ExecuteInTicks(() =>
                {
                    Object.Destroy(gridLayout);
                },2);
            },10);
        }
        __instance._buttons = newButtonList.ToIl2CppArray();
    }
}