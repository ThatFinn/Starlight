using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using System;
using System.Runtime.InteropServices;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.Layout;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu.Model;
using Starlight.Components.AssetBundle;
using Starlight.Enums;
using Starlight.Popups;
using Starlight.Storage;
using Starlight.UI.Blueprints;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Starlight.Patches.MainMenu;
[HarmonyPatch()]
internal static class SaveGameRootUIPatch
{
    private static Button _exportButton;
    private static Button _iconButton;
    private static bool _addedAction;
    private static InputActionReference _onPress;
    private static InputEvent _inputEvent;
    private static SaveGamesRootUI _ui;

    private static readonly Action<InputEventData> Action = _ => OnExportButtonPressed();
    static void OnExportButtonPressed()
    {
        if (!_ui) return;
        if (!_exportButton.gameObject.active) return;
        if (NewSaveRow.IsRow(_ui._selectedModelIndex)) return;
        var dataBehaviours = _ui.FetchButtonBehaviorData();
        var load = dataBehaviours[_ui._selectedModelIndex];
        var loadGameBehaviorModel = load.TryCast<LoadGameBehaviorModel>();
        if (loadGameBehaviorModel==null)
        {
            var ofn = new OPENFILENAME();
            if (GetOpenFileName(ofn))
            {
                var filePath = ofn.lpstrFile;
                if (string.IsNullOrEmpty(filePath)) return;
                var savefile = StarlightSaveFileV01.Load(File.ReadAllBytes(filePath));
                var slot = NewSaveRow.SlotOf(_ui._selectedModelIndex);

                var error = SaveFileEUtil.ImportSaveV01(savefile, slot, true);
                if (error != StarlightError.NoError)
                {
                    Log(Tr("messages.save.import.error",error));
                    StarlightConfirmationViewerPopUp.Open(Tr("messages.save.import.error",error),null,null);
                    return;
                }
                systemContext.SceneLoader.LoadMainMenuSceneGroup();
            }
        }
        else
        {
            var sfn = new SAVEFILENAME();
            if (GetSaveFileName(sfn))
            {
                var filePath = sfn.lpstrFile;
                if (string.IsNullOrEmpty(filePath)) return;
                
                var error = SaveFileEUtil.ExportSaveV01(loadGameBehaviorModel.GameDataSummary, out StarlightSaveFileV01 savefile);
                if (error != StarlightError.NoError)
                {
                    Log(Tr("messages.save.export.error",error));
                    StarlightConfirmationViewerPopUp.Open(Tr("messages.save.export.error",error),null,null);
                    return;
                }
                if(filePath.EndsWith(".json")) File.WriteAllText(filePath,savefile.Export());
                else File.WriteAllBytes(filePath,savefile.ExportCompressed());
            }
        }
    }

    static void ScrollTo(ScrollRect scroll,RectTransform target)
    {
        var minus = 0f;
        
        foreach (var child in scroll.content.transform.GetChildren())
        {
            if (!child.gameObject.activeSelf) continue;
            minus = child.GetComponent<RectTransform>().offsetMax.y;
            break;
        }
        var siblingBefore = target.parent.GetChild(target.GetSiblingIndex() - 6);
        var upperBorder = (siblingBefore.gameObject.activeSelf ? Math.Abs(siblingBefore.GetComponent<RectTransform>().offsetMin.y) : 0f)+minus;

        var siblingAfterIndex = target.GetSiblingIndex() + 0;
        if (target.parent.childCount <= siblingAfterIndex) siblingAfterIndex = target.parent.childCount - 1;
        var siblingAfter = target.parent.GetChild(siblingAfterIndex);
        
        var lowerBorder = Mathf.Abs(siblingAfter.GetComponent<RectTransform>().offsetMax.y)+minus;

        if (upperBorder > scroll.content.anchoredPosition.y)
            scroll.content.anchoredPosition = new Vector2(scroll.content.anchoredPosition.x,upperBorder);
        else if (lowerBorder < scroll.content.anchoredPosition.y)
            scroll.content.anchoredPosition = new Vector2(scroll.content.anchoredPosition.x,lowerBorder);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(SaveGamesRootUI), nameof(SaveGamesRootUI.OnItemSelect))]
    internal static void OnItemSelect(SaveGamesRootUI __instance, int index)
    {
        try
        {
            var rect = __instance.gameObject.GetObjectRecursively<ScrollRect>("ButtonsScrollView");
            if (rect == null) return;
            int activeIndex = 0;
            foreach (var child in rect.content.transform.GetChildren())
            {
                if (!child.gameObject.activeSelf) continue;
                if (activeIndex == index)
                {
                    ScrollTo(rect,child.GetComponent<RectTransform>());
                    return;
                }
                activeIndex++;
            }
        }
        catch {}
        
    }

    [HarmonyPostfix,HarmonyPatch(typeof(SaveGamesRootUI), nameof(SaveGamesRootUI.FocusUI))]
    internal static void Postfix(SaveGamesRootUI __instance)
    {
        StarlightEntryPoint.BaseUIAddSliders.Add(__instance);
        if (!AllowSaveExport.HasFlag()) return;
        _ui = __instance;
        if (__instance.name.Contains("SRLE")) return;
        ExecuteInTicks((() =>
        {
            var actionPanel = _ui.gameObject.GetObjectRecursively<RectTransform>("ActionPanel");
            if (actionPanel.GetObjectRecursively<Button>("ExportButton") != null) return;
            _iconButton = actionPanel.GetObjectRecursively<Button>("IconButton");
            _exportButton = GameObject.Instantiate(_iconButton, actionPanel);
            _exportButton.name = "ExportButton";
            _exportButton.onClick.RemoveAllListeners();
            var exportRectTransform = _exportButton.GetComponent<RectTransform>();
            exportRectTransform.anchorMax = new Vector2(1,1);
            exportRectTransform.anchorMin = new Vector2(1,1);
            _exportButton.transform.localPosition = new Vector3(566.4542f, 850.0162f, -16.1379f);
            _exportButton.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = EmbeddedResourceEUtil.LoadSprite("Assets.icon.png").CopyWithoutMipmaps();
            var iconRect = _exportButton.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
            iconRect.sizeDelta *= 0.75f;
            var sortGroup = iconRect.AddComponent<SortingGroup>();
            sortGroup.enabled = false;
            sortGroup.sortingOrder = Mathf.FloorToInt(10 * UIBlueprint.ScaleFactor);
            iconRect.AddComponent<RoundedUIImage>().CornerRadius = 10 * UIBlueprint.ScaleFactor;
            if(_onPress==null) _onPress = Get<InputActionReference>("MainGame/Open Map");
            if(_inputEvent==null) _inputEvent = Get<InputEvent>("OpenMap");
            var inputEventDisplay = _exportButton.gameObject.GetObjectRecursively<InputEventDisplay>("KeyIcon");
            inputEventDisplay._inputEvent = _inputEvent;
            inputEventDisplay.HandleKeysChanged();
            _exportButton.GetComponent<InputEventButton>().InputEvent = _inputEvent;
            _exportButton.GetComponent<InputEventButton>().Awake();
            _exportButton.GetComponent<LayoutManager>().ForceTreeRebuild();
            _onPress?.action.Enable();
            if(!_addedAction)
            {
                _inputEvent?.add_Performed(Action);
                _addedAction = true;
            }
        }), 2);
    }

    [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetSaveFileName([In, Out] SAVEFILENAME ofn);
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class SAVEFILENAME
    {
        public int lStructSize = Marshal.SizeOf(typeof(SAVEFILENAME));
        public IntPtr hwndOwner = IntPtr.Zero;
        public IntPtr hInstance = IntPtr.Zero;
        public string lpstrFilter = "SR2 Save Files (*"+StarlightSaveFileV01.Extension+")\0*"+StarlightSaveFileV01.Extension+"\0All Files\0*.*\0";
        public string lpstrCustomFilter = null;
        public int nMaxCustFilter = 0;
        public int nFilterIndex = 1;
        public string lpstrFile = new string(new char[512]);
        public int nMaxFile = 260;
        public string lpstrFileTitle = null;
        public int nMaxFileTitle = 0;
        public string lpstrInitialDir = null;
        public string lpstrTitle = "Save Game File";
        public int Flags = 0x00000002 | 0x00080000;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt = StarlightSaveFileV01.Extension.Substring(1);
        public IntPtr lCustData = IntPtr.Zero;
        public IntPtr lpfnHook = IntPtr.Zero;
        public string lpTemplateName = null;
    }
    [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetOpenFileName([In, Out] OPENFILENAME ofn);
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class OPENFILENAME
    {
        public int lStructSize = Marshal.SizeOf(typeof(OPENFILENAME));
        public IntPtr hwndOwner = IntPtr.Zero;
        public IntPtr hInstance = IntPtr.Zero;
        public string lpstrFilter = "SR2 Save Files (*"+StarlightSaveFileV01.Extension+")\0*"+StarlightSaveFileV01.Extension+"\0All Files\0*.*\0";
        public string lpstrCustomFilter = null;
        public int nMaxCustFilter = 0;
        public int nFilterIndex = 1;
        public string lpstrFile = new string(new char[512]);
        public int nMaxFile = 260;
        public string lpstrFileTitle = null;
        public int nMaxFileTitle = 0;
        public string lpstrInitialDir = null;
        public string lpstrTitle = "Open Save File";
        public int Flags = 0x00000008 | 0x00001000;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt = StarlightSaveFileV01.Extension.Substring(1);
        public IntPtr lCustData = IntPtr.Zero;
        public IntPtr lpfnHook = IntPtr.Zero;
        public string lpTemplateName = null;
    }
}