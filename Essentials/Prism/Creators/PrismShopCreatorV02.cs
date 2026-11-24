using System;
using System.Security.Cryptography;
using System.Text;
using Il2CppMonomiPark.SlimeRancher.Economy;
using Starlight.Prism.Lib;

namespace Starlight.Prism.Creators;

/// <summary>
/// A request to sell an item in one of the game's shops
/// </summary>
public record ShopRequest
{
    /// <summary>
    /// The item sold
    /// </summary>
    public required IdentifiableType Item { get; init; }

    /// <summary>
    /// A template listing of an item the game already sells
    /// </summary>
    /// <remarks> Has to match the <see cref="ShopRequest.UnlockType"/> </remarks>
    public required IdentifiableType Template { get; init; }

    /// <summary>
    /// The price in newbucks
    /// </summary>
    /// <remarks> Null = the template's price </remarks>
    public int? Price { get; init; }

    /// <summary>
    /// Whether the item or its blueprint is sold
    /// </summary>
    /// <remarks> The <see cref="Template"/> has to be sold the same way </remarks>
    public AcquireGadgetType UnlockType { get; init; } = AcquireGadgetType.ITEM_BLUEPRINT;
}

/// <summary>
/// A shop listing created by Prism
/// </summary>
/// <param name="Item"> The item sold </param>
/// <param name="UniqueId"> The persistence id of the listing </param>
public record CreatedShopListing(IdentifiableType Item, string UniqueId);

/// <summary>
/// The shop creator, used to list items for sale in the game's shops
/// </summary>
public static class PrismShopCreatorV02
{
    /// <summary>
    /// The prefix of the name every listing's persistence id is made from; <c>Starlight.ShopItem.IdentifiableType.ModdedTestGadgetABCD.ITEM_BLUEPRINT</c>
    /// </summary>
    private const string ItemPrefix = "Starlight.ShopItem.";

    /// <summary>
    /// The prefix of the name every listing's asset guid is made from; <c>Starlight.ShopAsset.IdentifiableType.ModdedTestGadgetABCD.ITEM_BLUEPRINT</c>
    /// </summary>
    private const string AssetPrefix = "Starlight.ShopAsset.";

    /// <summary>
    /// Creates a shop listing from a <see cref="ShopRequest"/>
    /// </summary>
    /// <returns> The created listing, or null </returns>
    public static CreatedShopListing? Create(ShopRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsGameReady)
        {
            LogError($"Prism: Can't create shop listing for '{request.Item?.name}': the game isn't ready");
            return null;
        }

        var item = request.Item;
        if (item == null || string.IsNullOrEmpty(item.ReferenceId))
        {
            LogError($"Prism: Can't create shop listing for '{item?.name}': the item isn't registered");
            return null;
        }

        if (request.Template == null)
        {
            LogError($"Prism: Can't create shop listing for '{item.name}': there's no template to copy");
            return null;
        }

        var key      = $"{item.ReferenceId}.{request.UnlockType}";
        var uniqueId = StableGuid(ItemPrefix + key).ToString("D");

        if (PrismLibShop.IsListed(uniqueId))
        {
            LogWarning($"Prism: Can't create shop listing for '{item.name}': it's already listed as {request.UnlockType}");
            return null;
        }

        PrismLibShop.Add(request, StableGuid(AssetPrefix + key).ToString("N"), uniqueId);

        return new CreatedShopListing(item, uniqueId);
    }
    
    private static Guid StableGuid(string name) => new(MD5.HashData(Encoding.UTF8.GetBytes(name)));
}