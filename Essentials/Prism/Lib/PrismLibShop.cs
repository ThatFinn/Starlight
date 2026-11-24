using System;
using System.Linq;
using Il2CppMonomiPark.SlimeRancher.Economy;
using Il2CppMonomiPark.SlimeRancher.Shop;
using Il2CppMonomiPark.SlimeRancher.UI.Framework;
using Starlight.Prism.Creators;

namespace Starlight.Prism.Lib;

/// <summary>
/// Adds created items into the game's shop tables
/// </summary>
internal static class PrismLibShop
{
    private sealed record Entry(ShopRequest Request, string AssetGuid, string UniqueId);

    private readonly record struct Sold(ShopItemAssetReference? Item, AcquireGadgetType? Unlock);

    private static readonly List<Entry> Entries = new();

    private static readonly Dictionary<string, IdentifiableType> ItemsByUniqueId = new();

    /// <summary>
    /// Whether an item is listed under the <paramref name="uniqueId"/> already
    /// </summary>
    internal static bool IsListed(string uniqueId) => ItemsByUniqueId.ContainsKey(uniqueId);

    /// <summary>
    /// Lists an item, added to its table once the shop loads it
    /// </summary>
    internal static void Add(ShopRequest request, string assetGuid, string uniqueId)
    {
        Entries.Add(new Entry(request, assetGuid, uniqueId));
        ItemsByUniqueId[uniqueId] = request.Item;
    }

    /// <summary>
    /// The created item a shop entry sells, or null for the game's own
    /// </summary>
    internal static IdentifiableType? ItemFor(ShopItemAssetReference? reference) =>
        reference?._uniqueId is { } id && ItemsByUniqueId.TryGetValue(id, out var item) ? item : null;

    /// <summary>
    /// Adds every listing that belongs to the <paramref name="source"/> table
    /// </summary>
    internal static void AddTo(ShopDataSourceBase? source)
    {
        if (source == null || Entries.Count == 0) return;

        var fixedTable = source.TryCast<ShopFixedItemsTable>();
        if (fixedTable != null)
        {
            AddTo(fixedTable);
            return;
        }

        var randomTable = source.TryCast<ShopRandomItemsTable>();
        if (randomTable != null) AddTo(randomTable);
    }

    private static void AddTo(ShopFixedItemsTable table)
    {
        if (table._itemListings == null) return;

        var listings = table._itemListings.ToList();

        if (!Insert(listings, row => new Sold(row._itemCost?.ShopItem, row._itemCost?.UnlockType), (template, entry) => new ShopFixedItemsTable.ItemEntry
            {
                _itemCost          = CostFor(entry, template._itemCost),
                _acquireLimit      = template._acquireLimit,
                AvailableCondition = template.AvailableCondition
            }))
            return;

        table._itemListings = listings.ToArray();
    }

    private static void AddTo(ShopRandomItemsTable table)
    {
        if (table._items == null) return;

        var items = table._items.ToList();

        if (!Insert(items, row => new Sold(row._item, row._acquireType), (template, entry) => new ShopRandomItemsTable.ItemRateEntry
            {
                _item              = ReferenceFor(entry, template._item),
                _acquireType       = template._acquireType,
                _occurenceWeight   = template._occurenceWeight,
                AvailableCondition = template.AvailableCondition,
                _costs             = template._costs.Select(cost => new ShopRandomItemsTable.CostRateEntry
                {
                    _occurenceWeight = cost._occurenceWeight,
                    _acquireLimit    = cost._acquireLimit,
                    _purchaseCost    = PriceOr(entry, cost._purchaseCost)
                }).ToArray()
            }))
            return;

        table._items = items.ToArray();
    }

    /// <summary>
    /// Puts each missing listing right after its template
    /// </summary>
    /// <returns> Whether anything was added </returns>
    private static bool Insert<T>(List<T> table, Func<T, Sold> soldOf, Func<T, Entry, T> create) where T : class
    {
        var added = false;

        foreach (var entry in Entries)
        {
            if (table.Any(row => soldOf(row).Item?._uniqueId == entry.UniqueId)) continue;

            var templateIndex = table.FindIndex(row => IsTemplate(soldOf(row), entry));
            if (templateIndex < 0) continue;

            var index = table.FindLastIndex(row => IsTemplate(soldOf(row), entry) || IsAddedNextTo(soldOf(row), entry));

            table.Insert(index + 1, create(table[templateIndex], entry));
            added = true;
        }

        return added;
    }

    private static bool IsTemplate(Sold sold, Entry entry) =>
        sold.Unlock == entry.Request.UnlockType
        && sold.Item?._identifiableReferenceId == entry.Request.Template.ReferenceId;

    private static bool IsAddedNextTo(Sold sold, Entry entry) =>
        sold.Item?._uniqueId is { } id
        && Entries.Any(other => other.UniqueId == id && other.Request.Template.ReferenceId == entry.Request.Template.ReferenceId);

    private static ShopItemCost CostFor(Entry entry, ShopItemCost template) => new()
    {
        ShopItem     = ReferenceFor(entry, template.ShopItem),
        UnlockType   = template.UnlockType,
        PurchaseCost = PriceOr(entry, template.PurchaseCost)
    };

    private static ShopItemAssetReference ReferenceFor(Entry entry, ShopItemAssetReference template) => new()
    {
        _assetInterface          = new AssetInterface<IAcquirable>(entry.AssetGuid),
        _filterFlags             = template._filterFlags,
        _identifiableReferenceId = entry.Request.Item.ReferenceId,
        _uniqueId                = entry.UniqueId
    };

    /// <summary>
    /// The listing's price, or the template's cost when it has none
    /// </summary>
    private static PurchaseCost PriceOr(Entry entry, PurchaseCost template) =>
        entry.Request.Price is { } price ? Newbucks(price) : template;

    private static PurchaseCost Newbucks(int price)
    {
        var currencies = new Il2CppSystem.Collections.Generic.List<CurrencyCostEntry>();
        currencies.Add(CurrencyCostEntry.CreateNewbucks(price));

        return new PurchaseCost(
            currencies,
            new Il2CppSystem.Collections.Generic.List<IdentCostEntry>(),
            new Il2CppSystem.Collections.Generic.List<Il2CppMonomiPark.SlimeRancher.Player.Component.UpgradeComponent>());
    }
}