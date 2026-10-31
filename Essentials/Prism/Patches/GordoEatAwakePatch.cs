using Starlight.Components;
using Starlight.Prism.Wrappers;
using Starlight.Storage;

namespace Starlight.Prism.Patches;

[PrismPatch()]
[HarmonyPatch(typeof(GordoEat), nameof(GordoEat.Awake))]
internal static class GordoEatAwakePatch
{
    internal static readonly Dictionary<IdentifiableType, PrismSlime> Gordos = new( );
    public static void Postfix(GordoEat __instance)
    {
        ExecuteInTicks(() =>
        {
            if (__instance == null) return;

            var identifiable = __instance.GetComponent<GordoIdentifiable>();
            if (identifiable == null) return;

            var ident = identifiable.identType;
            if (ident != null && Gordos.ContainsKey(ident))
            {
                var renderer = __instance.gameObject.GetObjectRecursively<GameObject>("Vibrating").AddComponent<StarlightSlimeRenderer>();
                var obj = renderer.RenderAppearance(Gordos[ident]).transform;
                obj.localPosition = new Vector3(0, 0.5f, 0);
                obj.localRotation = Quaternion.Euler(0,180,0);
                foreach (var child in obj.GetAllChildren())
                {
                    if (child.name.Contains("body") || child.name.Contains("slime_default") || child.name.Contains("slime_face"))
                    {
                        ExecuteInTicks((() => child.SetActive(false)), 2);
                    }
                }
            }
        },1);
    }
}