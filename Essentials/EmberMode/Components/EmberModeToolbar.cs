using Il2CppSystem.Linq;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Starlight.Components;
using Starlight.Enums;
using Starlight.Menus;
using Starlight.Menus.Development;
using Starlight.Storage;
using Starlight.Managers;
using Starlight.Popups;
using Starlight.UI;
using Starlight.UI.Blueprints;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Starlight.EmberMode.Components;

[InjectIntoIL]
internal class EmberModeToolbar : MonoBehaviour
{
    void Start()
    {
        var rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas && !canvas.GetComponent<GraphicRaycaster>())
            canvas.gameObject.AddComponent<GraphicRaycaster>();

        var theme = EmberModeUITheme.Create();
        var fontTheme = new FontTheme();

        BuildMenubar().Render(theme, fontTheme, rect);
    }

    private GameObject _currentlyHovered;
    private int _debugFrame = 0;

    void Update()
    {
        _debugFrame++;

        if (!EventSystem.current)
        {
            if (_debugFrame % 120 == 0) Log("[EmberModeToolbar] EventSystem.current is NULL");
            return;
        }

        var mousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;

        if (DebugLogging.HasFlag() && Mouse.current?.leftButton.wasPressedThisFrame == true)
            Log($"[EmberModeToolbar] LEFT CLICK at {mousePos}");

        var pointerEventData = new PointerEventData(EventSystem.current) { position = mousePos };
        var results = new Il2CppSystem.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        if (DebugLogging.HasFlag() && Mouse.current?.leftButton.wasPressedThisFrame == true)
        {
            Log($"[EmberModeToolbar] Raycast hit count: {results.Count}, Registry size: {MenubarRegistry.Interactions.Count}");
            foreach (var r in results)
                Log($"[EmberModeToolbar]   hit: {r.gameObject?.name} (id={r.gameObject?.GetInstanceID()}) | inRegistry: {MenubarRegistry.Interactions.ContainsKey(r.gameObject.GetInstanceID())}");
        }

        GameObject hitObject = null;
        foreach (var result in results)
            if (MenubarRegistry.Interactions.ContainsKey(result.gameObject.GetInstanceID()))
            {
                hitObject = result.gameObject;
                break;
            }

        if (hitObject != _currentlyHovered)
        {
            if (_currentlyHovered && MenubarRegistry.Interactions.TryGetValue(_currentlyHovered.GetInstanceID(), out var oldInt))
            {
                oldInt.OnHoverExit?.Invoke();
                if (oldInt.TargetImage) 
                    oldInt.TargetImage.color = oldInt.NormalColor;
            }

            _currentlyHovered = hitObject;

            if (_currentlyHovered && MenubarRegistry.Interactions.TryGetValue(_currentlyHovered.GetInstanceID(), out var newInt))
            {
                newInt.OnHoverEnter?.Invoke();
                if (newInt.TargetImage) 
                    newInt.TargetImage.color = newInt.HoverColor;
            }
        }

        
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
        {
            if (hitObject && MenubarRegistry.Interactions.TryGetValue(hitObject.GetInstanceID(), out var hitInt))
            {
                if(DebugLogging.HasFlag())
                    Log($"[EmberModeToolbar] Invoking OnClick for: {hitObject.name}");
                hitInt.OnClick?.Invoke();
            }
            else if(DebugLogging.HasFlag())
                Log($"[EmberModeToolbar] Click — no matching registry object hit");
        }


        if (hitObject && MenubarRegistry.Interactions.TryGetValue(hitObject.GetInstanceID(), out var pressInt))
        {
            if (pressInt.TargetImage)
                pressInt.TargetImage.color = (Mouse.current?.leftButton.isPressed == true)
                    ? pressInt.PressedColor
                    : pressInt.HoverColor;
        }
    }

    private static MenubarUIBlueprintV01 BaseMenubar() => new()
    {
        Name = "EmberModeMenubar",
        Anchors = new Vector4(0, 1, 1, 1),
        Pivot = new Vector2(0.5f, 1f),
        Size = Vector2.zero,

        BarHeight = 18f,
        EntryHeight = 16f,
        LabelFontSize = 10f,
        DropdownWidth = 220f,
    };

    private static List<MenubarEntry> BuildSaveSlotEntries(string prefix, Dictionary<int, Summary> slotSummaries, bool disableIfEmpty, bool forceDisable, System.Func<int, Summary, SystemAction> actionBuilder)
    {
        var chunkedEntries = new List<MenubarEntry>();
        var maxSlots = SAVESLOT_COUNT.Get();
        int chunkSize = 15;
        
        try
        {
            for (int chunkStart = 0; chunkStart < maxSlots; chunkStart += chunkSize)
            {
                int chunkEnd = System.Math.Min(chunkStart + chunkSize, maxSlots);
                var chunkSubEntries = new List<MenubarEntry>();
                
                chunkSubEntries.Add(new MenubarEntry
                {
                    Label = "Split for performance reasons",
                    IsDisabled = true
                });
                chunkSubEntries.Add(new MenubarEntry { IsSeparator = true });
                
                bool anyValid = false;
                for (int j = chunkStart; j < chunkEnd; j++)
                {
                    int slotIndex = j;
                    bool hasSummary = slotSummaries.TryGetValue(slotIndex, out var summary);
                    
                    if (hasSummary || !disableIfEmpty)
                    {
                        anyValid = true;
                        string displayName = hasSummary ? (!string.IsNullOrEmpty(summary.DisplayName) ? summary.DisplayName : summary.Name) : "Empty";
                        chunkSubEntries.Add(new MenubarEntry
                        {
                            Label = $"Slot {slotIndex + 1} - {displayName}",
                            OnClick = actionBuilder(slotIndex, summary)
                        });
                    }
                    else
                    {
                        chunkSubEntries.Add(new MenubarEntry
                        {
                            Label = $"Slot {slotIndex + 1} - Empty",
                            IsDisabled = true
                        });
                    }
                }
                
                
                if (anyValid || !disableIfEmpty)
                {
                    chunkedEntries.Add(new MenubarEntry
                    {
                        Label = $"{prefix} (Slots {chunkStart + 1} - {chunkEnd})",
                        SubEntries = chunkSubEntries,
                        IsDisabled = forceDisable
                    });
                }
            }
        }
        catch (System.Exception e) { LogError(e); }
        
        return chunkedEntries;
    }

    private static MenubarUIBlueprintV01 BuildMenubar()
    {
        var menubar = BaseMenubar();

        var fileItem = new MenubarItem { Label = Tr("embermode.toolbar.file") };
        fileItem.OnBeforeOpen = () =>
        {
            fileItem.Entries.Clear();
            
            
            var slotSummaries = new Dictionary<int, Summary>();
            if (gameContext && autoSaveDirector != null)
            {
                try
                {
                    foreach (var summary in autoSaveDirector.EnumerateAllSaveGamesIncludingBackups().ToList())
                    {
                        if (summary.IsInvalid) continue;
                        slotSummaries[summary.SaveSlotIndex] = summary;
                    }
                }
                catch { }
            }
            
            
            fileItem.Entries.Add(new MenubarEntry { Label = Tr("embermode.toolbar.file.save"), IsDisabled = !inGame, OnClick = HandleFileSave() });
            fileItem.Entries.Add(new MenubarEntry { IsSeparator = true });
            
            
            var saveAsEntries = BuildSaveSlotEntries("Save As", slotSummaries, false, !inGame, (index, summary) => HandleFileSaveAs(index, summary));
            fileItem.Entries.AddRange(saveAsEntries);
            fileItem.Entries.Add(new MenubarEntry { IsSeparator = true });
            
            
            var loadEntries = BuildSaveSlotEntries("Load", slotSummaries, true, false, (index, summary) => HandleFileLoad(index, summary));
            fileItem.Entries.AddRange(loadEntries);
            fileItem.Entries.Add(new MenubarEntry { IsSeparator = true });
            
            
            var deleteEntries = BuildSaveSlotEntries("Delete", slotSummaries, true, false, (index, summary) => HandleFileDelete(index, summary));
            fileItem.Entries.AddRange(deleteEntries);
        };
        menubar.Items.Add(fileItem);

        var settingsItem = new MenubarItem { Label = LanguageEUtil.Tr("embermode.toolbar.settings") };
        settingsItem.OnBeforeOpen = () =>
        {
            bool isLoaded = sceneContext && sceneContext.Camera;
            bool isNoclip = isLoaded && sceneContext.Camera.GetComponent<NoClipComponent>();
            bool infEnergy = isLoaded && StarlightSaveManager.inGameData != null && StarlightSaveManager.inGameData.InfiniteEnergyActive;
            bool infHealth = isLoaded && StarlightSaveManager.inGameData != null && StarlightSaveManager.inGameData.InfiniteHealthActive;
            
            settingsItem.Entries = new List<MenubarEntry>
            {
                new MenubarEntry 
                { 
                    Label = "NoClip", 
                    IsDisabled = !isLoaded,
                    IsChecked = () => isNoclip, 
                    OnClick = () => {
                        if (!isLoaded) return;
                        StarlightNoClipManager.Toggle();
                    } 
                },
                new MenubarEntry 
                { 
                    Label = "Infinite Energy", 
                    IsDisabled = !inGame,
                    IsChecked = () => infEnergy, 
                    OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("infenergy true", true); } 
                },
                new MenubarEntry 
                { 
                    Label = "Infinite Health", 
                    IsDisabled = !inGame,
                    IsChecked = () => infHealth, 
                    OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("infhealth", true); } 
                }
            };
        };
        menubar.Items.Add(settingsItem);

        
        var debugItem = new MenubarItem { Label = LanguageEUtil.Tr("embermode.toolbar.debug") };
        debugItem.OnBeforeOpen = () =>
        {
            debugItem.Entries = new List<MenubarEntry>
            {
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.giveupgrades"), IsDisabled = !inGame, OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("upgrade set * 10", true); } },
                new MenubarEntry { IsSeparator = true },
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.unlockpedia"), IsDisabled = !inGame, OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("pedia unlock * false", true); } },
                new MenubarEntry { IsSeparator = true },
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.clearinv"), IsDisabled = !inGame, OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("clearinv", true); } },
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.refillinv"), IsDisabled = !inGame, OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("refillinv", true); } },
                new MenubarEntry { IsSeparator = true },
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.copyloc"), IsDisabled = !inGame, OnClick = () => { if (inGame) { try { GUIUtility.systemCopyBuffer = Warp.CurrentLocation().ToString(); } catch { } } } },
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.teleportloc"), IsDisabled = !inGame, OnClick = () => { if (inGame) { try { Warp.FromString(GUIUtility.systemCopyBuffer.Trim()).WarpPlayerThere(); } catch { } } } },
                new MenubarEntry { IsSeparator = true },
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.addcredits"), IsDisabled = !inGame, OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("newbucks 1000", true); } },
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.removecredits"), IsDisabled = !inGame, OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("newbucks -1000", true); } },
                new MenubarEntry { IsSeparator = true },
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.dectime"), IsDisabled = !inGame, OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("fastforward -1", true); } },
                new MenubarEntry { Label = LanguageEUtil.Tr("embermode.toolbar.debug.inctime"), IsDisabled = !inGame, OnClick = () => { if (inGame) StarlightCommandManager.ExecuteByString("fastforward 1", true); } },
            };
        };
        menubar.Items.Add(debugItem);

        var sceneGroupItem = new MenubarItem { Label = "SceneGroup" };
        sceneGroupItem.OnBeforeOpen = () =>
        {
            sceneGroupItem.Entries = GetAllSceneGroups();
        };
        menubar.Items.Add(sceneGroupItem);

        var warpsItem = new MenubarItem { Label = Tr("embermode.toolbar.warps") };
        warpsItem.OnBeforeOpen = () =>
        {
            warpsItem.Entries.Clear();
            if (StarlightSaveManager.data.warps == null || StarlightSaveManager.data.warps.Count == 0)
            {
                warpsItem.Entries.Add(new MenubarEntry { Label = Tr("embermode.toolbar.warps.nowarps"), IsDisabled = true });
            }
            else
            {
                foreach (var kvp in StarlightSaveManager.data.warps)
                {
                    var warpName = kvp.Key;
                    warpsItem.Entries.Add(new MenubarEntry 
                    { 
                        Label = warpName, 
                        SubEntries = new List<MenubarEntry>
                        {
                            new MenubarEntry 
                            { 
                                Label = Tr("embermode.toolbar.warps.teleport"), 
                                IsDisabled = !inGame,
                                OnClick = () => 
                                {
                                    if (!inGame) return;
                                    var w = StarlightWarpManager.GetWarp(warpName);
                                    if (w != null) w.WarpPlayerThere();
                                }
                            },
                            new MenubarEntry 
                            { 
                                Label = Tr("embermode.toolbar.file.delete"), 
                                OnClick = () => 
                                {
                                    StarlightWarpManager.RemoveWarp(warpName);
                                }
                            }
                        }
                    });
                }
            }
        };
        menubar.Items.Add(warpsItem);

        menubar.Items.AddRange(
        [
            new MenubarItem
            {
                Label = "Windows",
                Entries =
                [
                    new MenubarEntry { Label = Tr("embermode.toolbar.tools.modmenu"), OnClick = (()=>{MenuEUtil.GetMenu<StarlightModMenu>().Open();}) },
                    new MenubarEntry { Label = Tr("embermode.toolbar.tools.studio"), OnClick = (()=>{MenuEUtil.GetMenu<StarlightStudioMenu>().Open();}) },
                    new MenubarEntry { IsSeparator = true },
                    new MenubarEntry { 
                        Label = "Transform Inspector", 
                        IsChecked = () => EmberModeTransformInspector.Instance && EmberModeTransformInspector.Instance.WindowRoot && EmberModeTransformInspector.Instance.WindowRoot.activeSelf,
                        OnClick = () => {
                            if (EmberModeTransformInspector.Instance && EmberModeTransformInspector.Instance.WindowRoot)
                            {
                                EmberModeTransformInspector.Instance.WindowRoot.SetActive(!EmberModeTransformInspector.Instance.WindowRoot.activeSelf);
                            }
                        } 
                    },
                ]
            },
            new MenubarItem
            {
                Label = Tr("embermode.toolbar.time"),
                Entries =
                [
                    new MenubarEntry { Label = "0x",  OnClick = HandleTime(0.0f) },
                    new MenubarEntry { IsSeparator = true },
                    new MenubarEntry { Label = "0.1x",  OnClick = HandleTime(0.1f) },
                    new MenubarEntry { Label = "0.25x",  OnClick = HandleTime(0.25f) },
                    new MenubarEntry { Label = "0.5x",  OnClick = HandleTime(0.5f) },
                    new MenubarEntry { Label = "0.75x",  OnClick = HandleTime(0.75f) },
                    new MenubarEntry { IsSeparator = true },
                    new MenubarEntry { Label = "1x",  OnClick = HandleTime(1.0f) },
                    new MenubarEntry { IsSeparator = true },
                    new MenubarEntry { Label = "1.5x",  OnClick = HandleTime(1.5f) },
                    new MenubarEntry { Label = "2x",  OnClick = HandleTime(2.0f) },
                    new MenubarEntry { Label = "2.5x",  OnClick = HandleTime(2.5f) },
                    new MenubarEntry { Label = "3.0x",  OnClick = HandleTime(3.0f) },
                    new MenubarEntry { Label = "4.0x",  OnClick = HandleTime(4.0f) },
                    new MenubarEntry { Label = "5.0x",  OnClick = HandleTime(5.0f) },
                    new MenubarEntry { Label = "10.0x",  OnClick = HandleTime(10.0f) },
                    new MenubarEntry { Label = "15.0x",  OnClick = HandleTime(15.0f) },
                ]
            }
        ]);

        menubar.Items.Add(new MenubarItem
        {
            Label = "Mode",
            Entries =
            [
                new MenubarEntry { Label = "Gameplay", IsChecked = () => EmberModeMode.Current == EmberModeMode.Mode.Gameplay, OnClick = () => EmberModeMode.SetMode(EmberModeMode.Mode.Gameplay) },
                new MenubarEntry { Label = "Transform", IsChecked = () => EmberModeMode.Current == EmberModeMode.Mode.Transform, OnClick = () => EmberModeMode.SetMode(EmberModeMode.Mode.Transform) },
            ]
        });

        return menubar;
    }

    private static List<MenubarEntry> GetAllSceneGroups()
    {
        var list = new List<MenubarEntry>()
        {
            new MenubarEntry { Label = "This may break the game/mods!", IsDisabled = true, },
            new MenubarEntry { Label = "Do not report issues when using this!", IsDisabled = true, },
       };
        foreach (var group in systemContext.SceneLoader.SceneGroupList.items)
        {
            var type = group.IsGameplay ? "Type: Gameplay" : "Type: ";
            if (group._isEditor)
                type += type == "Type: " ? "Editor" : ", Editor";
            if (type == "Type: ")
                type += "None";
            list.Add(new MenubarEntry 
            { 
                Label = group.ReferenceId, 
                SubEntries = new List<MenubarEntry>
                {
                    new MenubarEntry { Label = type, IsDisabled = true },
                    new MenubarEntry { IsSeparator = true },
                    new MenubarEntry { Label = Tr("embermode.toolbar.scenes.loadscene"), OnClick = () => { systemContext.SceneLoader.LoadSceneGroup(group, null); } },
                    new MenubarEntry { Label = Tr("embermode.toolbar.scenes.loadlocation"), OnClick = () => { LocationBookmarksUtil.GoToLocationPlayer(group, new Vector3(0, 50, 0) + new Vector3(0, LocationBookmarksUtil.PLAYER_HEIGHT / 2f, 0), Vector3.zero); } }
                }
            });
        }
        return list;
    }
    
    private static SystemAction HandleFileSaveAs(int slotIndex, Summary summary) => (() =>
    {
        if (!inGame) return;
        StarlightConfirmationViewerPopUp.OpenYesNo(
            $"Are you sure you want to Save As to Slot {slotIndex + 1}? Your current progress will be copied to the new slot, but you will REMAIN playing on your current slot.",
            () => {
                var currentSummary = GetCurrentSummarySafe();
                
                
                StarlightSaveFileV01 oldData = null;
                if (currentSummary != null && !currentSummary.IsInvalid)
                {
                    SaveFileEUtil.ExportSaveV01(currentSummary, out oldData);
                }
                
                
                autoSaveDirector.SaveGameAndFlush();
                
                currentSummary = GetCurrentSummarySafe();
                if (currentSummary == null) return;
                
                
                if (SaveFileEUtil.ExportSaveV01(currentSummary, out var newData) == StarlightError.NoError)
                {
                    
                    SaveFileEUtil.ImportSaveV01(newData, slotIndex + 1, false);
                    
                    
                    if (oldData != null)
                    {
                        SaveFileEUtil.ImportSaveV01(oldData, currentSummary.SaveSlotIndex + 1, false);
                    }
                }
            },
            () => {  }
        );
    });

    private static SystemAction HandleFileLoad(int slotIndex, Summary summary) => (() =>
    {
        StarlightConfirmationViewerPopUp.OpenYesNo(
            $"Are you sure you want to load the save in Slot {slotIndex + 1}? Any unsaved progress will be lost.",
            () => {
                MenuEUtil.CloseOpenMenu();
                MenuEUtil.CloseOpenPopUps();
                SaveFileEUtil.LoadSaveBySlotIndex(slotIndex);
            },
            () => {  }
        );
    });

    private static SystemAction HandleFileDelete(int slotIndex, Summary summary) => (() =>
    {
        StarlightConfirmationViewerPopUp.OpenYesNo(
            $"Are you sure you want to PERMANENTLY DELETE the save in Slot {slotIndex + 1}? This cannot be undone.",
            () => {
                if (summary != null && !summary.IsInvalid)
                {
                    SaveFileEUtil.DeleteSaveBySlotIndex(slotIndex);
                }
            },
            () => {  }
        );
    });

    private static SystemAction HandleFileSave() => (() =>
    {
        if (!inGame) return;
        if (autoSaveDirector) autoSaveDirector.SaveGameAndFlush();
        Log($"[EmberMode/File/Save] Saved game to current slot.");
    });

    private static Summary GetCurrentSummarySafe()
    {
        if (autoSaveDirector == null) return null;
        try
        {
            var summaries = autoSaveDirector.EnumerateAllSaveGamesIncludingBackups();
            if (summaries == null) return null;
            
            Summary highestSummary = null;
            foreach (var summary in summaries.ToList())
            {
                if (highestSummary == null || summary.SaveNumber > highestSummary.SaveNumber)
                {
                    highestSummary = summary;
                }
            }
            return highestSummary;
        }
        catch { return null; }
    }
    
    private static SystemAction HandleTime(float selection) => (() =>
    {
        if(DebugLogging.HasFlag()) Log($"[EmberMode/Time] {selection}");
        Time.timeScale = selection;
        if (selection == 0.0f)
        {
            NativeEUtil.CustomTimeScale=1f;
        }
        else NativeEUtil.CustomTimeScale=selection;
    });
}