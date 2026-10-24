using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.UI;
using Starlight.Prism.Data.Market;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Lib;
using Starlight.Prism.Wrappers;
using UnityEngine.Localization;

namespace Starlight.Prism;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
public static class PrismShortcuts
{
    internal static readonly Dictionary<IdentifiableType, PrismMarketData> MarketData = new ();
    internal static readonly Dictionary<PlortEntry, bool> MarketPlortEntries = new ();
    internal static readonly List<IdentifiableType> RemoveMarketPlortEntries = new ();
    internal static readonly List<SystemAction> CreateLargoActions = new ();
    
    
    internal static readonly Dictionary<IdentifiableTypeGroup, PrismIdentifiableTypeGroup> PrismIdentifiableTypeGroups = new ();
    internal static readonly Dictionary<string, PrismBaseSlime> PrismBaseSlimes = new ();
    internal static readonly Dictionary<string, PrismGordo> PrismGordos = new ();
    internal static readonly Dictionary<string, PrismLargo> PrismLargos = new ();

    internal static readonly Dictionary<PrismLargo, (PrismBaseSlime, PrismBaseSlime)> PrismLargoBases = new ();
    internal static readonly Dictionary<string, PrismPlort> PrismPlorts = new ();
    internal static readonly Dictionary<string, PrismToy> PrismToys = new ();
    internal static readonly Dictionary<string, PrismGadget> PrismGadgets = new ();
    internal static readonly Dictionary<string, PrismLiquid> PrismLiquids = new ();
    
    internal static readonly Dictionary<string, PrismFood> PrismUnknownFoods = new ();
    internal static readonly Dictionary<string, PrismVeggieFood> PrismVeggieFoods = new ();
    internal static readonly Dictionary<string, PrismFruitFood> PrismFruitFoods = new ();
    internal static readonly Dictionary<string, PrismNectarFood> PrismNectarFoods = new ();
    internal static readonly Dictionary<string, PrismChickenFood> PrismChickenFoods = new ();
    internal static readonly Dictionary<string, PrismBabyChickenFood> PrismBabyChickenFoods = new ();
    
    internal static readonly Dictionary<FixedPediaEntry, PrismFixedPediaEntry> PrismFixedPediaEntries = new();
    internal static readonly Dictionary<IdentifiablePediaEntry, PrismIdentifiablePediaEntry> PrismIdentifiablePediaEntries = new ();
    internal static readonly Dictionary<RadiantSlimePediaEntry, PrismRadiantSlimePediaEntry> PrismRadiantSlimePediaEntries = new ();
    internal static readonly Dictionary<PediaEntry, PrismPediaEntry> PrismUnknownPediaEntries = new ();
    internal static LocalizedString EmptyTranslation;
    internal static Sprite UnavailableIcon;
    
    
    private static SlimeAppearanceDirector _mainAppearanceDirector;

    public static SlimeAppearanceDirector mainAppearanceDirector
    {
        get
        {
            if (_mainAppearanceDirector == null) _mainAppearanceDirector = Get<SlimeAppearanceDirector>("MainSlimeAppearanceDirector");
            return _mainAppearanceDirector;
        }
    }
    internal static readonly Dictionary<string, List<Action>> OnSceneLoaded = new ();
    internal static void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        var pair = OnSceneLoaded.FirstOrDefault(x => sceneName.Contains(x.Key));

        if (pair.Value != null)
            foreach (var action in pair.Value)
                action();
    }
    internal static void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        
    }

    
    
    public static PrismBaseSlime GetPrismBaseSlime(this PrismNativeBaseSlime nativeBaseSlime)
    {
        try { return LookupEUtil.baseSlimeTypes.GetEntryByRefID(nativeBaseSlime.GetReferenceID()).GetPrismBaseSlime(); } catch { }

        return null;
    }
    public static PrismLiquid GetPrismLiquid(this PrismNativeLiquid nativeLiquid)
    {
        try { return LookupEUtil.liquidTypes.GetEntryByRefID(nativeLiquid.GetReferenceID()).GetPrismLiquid(); } catch { }

        return null;
    }
    public static PrismPlort GetPrismPlort(this PrismNativePlort nativePlort)
    {
        try { return LookupEUtil.plortTypes.GetEntryByRefID(nativePlort.GetReferenceID()).GetPrismPlort(); 
        } catch { }

        return null;
    }
    public static PrismBaseSlime GetPrismBaseSlime(this SlimeDefinition customOrNativeSlime)
    {
        if (!customOrNativeSlime) return null;
        if (customOrNativeSlime.IsLargo) return null;
        if (PrismBaseSlimes.TryGetValue(customOrNativeSlime.ReferenceId, out var slime)) return slime;
        var newSlime = new PrismBaseSlime(customOrNativeSlime, true);
        PrismBaseSlimes.Add(customOrNativeSlime.ReferenceId, newSlime);
        return newSlime;
    }
    public static PrismGordo GetPrismGordo(this IdentifiableType customOrNativeGordo)
    {
        if (customOrNativeGordo == null) return null;
        if (customOrNativeGordo.prefab==null) return null;
        if (!customOrNativeGordo.prefab.HasComponent<GordoIdentifiable>()) return null;
        if (PrismGordos.TryGetValue(customOrNativeGordo.ReferenceId, out var gordo)) return gordo;
        var newGordo = new PrismGordo(customOrNativeGordo, true);
        PrismGordos.Add(customOrNativeGordo.ReferenceId, newGordo);
        return newGordo;
    }
    public static PrismLargo GetPrismLargo(this SlimeDefinition customOrNativeSlime)
    {
        if (!customOrNativeSlime) return null;
        if (!customOrNativeSlime.IsLargo) return null;
        if (PrismLargos.TryGetValue(customOrNativeSlime.ReferenceId, out var largo)) return largo;
        var newSlime = new PrismLargo(customOrNativeSlime, !customOrNativeSlime.ReferenceId.Contains("Modded"));
        PrismLargos.Add(customOrNativeSlime.ReferenceId, newSlime);
        return newSlime;
    }
    
    public static PrismSlime GetPrismSlime(this SlimeDefinition customOrNativeSlime)
    {
        if (!customOrNativeSlime) return null;
        if (customOrNativeSlime.IsLargo)
        {
            if (PrismLargos.TryGetValue(customOrNativeSlime.ReferenceId, out var slime)) return slime;
            var newLargo = new PrismLargo(customOrNativeSlime, !customOrNativeSlime.ReferenceId.Contains("Modded"));
            PrismLargos.Add(customOrNativeSlime.ReferenceId, newLargo);
            return newLargo;
        }
        if (PrismBaseSlimes.TryGetValue(customOrNativeSlime.ReferenceId, out var prismSlime)) return prismSlime;
        var newBaseSlime = new PrismBaseSlime(customOrNativeSlime, !customOrNativeSlime.ReferenceId.Contains("Modded"));
        PrismBaseSlimes.Add(customOrNativeSlime.ReferenceId, newBaseSlime);
        return newBaseSlime;
        
    }

    public static PrismPlort GetPrismPlort(this IdentifiableType customOrNativePlort)
    {
        if (!customOrNativePlort) return null;
        if (!customOrNativePlort.IsPlort) return null;
        if (PrismPlorts.TryGetValue(customOrNativePlort.ReferenceId, out var plort)) return plort;
        var newPlort = new PrismPlort(customOrNativePlort, !customOrNativePlort.ReferenceId.Contains("Modded"));
        PrismPlorts.Add(customOrNativePlort.ReferenceId, newPlort);
        return newPlort;
    }
    
    public static PrismToy GetPrismToy(this ToyDefinition customOrNativeToy)
    {
        if (!customOrNativeToy) return null;
        if (PrismToys.TryGetValue(customOrNativeToy.ReferenceId, out var toy)) return toy;
        var newToy = new PrismToy(customOrNativeToy, !customOrNativeToy.ReferenceId.Contains("Modded"));
        PrismToys.Add(customOrNativeToy.ReferenceId, newToy);
        return newToy;
    }
    
    public static PrismGadget GetPrismGadget(this GadgetDefinition customOrNativeGadget)
    {
        if (!customOrNativeGadget) return null;
        if (PrismGadgets.TryGetValue(customOrNativeGadget.ReferenceId, out var gadget)) return gadget;
        var newGadget = new PrismGadget(customOrNativeGadget, !customOrNativeGadget.ReferenceId.Contains("Modded"));
        PrismGadgets.Add(customOrNativeGadget.ReferenceId, newGadget);
        return newGadget;
    }
    
    public static PrismLiquid GetPrismLiquid(this LiquidDefinition customOrNativeLiquid)
    {
        if (!customOrNativeLiquid) return null;
        if (PrismLiquids.TryGetValue(customOrNativeLiquid.ReferenceId, out var liquid)) return liquid;
        var newLiquid = new PrismLiquid(customOrNativeLiquid, !customOrNativeLiquid.ReferenceId.Contains("Modded"));
        PrismLiquids.Add(customOrNativeLiquid.ReferenceId, newLiquid);
        return newLiquid;
    }
    
    
    
    
    public static PrismVeggieFood GetPrismVeggieFood(this IdentifiableType customOrNativeVeggieFood)
    {
        if (!customOrNativeVeggieFood) return null;
        if (customOrNativeVeggieFood.IsAnimal) return null;
        if (PrismVeggieFoods.TryGetValue(customOrNativeVeggieFood.ReferenceId, out var veggieFood)) return veggieFood;
        if(!LookupEUtil.veggieFoodTypes.Contains(customOrNativeVeggieFood)) return null;
        var newVeggieFood = new PrismVeggieFood(customOrNativeVeggieFood, !customOrNativeVeggieFood.ReferenceId.Contains("Modded"));
        PrismVeggieFoods.Add(customOrNativeVeggieFood.ReferenceId, newVeggieFood);
        return newVeggieFood;
    }
    public static PrismFruitFood GetPrismFruitFood(this IdentifiableType customOrNativeFruitFood)
    {
        if (!customOrNativeFruitFood) return null;
        if (customOrNativeFruitFood.IsAnimal) return null;
        if (PrismFruitFoods.TryGetValue(customOrNativeFruitFood.ReferenceId, out var fruitFood)) return fruitFood;
        if(!LookupEUtil.fruitFoodTypes.Contains(customOrNativeFruitFood)) return null;
        var newFruitFood = new PrismFruitFood(customOrNativeFruitFood, !customOrNativeFruitFood.ReferenceId.Contains("Modded"));
        PrismFruitFoods.Add(customOrNativeFruitFood.ReferenceId, newFruitFood);
        return newFruitFood;
    }
    public static PrismNectarFood GetPrismNectarFood(this IdentifiableType customOrNativeNectarFood)
    {
        if (!customOrNativeNectarFood) return null;
        if (customOrNativeNectarFood.IsAnimal) return null;
        if (PrismNectarFoods.TryGetValue(customOrNativeNectarFood.ReferenceId, out var nectarFood)) return nectarFood;
        if(!LookupEUtil.nectarFoodTypes.Contains(customOrNativeNectarFood)) return null;
        var newNectarFood = new PrismNectarFood(customOrNativeNectarFood, !customOrNativeNectarFood.ReferenceId.Contains("Modded"));
        PrismNectarFoods.Add(customOrNativeNectarFood.ReferenceId, newNectarFood);
        return newNectarFood;
    }
    public static PrismChickenFood GetPrismChickenFood(this IdentifiableType customOrNativeChickenFood)
    {
        if (!customOrNativeChickenFood) return null;
        if (!customOrNativeChickenFood.IsAnimal) return null;
        if (PrismChickenFoods.TryGetValue(customOrNativeChickenFood.ReferenceId, out var chickenFood)) return chickenFood;
        if(!LookupEUtil.meatFoodTypes.Contains(customOrNativeChickenFood)) return null;
        var newChickenFood = new PrismChickenFood(customOrNativeChickenFood, !customOrNativeChickenFood.ReferenceId.Contains("Modded"));
        PrismChickenFoods.Add(customOrNativeChickenFood.ReferenceId, newChickenFood);
        return newChickenFood;
    }
    public static PrismBabyChickenFood GetPrismBabyChickenFood(this IdentifiableType customOrNativeBabyChickenFood)
    {
        if (!customOrNativeBabyChickenFood) return null;
        if (!customOrNativeBabyChickenFood.IsAnimal) return null;
        if (PrismBabyChickenFoods.TryGetValue(customOrNativeBabyChickenFood.ReferenceId, out var babyChickenFood)) return babyChickenFood;
        if(!LookupEUtil.chickFoodTypes.Contains(customOrNativeBabyChickenFood)) return null;
        var newBabyChickenFood = new PrismBabyChickenFood(customOrNativeBabyChickenFood, !customOrNativeBabyChickenFood.ReferenceId.Contains("Modded"));
        PrismBabyChickenFoods.Add(customOrNativeBabyChickenFood.ReferenceId, newBabyChickenFood);
        return newBabyChickenFood;
    }
    
    
    public static PrismFood GetPrismFood(this IdentifiableType customOrNativeFood)
    {
        if (!customOrNativeFood) return null;
        if (LookupEUtil.veggieFoodTypes.Contains(customOrNativeFood) && !customOrNativeFood.IsAnimal)
        {
            if (PrismVeggieFoods.TryGetValue(customOrNativeFood.ReferenceId, out var veggieFood)) return veggieFood;
            var newVeggieFood = new PrismVeggieFood(customOrNativeFood, !customOrNativeFood.ReferenceId.Contains("Modded"));
            PrismVeggieFoods.Add(customOrNativeFood.ReferenceId, newVeggieFood);
            return newVeggieFood;
        }
        if (LookupEUtil.fruitFoodTypes.Contains(customOrNativeFood) && !customOrNativeFood.IsAnimal)
        {
            if (PrismFruitFoods.TryGetValue(customOrNativeFood.ReferenceId, out var fruitFood)) return fruitFood;
            var newFruitFood = new PrismFruitFood(customOrNativeFood, !customOrNativeFood.ReferenceId.Contains("Modded"));
            PrismFruitFoods.Add(customOrNativeFood.ReferenceId, newFruitFood);
            return newFruitFood;
        }
        if (LookupEUtil.nectarFoodTypes.Contains(customOrNativeFood) && !customOrNativeFood.IsAnimal)
        {
            if (PrismNectarFoods.TryGetValue(customOrNativeFood.ReferenceId, out var nectarFood)) return nectarFood;
            var newNectarFood = new PrismNectarFood(customOrNativeFood, !customOrNativeFood.ReferenceId.Contains("Modded"));
            PrismNectarFoods.Add(customOrNativeFood.ReferenceId, newNectarFood);
            return newNectarFood;
        }
        if (LookupEUtil.meatFoodTypes.Contains(customOrNativeFood) && customOrNativeFood.IsAnimal)
        {
            if (PrismChickenFoods.TryGetValue(customOrNativeFood.ReferenceId, out var chickenFood)) return chickenFood;
            var newChickenFood = new PrismChickenFood(customOrNativeFood, !customOrNativeFood.ReferenceId.Contains("Modded"));
            PrismChickenFoods.Add(customOrNativeFood.ReferenceId, newChickenFood);
            return newChickenFood;
        }
        if (LookupEUtil.chickFoodTypes.Contains(customOrNativeFood) && customOrNativeFood.IsAnimal)
        {
            if (PrismBabyChickenFoods.TryGetValue(customOrNativeFood.ReferenceId, out var babyChickenFood)) return babyChickenFood;
            var newBabyChickenFood = new PrismBabyChickenFood(customOrNativeFood, !customOrNativeFood.ReferenceId.Contains("Modded"));
            PrismBabyChickenFoods.Add(customOrNativeFood.ReferenceId, newBabyChickenFood);
            return newBabyChickenFood;
        }
        if (PrismUnknownFoods.TryGetValue(customOrNativeFood.ReferenceId, out var food)) return food;
        var newFood = new PrismVeggieFood(customOrNativeFood, !customOrNativeFood.ReferenceId.Contains("Modded"));
        PrismVeggieFoods.Add(customOrNativeFood.ReferenceId, newFood);
        return newFood;
        
    }
    
    
    
    
    
    public static PrismIdentifiableTypeGroup GetPrismIdentifiableGroup(this IdentifiableTypeGroup group)
    {
        if (group == null) return null;
        if (PrismIdentifiableTypeGroups.TryGetValue(group, out var identifiableGroup)) return identifiableGroup;
        var newPlort = new PrismIdentifiableTypeGroup(group, true);
        PrismIdentifiableTypeGroups.Add(group, newPlort);
        return newPlort;
    }


    
    
    public static PrismPediaEntry GetPrismPediaEntry(this PediaEntry customOrNativePedia)
    {
        if (customOrNativePedia == null) return null;
        if (customOrNativePedia.TryCast<FixedPediaEntry>()!=null)
        {
            var pedia = customOrNativePedia.Cast<FixedPediaEntry>();
            if (PrismFixedPediaEntries.TryGetValue(pedia, out var entry)) return entry;
            var newEntry = new PrismFixedPediaEntry(pedia, true);
            PrismFixedPediaEntries.Add(pedia, newEntry);
            return newEntry;
        }
        if (customOrNativePedia.TryCast<IdentifiablePediaEntry>()!=null)
        {
            var pedia = customOrNativePedia.Cast<IdentifiablePediaEntry>();
            if (PrismIdentifiablePediaEntries.TryGetValue(pedia, out var entry)) return entry;
            var newEntry = new PrismIdentifiablePediaEntry(pedia, true);
            PrismIdentifiablePediaEntries.Add(pedia, newEntry);
            return newEntry;
        }
        if (SupportRadiant.HasFlag())
        {
            var radiantEntry = GetPrismPediaEntry_RadiantStuff(customOrNativePedia);
            if (radiantEntry != null) return radiantEntry;
        }
        
        if (PrismUnknownPediaEntries.TryGetValue(customOrNativePedia, out var entry2)) return entry2;
        var newEntry2 = new PrismPediaEntry(customOrNativePedia, true);
        PrismUnknownPediaEntries.Add(customOrNativePedia, newEntry2);
        return newEntry2;
        
    }

    private static PrismPediaEntry GetPrismPediaEntry_RadiantStuff(PediaEntry customOrNativePedia)
    {
        if (customOrNativePedia.TryCast<RadiantSlimePediaEntry>()!=null)
        {
            var pedia = customOrNativePedia.Cast<RadiantSlimePediaEntry>();
            if (PrismRadiantSlimePediaEntries.TryGetValue(pedia, out var entry)) return entry;
            var newEntry = new PrismRadiantSlimePediaEntry(pedia, true);
            PrismRadiantSlimePediaEntries.Add(pedia, newEntry);
            return newEntry;
        }

        return null;
    }
    
    public static PrismFixedPediaEntry GetPrismFixedPediaEntry(this FixedPediaEntry customOrNativePedia)
    {
        if (customOrNativePedia == null) return null;
        if (PrismFixedPediaEntries.TryGetValue(customOrNativePedia, out var entry)) return entry;
        var newPedia = new PrismFixedPediaEntry(customOrNativePedia, true);
        PrismFixedPediaEntries.Add(customOrNativePedia, newPedia);
        return newPedia;
    }
    public static PrismIdentifiablePediaEntry GetPrismIdentifiablePediaEntry(this IdentifiablePediaEntry customOrNativePedia)
    {
        if (customOrNativePedia == null) return null;
        if (PrismIdentifiablePediaEntries.TryGetValue(customOrNativePedia, out var entry)) return entry;
        var newPedia = new PrismIdentifiablePediaEntry(customOrNativePedia, true);
        PrismIdentifiablePediaEntries.Add(customOrNativePedia, newPedia);
        return newPedia;
    }
    public static PrismRadiantSlimePediaEntry GetPrismRadiantSlimePediaEntry(this RadiantSlimePediaEntry customOrNativePedia)
    {
        if (!SupportRadiant.HasFlag()) return null;
        if (customOrNativePedia == null) return null;
        if (PrismRadiantSlimePediaEntries.TryGetValue(customOrNativePedia, out var entry)) return entry;
        var newPedia = new PrismRadiantSlimePediaEntry(customOrNativePedia, true);
        PrismRadiantSlimePediaEntries.Add(customOrNativePedia, newPedia);
        return newPedia;
    }
}