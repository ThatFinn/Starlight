using System;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Pause;
using Starlight.Buttons;
using Starlight.Buttons.Definitions;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(PauseMenuRoot), nameof(PauseMenuRoot.Awake))]
internal static class SR2PauseMenuButtonPatch
{
    internal static List<CustomPauseMenuButton> buttons = new ();
    internal static bool SafeLock;
    internal static bool PostSafeLock;
    internal static void Prefix(PauseMenuRoot __instance)
    {
        if (!InjectPauseButtons.HasFlag()) return;
        if (SafeLock) { return; }
        SafeLock = true;
        try
        {
            dynamic pauseMenuRoot = __instance;
            PauseItemModelList pauseItemModelList = null;
            try { pauseItemModelList=pauseMenuRoot._pauseItemModelList; } catch { }
            try { pauseItemModelList=pauseMenuRoot.pauseItemModelList; } catch { }
            var items = pauseItemModelList.items;
            foreach (var button in buttons)
            {
                if (button.Label == null || button.Action == null) continue;
                try
                {
                    if (button.Model != null)
                    {
                        if (!button.Enabled)
                        {
                            if (items.Contains(button.Model))
                                items.Remove(button.Model);
                            continue;
                        }

                        if (items.Contains(button.Model))
                            continue;
                        if (!items.Contains(button.Model))
                            items.Insert(Math.Clamp(button.InsertIndex,0,items.Count), button.Model);
                        continue;
                    }

                    button.Model = ScriptableObject.CreateInstance<CustomPauseItemModel>();
                    button.Model.Action = button.Action;
                    button.Model.label = button.Label;
                    button.Model.name = button.Label.GetLocalizedString();
                    button.Model.hideFlags |= HideFlags.HideAndDontSave;
                    //button._model.prefabToSpawn = button._prefabToSpawn;

                    if (!button.Enabled)
                    {
                        if (items.Contains(button.Model))
                            items.Remove(button.Model);
                        continue;
                    }

                    if (!items.Contains(button.Model))
                        items.Insert(Math.Clamp(button.InsertIndex,0,items.Count), button.Model);

                }
                catch (Exception e) { LogError(e); }
                
            }
            
            pauseItemModelList.items = items;
            try { pauseMenuRoot._pauseItemModelList=pauseItemModelList; } catch { }
            try { pauseMenuRoot.pauseItemModelList=pauseItemModelList; } catch { }
        }
        catch (Exception e) { LogError(e);}
        SafeLock = false;
    }
}