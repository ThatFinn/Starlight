using System;
using Il2CppInterop.Runtime;
using Starlight.Buttons;
using Il2CppMonomiPark.SlimeRancher.UI.RanchHouse;

namespace Starlight.Patches.InGame;

[HarmonyPatch(typeof(RanchHouseMenuRoot), nameof(RanchHouseMenuRoot.SetViewModel))]
internal static class SR2RanchUIButtonPatch
{
    internal static readonly List<CustomRanchUIButton> Buttons = new ();
    private static bool _safeLock;
    internal static bool PostSafeLock;
    
    internal static void Prefix(RanchHouseMenuRoot __instance, RanchHouseViewModel viewModel)
    {
        if (!InjectRanchUIButtons.HasFlag()) return;
        if (viewModel == null) return;
        if (_safeLock) { return; }
        _safeLock = true;
        try
        {
            foreach (var button in Buttons)
            {
                if (button.Label == null || button.Action == null) continue;
                try
                {
                    if (!button.Enabled)
                    {
                        if (button.Model != null && viewModel._items.Contains(button.Model))
                            viewModel._items.Remove(button.Model);
                        continue;
                    }
                    
                    if (button.Model != null)
                    {
                        if (viewModel._items.Contains(button.Model))
                            continue;
                        viewModel._items.Insert(Math.Clamp(button.InsertIndex, 0, viewModel._items.Count), button.Model);
                        continue;
                    }
                    
                    RanchHouseCloseItemData data = ScriptableObject.CreateInstance<RanchHouseCloseItemData>();
                    data._label = button.Label;
                    data.name = button.Label.GetLocalizedString();
                    data.hideFlags |= HideFlags.HideAndDontSave;
                    
                    RanchHouseCloseItemViewModel model = new RanchHouseCloseItemViewModel(data);
                    model.CloseRequested +=
                        DelegateSupport.ConvertDelegate<Il2CppSystemAction>(
                            new SystemAction(() => button.Action?.Invoke())
                        );
                    button.Model = model;
                    
                    if (!viewModel._items.Contains(button.Model))
                        viewModel._items.Insert(Math.Clamp(button.InsertIndex, 0, viewModel._items.Count), button.Model);
                }
                catch (Exception e) { LogError(e); }
            }
        }
        finally
        {
            _safeLock = false;
        }
    }
}