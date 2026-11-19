using Il2CppMonomiPark.SlimeRancher.World;

namespace Starlight.Prism;

public static class Callbacks
{
    public delegate void OnPlortSoldEvent(int amount, IdentifiableType id);
    public delegate void OnZoneEnterEvent(ZoneDefinition zone);
    public delegate void OnZoneExitEvent(ZoneDefinition zone);

    public static event OnPlortSoldEvent OnPlortSold;
    public static event OnZoneEnterEvent OnZoneEnter;
    public static event OnZoneExitEvent OnZoneExit;
    
    internal static void Invoke_onPlortSold(int amount, IdentifiableType id) => OnPlortSold?.Invoke(amount, id);
    internal static void Invoke_onZoneEnter(ZoneDefinition zone) => OnZoneEnter?.Invoke(zone);
    internal static void Invoke_onZoneExit(ZoneDefinition zone) => OnZoneExit?.Invoke(zone);

}