using Starlight.Prism.Creators;
using Starlight.Storage;

namespace Starlight.Prism.Patches;

[PrismPatch]
[HarmonyPatch(typeof(GadgetDirector), nameof(GadgetDirector.Awake))]
internal static class GadgetDirectorAwakePatch
{
    public static void Postfix()
    {
        try
        {
            PrismGadgetCreatorV02.JoinGroups();
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to make created gadgets members of their groups", ex);
        }
    }
}
