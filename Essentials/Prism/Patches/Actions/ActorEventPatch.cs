using System;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Damage;
using Starlight.Prism.Lib;
using Starlight.Storage;

namespace Starlight.Prism.Patches.Actions;

/// <summary>
/// Invokes the custom actor spawn action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(InstantiationHelpers), nameof(InstantiationHelpers.InstantiateActor))]
internal static class ActorSpawnPatch
{
    public static void Postfix(GameObject __result) => ActorEventPatch.Spawned(__result, isNew: true);
}

/// <summary>
/// Invokes the custom actor load action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(InstantiationHelpers), nameof(InstantiationHelpers.InstantiateActorFromModel))]
internal static class ActorRestoreSpawnPatch
{
    public static void Postfix(GameObject __result) => ActorEventPatch.Spawned(__result, isNew: false);
}

/// <summary>
/// Invokes the custom slime death action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(DeathHandler), nameof(DeathHandler.Kill))]
internal static class SlimeKillPatch
{
    public static void Prefix(GameObject gameObject, Damage damage)
    {
        try
        {
            SlimeEvents.NotifyDeath(gameObject, damage);
        }
        catch (Exception ex)
        {
            LogError("Prism: A slime's custom death action failed", ex);
        }
    }
}

/// <summary>
/// Invokes the custom vac in action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(Vacuumable), nameof(Vacuumable.TryConsume))]
internal static class ActorVacInPatch
{
    public static void Postfix(Vacuumable __instance, bool __result)
    {
        if (!__result) return;

        ActorEventPatch.Invoke("custom vac in action", () =>
        {
            SlimeEvents.NotifyVacIn(__instance.gameObject);
            ItemEvents.NotifyVacIn(__instance.gameObject);
        });
    }
}

/// <summary>
/// Invokes the custom vac shoot action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(Vacuumable), nameof(Vacuumable.Launch))]
internal static class ActorShootOutPatch
{
    public static void Postfix(Vacuumable __instance, Vacuumable.LaunchSource source)
    {
        ActorEventPatch.Invoke("custom shoot out action", () =>
        {
            SlimeEvents.NotifyShootOut(__instance.gameObject, source);
            ItemEvents.NotifyShootOut(__instance.gameObject, source);
        });
    }
}

/// <summary>
/// Invokes the custom vac hold action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(Vacuumable), nameof(Vacuumable.Hold))]
internal static class SlimeHoldPatch
{
    public static void Postfix(Vacuumable __instance) =>
        ActorEventPatch.Invoke("custom hold action", () =>
        {
            SlimeEvents.NotifyHold(__instance.gameObject);
            ToyEvents.NotifyHold(__instance.gameObject);
        });
}

/// <summary>
/// Invokes the custom eat(en) action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(SlimeEat), nameof(SlimeEat.InvokeOnEat))]
internal static class SlimeEatPatch
{
    public static void Postfix(SlimeEat __instance, IdentifiableType otherId, bool eatingLaunchedFood, bool isFavorite) =>
        ActorEventPatch.Invoke("custom eat action", () =>
        {
            SlimeEvents.NotifyAte(__instance.gameObject, otherId, isFavorite, eatingLaunchedFood);
            ItemEvents.NotifyEaten(otherId, __instance.gameObject, isFavorite, eatingLaunchedFood);
        });
}

/// <summary>
/// Invokes the custom slime produce plort action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(SlimeEat), nameof(SlimeEat.Produce))]
internal static class SlimeProducePatch
{
    public static void Postfix(SlimeEat __instance, int count, GameObject produces) =>
        ActorEventPatch.Invoke("custom produce action", () =>
        {
            var plort = produces == null ? null : produces.GetComponent<IdentifiableActor>();
            SlimeEvents.NotifyProduce(__instance.gameObject, plort == null ? null : plort.identType, count);
        });
}

/// <summary>
/// Invokes the custom slime damage action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(SlimeHealth), nameof(SlimeHealth.Damage))]
internal static class SlimeDamagePatch
{
    public static void Postfix(SlimeHealth __instance, Damage damage, bool __result)
    {
        if (!__result) return;

        ActorEventPatch.Invoke("custom damage action", () =>
            SlimeEvents.NotifyDamaged(__instance.gameObject, damage, __instance.GetCurrHealth()));
    }
}

/// <summary>
/// Invokes the custom destroy action
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(Destroyer), nameof(Destroyer.Destroy), new Type[] { typeof(UnityEngine.Object), typeof(float), typeof(string), typeof(bool), typeof(bool) })]
internal static class ActorRemovePatch
{
    public static void Prefix(Object instance, float t, string source)
    {
        if (!SlimeEvents.AnyAction && !ItemEvents.AnyAction) return;

        ActorEventPatch.Invoke("custom remove action", () =>
        {
            var removed = instance == null ? null : instance.TryCast<GameObject>();
            if (removed == null) return;

            SlimeEvents.NotifyRemoved(removed, source, t);
            ItemEvents.NotifyRemoved(removed, source, t);
        });
    }
}

internal static class ActorEventPatch
{
    internal static void Spawned(GameObject? spawned, bool isNew) => Invoke("custom spawn action", () =>
    {
        SlimeEvents.NotifySpawn(spawned, isNew);
        ItemEvents.NotifySpawn(spawned, isNew);
    });
    
    internal static void Invoke(string what, Action notify)
    {
        try
        {
            notify();
        }
        catch (Exception ex)
        {
            LogError($"Prism: A {what} failed", ex);
        }
    }
}