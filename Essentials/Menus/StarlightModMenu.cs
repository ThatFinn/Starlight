using System;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using Il2CppTMPro;
using MelonLoader;
using Starlight.Components;
using Starlight.Enums;
using Starlight.Enums.Sounds;
using Starlight.Storage.Prefs;
using Starlight.Managers;
using Starlight.Popups;
using Starlight.Storage;
using Starlight.UI;
using Starlight.UI.Blueprints;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Starlight.Menus;

internal class StarlightModMenu : StarlightMenu
{
    public new static MenuIdentifier GetMenuIdentifier() => new ("modmenu",StarlightMenuFont.Native,StarlightMenuTheme.Starlight, "ModMenu",true,true);

    protected override bool createCommands => true;
    protected override bool inGameOnly => false;
    
    //internal static readonly Dictionary<MelonPreferences_Entry, SystemAction> EntriesWithActions = new ();
    private readonly List<Key> _allPossibleUnityKeys = new ();
    private readonly List<KeyCode> _allPossibleUnityKeyCodes = new ();
    private readonly List<LKey> _allPossibleLKey = new ();
    private static RectTransform _openThing;
    private static int _listeningType;
    private static System.Action<int> _listeningAction = null;
    private static bool _warningEnabled = false;
    public new static GameObject GetMenuRootObject()
    {
        var obj = new GameObject("StarlightModMenu");
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = Vector2.zero; 
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        return obj;
    }
    
    protected override void OnAwake()
    {
        GetComponent<RectTransform>().localPosition = Vector2.zero;
        requiredFeatures = [EnableModMenu];
        openActions = [MenuActions.PauseGameFalse, MenuActions.HideMenus, MenuActions.OverrideInput];
        closeActions = [MenuActions.UnPauseGameFalse, MenuActions.UnHideMenus, MenuActions.DeOverrideInput, MenuActions.EnableInput];
    }

    protected override void OnLateAwake()
    {
        foreach (string stringKey in Enum.GetNames(typeof(Key)))
            if (!string.IsNullOrEmpty(stringKey))
                if (Enum.TryParse(stringKey, out Key key)) _allPossibleUnityKeys.Add(key);
        _allPossibleUnityKeys.Remove(Key.LeftCommand);
        _allPossibleUnityKeys.Remove(Key.RightCommand);
        _allPossibleUnityKeys.Remove(Key.None);
        
        foreach (string stringKey in Enum.GetNames(typeof(KeyCode)))
            if (!string.IsNullOrEmpty(stringKey))
                if (Enum.TryParse(stringKey, out KeyCode key)) _allPossibleUnityKeyCodes.Add(key);
        _allPossibleUnityKeyCodes.Remove(KeyCode.LeftWindows);
        _allPossibleUnityKeyCodes.Remove(KeyCode.RightWindows);
        _allPossibleUnityKeyCodes.Remove(KeyCode.None);
        
        foreach (string stringKey in Enum.GetNames(typeof(LKey)))
            if (!string.IsNullOrEmpty(stringKey))
               if (Enum.TryParse(stringKey, out LKey key)) _allPossibleLKey.Add(key);
        _allPossibleLKey.Remove(LKey.None);
    }

    [HideFromIl2Cpp] private PanelUIBlueprintV01 blueprint => new ()
    {
        Color = UIColor.Primary, CornerRadius = 90, Size = new Vector2(1330, 840),
        Children =
        [
            new PanelUIBlueprintV01 { Color = UIColor.Accent, CornerRadius = 0, Size = new Vector2(1330,10), Position = new Vector2(0,-265f) },
            new PanelUIBlueprintV01 { Color = UIColor.Accent, CornerRadius = 0, Size = new Vector2(1330,10), Position = new Vector2(0,265f) },
            new NavigationUIBlueprintV01
            {
                CornerRadius = 90, Size = new Vector2(1330, 840),
                TabSize = new Vector2(1330,520),
                Tabs = [
                    new PanelUIBlueprintV01
                    {
                        mame = "modmenu.category.modmenu",
                        Children = [
                            new VScrollUIBlueprintV01
                            {
                                Size = new Vector2(660,520), Position = new Vector2(-332.5f,0), mame="ModMenuModsScrollRec",
                                Children = GetAllModButtons()
                            },
                            new PanelUIBlueprintV01 { Color = UIColor.Accent, Size = new Vector2(10,520) },
                            new TextUIBlueprintV01
                            {
                                Size = new Vector2(660,520), CornerRadius = 10, Position = new Vector2(332.5f,0), mame="ModMenuInfoTextRec",
                                TextContent = "nada", FontSize = 17, ClickableLinks = true,
                                Margins= new Vector4(10,10,10,10),
                            },
                            new ButtonUIBlueprintV01()
                            {
                                OnClick = (() => { Close(); MenuEUtil.GetMenu<StarlightThemeMenu>().OpenC(this); }), mame="ModMenuThemeMenuButtonRec",
                                Size = new (420, 45), Position = new (453, -237), CornerRadius = 20,
                                Children =
                                [
                                    new TextUIBlueprintV01()
                                    {
                                        Color = UIColor.TextButton,
                                        TextContent = "modmenu.openthememenu",
                                        Alignment = TextAlignmentOptions.Center,
                                        FontSize = 30,
                                        Anchors = new Vector4(0,0,1,1),
                                    }
                                ]
                            } 
                        ]
                    },
                    new PanelUIBlueprintV01
                    {
                        mame = "modmenu.category.modconfig",
                        Children = [
                            new VScrollUIBlueprintV01
                            {
                                Size = new Vector2(1330,520), Position = new Vector2(0f,0), mame="ModMenuModConfigScrollRec",
                                Children = GetAllModConfigs()
                            },
                            new TextUIBlueprintV01
                            {
                                Size = new Vector2(1330,50), Position  =new (0,-320), mame ="ModMenuModConfigWarningRec",
                                TextContent="modmenu.warning.restart", Color = UIColor.TextWarning, FontSize = 30,
                                Alignment = TextAlignmentOptions.Center
                            }
                        ]
                    }
                ],
                ChildrenWithButtons = [
                    new ButtonUIBlueprintV01()
                    {
                        OnClick = () =>
                            {
                                if (EnableRepoMenu.HasFlag())
                                {
                                    Close(); 
                                    MenuEUtil.GetMenu<StarlightRepoMenu>().OpenC(this);
                                }
                                else
                                {
                                    AudioEUtil.PlaySound(MenuSound.Error);
                                    StarlightTextViewerPopUp.Open(Tr("feature.indevelopment"));
                                }
                            },
                        CornerRadius = 40,
                        Children =
                        [
                            new TextUIBlueprintV01()
                            {
                                Color = UIColor.TextButton,
                                TextContent = "modmenu.category.repo",
                                Alignment = TextAlignmentOptions.Center,
                                FontSize = 40,
                                Anchors = new Vector4(0,0,1,1),
                                FontStyle = FontStyles.Bold,
                            }
                        ]
                    }
                ]
            },
            new PanelUIBlueprintV01
            {
                Color = UIColor.Badge, CornerRadius = 50, Size = new Vector2(530,90), Position = new Vector2(0,-420f),
                Children = [
                    new TextUIBlueprintV01
                    {
                        TextContent = "modmenu.title", Size = new Vector2(530,90), FontSize = 50, 
                        Color = UIColor.TextButton, Alignment = TextAlignmentOptions.Center,
                    }
                ]
                
            },
        ]
    };

    [HideFromIl2Cpp] private static List<UIBlueprint> GetAllModConfigs()
    {
        var entries = new Dictionary<string,List<UIBlueprint>>();
        
        foreach (var category in MelonPreferences.Categories)
        {
            if (category == null) continue;
            if (category.IsHidden) continue;
            var list = new List<UIBlueprint>();
            var displayName = category.DisplayName;
            try
            {
                var value = StarlightPackageManager.GetPackageInfoFromID(displayName.Replace("_","."))?.Name;
                if (value != null)
                    displayName = value;
            } catch {}
            list.Add(new PanelUIBlueprintV01()
            {
                Size = new Vector2(800,60), Color = UIColor.Transparent,
                Children = [
                    new TextUIBlueprintV01()
                    {
                        TextContent = displayName, Margins = new Vector4(5,1,5,1), 
                        Color = UIColor.TextCategoryAlternate, FontSize = 40, FontAutoSizeMax = 40, EnableAutoSizing = true,
                        Size = new Vector2(0,0), Anchors = new Vector4(0,0,1,1), Alignment = TextAlignmentOptions.Left,
                    }
                ]
            });
            foreach (var entry in category.Entries)
            {
                if (entry == null) continue;
                if (entry.IsHidden) continue;
                var final = "<NoName>";
                try
                {
                    if (!string.IsNullOrWhiteSpace(entry.DisplayName))
                        final = entry.DisplayName;
                } catch { }
                try
                {
                    if (!string.IsNullOrWhiteSpace(entry.Description))
                        final += $"\n<size=75%>{entry.Description}</size>";
                } catch { }
                var blueprint = new PanelUIBlueprintV01()
                {
                    Size = new Vector2(800, 50), Color = UIColor.Transparent,
                    Components = [typeof(ModConfigSizingFixer).IL2CPPTypeof()],
                    Children =
                    [
                        new TextUIBlueprintV01()
                        {
                            mame = "NameAndDescription",
                            TextContent = final, Margins = new Vector4(5, 1, 500, 1), FontSize = 24, Size = new Vector2(0, 0),
                            Anchors = new Vector4(0, 0, 1, 1), Alignment = TextAlignmentOptions.Left
                        }
                    ]
                };
                if (entry.BoxedEditedValue is int) ApplyIntFeatures(entry,blueprint,category);
                else if (entry.BoxedEditedValue is float) ApplyFloatFeatures(entry,blueprint,category);
                else if (entry.BoxedEditedValue is double) ApplyDoubleFeatures(entry,blueprint,category);
                else if (entry.BoxedEditedValue is long) ApplyLongFeatures(entry,blueprint,category);
                else if (entry.BoxedEditedValue is string) ApplyStringFeatures(entry,blueprint,category);
                else if (entry.BoxedEditedValue is bool) ApplyBoolFeatures(entry,blueprint,category);
                else if (entry.BoxedEditedValue is LKey) ApplyLKeyFeatures(entry,blueprint,category);
                else if (entry.BoxedEditedValue is KeyCode) ApplyKeyCodeFeatures(entry,blueprint,category);
                else if (entry.BoxedEditedValue is Key) ApplyKeyFeatures(entry,blueprint,category);
                else if (entry.BoxedEditedValue is Enum) ApplyEnumFeatures(entry,blueprint,category);
                else ApplyUnknownFeatures(entry,blueprint,category);
                
                list.Add(blueprint);
                
            }
            entries.Add(displayName,list);
        } 
        
        foreach (var prefs in PackagePrefs.allPrefs)
        {
            if (prefs == null) continue;
            if (prefs.IsHidden()) continue;
            var list = new List<UIBlueprint>();
            var displayName = prefs.expansionID;
            try
            {
                var value = StarlightPackageManager.GetPackageInfoFromID(prefs.expansionID)?.Name;
                if (value != null)
                    displayName = value;
            } catch {}
            list.Add(new PanelUIBlueprintV01()
            {
                Size = new Vector2(800,60), Color = UIColor.Transparent,
                Children = [
                    new TextUIBlueprintV01()
                    {
                        TextContent = displayName, Margins = new Vector4(5,1,5,1), 
                        Color = UIColor.TextCategory, FontSize = 40, FontAutoSizeMax = 40, EnableAutoSizing = true,
                        Size = new Vector2(0,0), Anchors = new Vector4(0,0,1,1), Alignment = TextAlignmentOptions.Left,
                    }
                ]
            });
            foreach (var entry in prefs.entries)
            {
                if (entry == null) continue;
                if (entry.isHidden) continue;
                var final = "<NoName>";
                try
                {
                    if (!string.IsNullOrWhiteSpace(entry.displayName))
                        final = entry.displayName;
                } catch { }
                try
                {
                    if (!string.IsNullOrWhiteSpace(entry.description))
                        final += $"\n<size=75%>{entry.description}</size>";
                } catch { }
                var blueprint = new PanelUIBlueprintV01()
                {
                    Size = new Vector2(800, 50), Color = UIColor.Transparent,
                    Components = [typeof(ModConfigSizingFixer).IL2CPPTypeof()],
                    Children =
                    [
                        new TextUIBlueprintV01()
                        {
                            mame = "NameAndDescription",
                            TextContent = final, Margins = new Vector4(5, 1, 500, 1), FontSize = 24, Size = new Vector2(0, 0),
                            Anchors = new Vector4(0, 0, 1, 1), Alignment = TextAlignmentOptions.Left
                        }
                    ]
                };
                if (entry.dynamicValue is int) ApplyIntFeatures(entry,blueprint,prefs);
                else if (entry.dynamicValue is float) ApplyFloatFeatures(entry,blueprint,prefs);
                else if (entry.dynamicValue is double) ApplyDoubleFeatures(entry,blueprint,prefs);
                else if (entry.dynamicValue is long) ApplyLongFeatures(entry,blueprint,prefs);
                else if (entry.dynamicValue is string) ApplyStringFeatures(entry,blueprint,prefs);
                else if (entry.dynamicValue is bool) ApplyBoolFeatures(entry,blueprint,prefs);
                else if (entry.dynamicValue is LKey) ApplyLKeyFeatures(entry,blueprint,prefs);
                else if (entry.dynamicValue is KeyCode) ApplyKeyCodeFeatures(entry,blueprint,prefs);
                else if (entry.dynamicValue is Key) ApplyKeyFeatures(entry,blueprint,prefs);
                else if (entry.dynamicValue is Enum) ApplyEnumFeatures(entry,blueprint,prefs);
                else ApplyUnknownFeatures(entry,blueprint,prefs);
                
                list.Add(blueprint);
                
            }
            entries.Add(displayName,list);
        } 
        
        
        var sorted = entries.OrderByDescending(x => x.Key == BuildInfo.Name).
            ThenBy(x => x.Key).Select(x => x.Value).ToList();
        var lists = sorted.ToNetList();
        var finalList = new List<UIBlueprint>();
        foreach (var list in lists) finalList.AddRange(list);
        return finalList;
    }
    [HideFromIl2Cpp] private static List<UIBlueprint> GetAllModButtons()
    {
        var entries = new Dictionary<string,UIBlueprint>();
        // Melons
        foreach (var info in StarlightPackageManager.GetAllMelonInfos())
            try { entries.Add(info.Name,ProcessPackage(info,null,false, null)); }
            catch (Exception e) { LogError(e); }
        // Expansions
        foreach (var info in StarlightPackageManager.GetAllExpansionInfos())
            try { entries.Add(info.Name,ProcessPackage(info,null,false, null)); }
            catch (Exception e) { LogError(e); }
        // Rotten
        foreach (var pair in StarlightPackageManager.GetAllRottenInfos())
            try { entries.Add(pair.Key.Name,ProcessPackage(pair.Key,null,true, pair.Value)); }
            catch (Exception e) { LogError(e); }
        var sorted = entries.OrderByDescending(x => x.Key == BuildInfo.Name).
            ThenBy(x => x.Key).Select(x => x.Value).ToList();
        return sorted.ToNetList();
    }
    [HideFromIl2Cpp] static UIBlueprint ProcessPackage(StarlightPackageInfo info,string downloadLink,bool isRotten, List<object> rottenInfo)
    {
        var button = new ButtonUIBlueprintV01
        {
            Size = new Vector2(595, 60),
            CornerRadius = 30,
            ButtonColors = UIColorBlock.AlternativeButtons,
            Children = [
                new TextUIBlueprintV01
                {
                    TextContent = info.Name, Alignment = TextAlignmentOptions.Center, FontSize = 26, FontAutoSizeMax = 26, EnableAutoSizing = true,
                    Margins = new Vector4(110,5,110,5),
                    Color = UIColor.TextButton, Anchors = new Vector4( 0,0,1,1)
                }
            ]
        };
        var isSelf = info is { Name: BuildInfo.Name, Author: BuildInfo.Author, Version: BuildInfo.DisplayVersion };
        if (info.Type == PackageType.Expansion||isSelf) button.ButtonColors = UIColorBlock.Buttons;
        if (isRotten) button.ButtonColors = UIColorBlock.GrayButtons;
        if (!isSelf&&(info.Type == PackageType.MelonMod || info.Type == PackageType.MelonPlugin))
        {
            button.Children.Add(new PanelUIBlueprintV01()
            {
                Size=new Vector2(50,50), Position = new Vector2(266,0),
                Color=UIColor.None,Sprite = ConvertEUtil.BytesToTexture2D(EncodedAssets.MelonIcon.RawAssetBytes).CopyWithoutMipmaps().Texture2DToSprite(),
                CornerRadius = 10
                //anchors = new Vector4(0,0.5f,0,0.5f),
            });
        }
        
        
        if (info.Icon)
        {
            button.Children.Add(new PanelUIBlueprintV01()
            {
                Size=new Vector2(50,50), Position = new Vector2(-266,0),
                Color=UIColor.None,Sprite = info.Icon,
                CornerRadius = 10
                //anchors = new Vector4(0,0.5f,0,0.5f),
            });
        }

        button.OnClick = () =>
        {
            AudioEUtil.PlaySound(MenuSound.Click);
            _openThing.GetObjectRecursively<GameObject>("ModMenuThemeMenuButtonRec").SetActive(isSelf);
            var finalText = "";
            if (isRotten)
            {
                finalText = Tr("modmenu.modinfo.brokenmod", info.Name);
                if (info.Type==PackageType.Expansion) finalText = Tr("modmenu.modinfo.brokenexpansion", info.Name);
            }
            else
            {
                finalText = Tr("modmenu.modinfo.mod", info.Name);
                if (info.Type==PackageType.Expansion) finalText = Tr("modmenu.modinfo.expansion", info.Name);
            }
            if(!string.IsNullOrWhiteSpace(info.ID)) finalText += "\n" + Tr("modmenu.modinfo.id", info.ID);
            finalText += "\n" + Tr("modmenu.modinfo.author", string.IsNullOrEmpty(info.Author)?"Anonymous":info.Author);

            if(info.Type==PackageType.Expansion)
                finalText += "\n" + Tr("modmenu.modinfo.useprism", info.UsePrism);

            if (info.CoAuthors is { Length: > 0 }) finalText += "\n" + Tr("modmenu.modinfo.coauthor", string.Join(", ",info.CoAuthors));
            if (info.Contributors is { Length: > 0 }) finalText += "\n" + Tr("modmenu.modinfo.contributors", string.Join(", ",info.Contributors));
            
            finalText += "\n" + Tr("modmenu.modinfo.version", info.Version) + "\n";
            
            
            if(!string.IsNullOrWhiteSpace(info.SourceCode)) finalText += "\n" + Tr("modmenu.modinfo.sourcecode", FormatLink(info.SourceCode));
            if(!string.IsNullOrWhiteSpace(info.Nexus)) finalText += "\n" + Tr("modmenu.modinfo.nexus", FormatLink(info.Nexus));
            if(!string.IsNullOrWhiteSpace(info.Discord)) finalText += "\n" + Tr("modmenu.modinfo.discord", FormatLink(info.Discord));
            if(!string.IsNullOrWhiteSpace(downloadLink)) finalText += "\n" + Tr("modmenu.modinfo.link", FormatLink(downloadLink));

            if (!isRotten)
            {
                
                finalText += "\n";
                if (info.Dependencies is { Length: > 0 })
                {
                    var translatedDependencies = info.Dependencies.ToList();
                    for (var i = 0; i < translatedDependencies.Count; i++)
                    {
                        var depInfo = StarlightPackageManager.GetPackageInfoFromID(translatedDependencies[i]);
                        if (depInfo is null) continue;
                        var newName = depInfo.Value.Name;
                        translatedDependencies[i] = newName.Contains(' ') ?"\""+newName+"\"":newName;
                    }
                    finalText += "\n" + Tr("modmenu.modinfo.dependencies", string.Join(", ", translatedDependencies));
                }
                finalText += "\n" + Tr("modmenu.modinfo.loadtime", info.LoadTime);
                finalText += "\n" + Tr("modmenu.modinfo.unloadtime", info.UnloadTime);
                finalText += "\n" + Tr("modmenu.modinfo.mprequirement", info.MultiplayerRequirement);
                finalText += "\n";
            }
            
            if(!string.IsNullOrWhiteSpace(info.Description)) 
                finalText += "\n" + Tr("modmenu.modinfo.description", info.Description + "\n");

            if (isRotten&&rottenInfo!=null&rottenInfo.Count>=3)
            {
                finalText += "\n";
                try {finalText += "\n" + Tr("modmenu.modinfo.path", rottenInfo[0]);} catch {}
                try {finalText += "\n" + Tr("modmenu.modinfo.exception", rottenInfo[1]);} catch {}
                try {finalText += "\n" + Tr("modmenu.modinfo.errormessage", rottenInfo[2]);} catch {}
            }
            _openThing.GetObjectRecursively<TextMeshProUGUI>("ModMenuInfoTextRec").text = finalText;
        };
        return button;
    }
    static string FormatLink(string url)
    {
        return $"<link=\"{url}\"><color=#2C6EC8><u>{url}</u></color></link>";
    }
    
    protected override void OnOpen()
    {
        _openThing = blueprint.Render(currentTheme, currentFontTheme, transform);
        try
        {
            _openThing.GetObjectRecursively<ScrollRect>("ModMenuModsScrollRec").content.transform.GetChild(0).gameObject.GetComponent<Button>().Press();
        } catch { }
        if(!_warningEnabled) _openThing.GetObjectRecursively<GameObject>("ModMenuModConfigWarningRec").SetActive(false);
    }

    protected override void OnClose()
    {
        Destroy(_openThing.gameObject);
    }
    
    public override void OnCloseUIPressed()
    {
        if (_listeningAction != null) return;
        if (MenuEUtil.isAnyPopUpOpen) return;
        Close();
    }
    
    protected override void OnUpdate()
    {
        if(_listeningAction !=null) switch (_listeningType)
        {
            case 1: foreach (var key in _allPossibleLKey) try { if(key.OnKeyDown()) { _listeningAction.Invoke(Convert.ToInt32(key)); } } catch { } break;
            case 2: foreach (var key in _allPossibleUnityKeyCodes) try { if(key.OnKeyDown()) _listeningAction.Invoke(Convert.ToInt32(key)); } catch { } break;
            case 3: foreach (var key in _allPossibleUnityKeys) try { if(Keyboard.current[key].wasPressedThisFrame) _listeningAction.Invoke(Convert.ToInt32(key)); }catch { } break;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame ||
            Mouse.current.middleButton.wasPressedThisFrame ||
            Mouse.current.backButton.wasPressedThisFrame ||
            Mouse.current.forwardButton.wasPressedThisFrame)
        {
            if (_listeningAction != null)
                _listeningAction.Invoke(0);
        }
    }

    static void ShowWarningText()
    {
        _warningEnabled = true;
        try
        {
            
            _openThing.GetObjectRecursively<GameObject>("ModMenuModConfigWarningRec").SetActive(true);
        }
        catch {}
    }

    
    
    
    
    private static void ApplyIntFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.stringifiedValue, RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.IntegerNumber, PlaceHolderContent = "modmenu.modconfig.enterint", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                if (string.IsNullOrEmpty(text)) text = "0";
                if (int.TryParse(text, out var value))
                {
                    entry.SetPref(value);
                    category.Save();
                    if(entry.showWarningOnEdit) ShowWarningText();
                }
            })
        });
    }
    private static void ApplyFloatFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.stringifiedValue, RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.DecimalNumber, PlaceHolderContent = "modmenu.modconfig.enterfloat", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                if (string.IsNullOrEmpty(text)) text = "0.0";
                if (float.TryParse(text, out var value))
                {
                    entry.SetPref(value);
                    category.Save();
                    if(entry.showWarningOnEdit) ShowWarningText();
                }
            })
        });
    }
    private static void ApplyDoubleFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.stringifiedValue, RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.DecimalNumber, PlaceHolderContent = "modmenu.modconfig.enterdouble", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                if (string.IsNullOrEmpty(text)) text = "0.0";
                if (double.TryParse(text, out var value))
                {
                    entry.SetPref(value);
                    category.Save();
                    if(entry.showWarningOnEdit) ShowWarningText();
                }
            })
        });
    }
    private static void ApplyLongFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.stringifiedValue, RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.IntegerNumber, PlaceHolderContent = "modmenu.modconfig.enterlong", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                if (string.IsNullOrEmpty(text)) text = "0";
                if (long.TryParse(text, out var value))
                {
                    entry.SetPref(value);
                    category.Save();
                    if(entry.showWarningOnEdit) ShowWarningText();
                }
            })
        });
    }
    private static void ApplyStringFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.stringifiedValue, RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.Standard, PlaceHolderContent = "modmenu.modconfig.enterstring", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                entry.SetPref(text);
                category.Save();
                if(entry.showWarningOnEdit) ShowWarningText();
            })
        });
    }
    private static void ApplyBoolFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new CheckboxUIBlueprintV01()
        {
            Size = new (45, 45), Position = new (625, 0), DefaultValue = entry.stringifiedValue.ToLower() == "true", CornerRadius = 10,
            OnValueChanged = (isOn =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                entry.SetPref(isOn);
                category.Save();
                if(entry.showWarningOnEdit) ShowWarningText();
            })
        });
    }
    private static void ApplyLKeyFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new ButtonUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), CornerRadius = 10,
            Children =
            [
                new TextUIBlueprintV01()
                {
                    TextContent = entry.stringifiedValue,
                    DisableAutoTranslation = true,
                    Alignment = TextAlignmentOptions.Center,
                    FontSize = 30,
                    Anchors = new Vector4(0,0,1,1),
                }
            ],
            OnClickButton = (button =>
            {
                var textMesh = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                textMesh.text = Tr("modmenu.modconfig.keylistening");
                _listeningType = 1;
                _listeningAction = (integer) =>
                {
                    var inputKey = (LKey) integer;
                    var key = inputKey == LKey.Escape ? LKey.None : inputKey;
                    textMesh.text = key.ToString();
                    entry.SetPref(key);
                    category.Save();
                    if(entry.showWarningOnEdit) ShowWarningText();
                    _listeningAction = null;
                };
            })
        });
    }
    private static void ApplyKeyCodeFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new ButtonUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), CornerRadius = 10,
            Children =
            [
                new TextUIBlueprintV01()
                {
                    TextContent = entry.stringifiedValue,
                    DisableAutoTranslation = true,
                    Alignment = TextAlignmentOptions.Center,
                    FontSize = 30,
                    Anchors = new Vector4(0,0,1,1),
                }
            ],
            OnClickButton = (button =>
            {
                var textMesh = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                textMesh.text = Tr("modmenu.modconfig.keylistening");
                _listeningType = 2;
                _listeningAction = (integer) =>
                {
                    var inputKey = (KeyCode) integer;
                    var key = inputKey == KeyCode.Escape ? KeyCode.None : inputKey;
                    textMesh.text = key.ToString();
                    entry.SetPref(key);
                    category.Save();
                    if(entry.showWarningOnEdit) ShowWarningText();
                    _listeningAction = null;
                };
            })
        });
    }
    private static void ApplyKeyFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new ButtonUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), CornerRadius = 10,
            Children =
            [
                new TextUIBlueprintV01()
                {
                    TextContent = entry.stringifiedValue,
                    DisableAutoTranslation = true,
                    Alignment = TextAlignmentOptions.Center,
                    FontSize = 30,
                    Anchors = new Vector4(0,0,1,1),
                }
            ],
            OnClickButton = (button =>
            {
                var textMesh = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                textMesh.text = Tr("modmenu.modconfig.keylistening");
                _listeningType = 3;
                _listeningAction = (integer) =>
                {
                    var inputKey = (Key) integer;
                    var key = inputKey == Key.Escape ? Key.None : inputKey;
                    textMesh.text = key.ToString();
                    entry.SetPref(key);
                    category.Save();
                    if(entry.showWarningOnEdit) ShowWarningText();
                    _listeningAction = null;
                };
            })
        });
    }
    private static void ApplyEnumFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new ButtonUIBlueprintV01()
        {
            Size = new(480, 45), Position = new(410, 0), CornerRadius = 10,
            ButtonColors = UIColorBlock.AlternativeButtons,
            Children =
            [
                new TextUIBlueprintV01()
                {
                    TextContent = entry.stringifiedValue,
                    DisableAutoTranslation = true,
                    Alignment = TextAlignmentOptions.Center,
                    FontSize = 30,
                    Anchors = new Vector4(0, 0, 1, 1),
                }
            ],
            OnClickButton = (button =>
            {
                try
                {
                    AudioEUtil.PlaySound(MenuSound.Click);
                    var textMesh = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    var enumType = entry.dynamicValue.GetType();
                    var values = Enum.GetValues(enumType);
                    var currentIndex = Array.IndexOf(values, entry.dynamicValue);
                    var nextValue = values.GetValue((currentIndex + 1) % values.Length);
                    textMesh.text = nextValue.ToString();
                    entry.SetPref(nextValue);
                    category.Save();
                    if (entry.showWarningOnEdit) ShowWarningText();
                    UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
                }
                catch (Exception e) { LogError(e); }
            })
        });
    }
    private static void ApplyUnknownFeatures(PackagePref entry, UIBlueprint blueprint, PackagePrefs category)
    {
        blueprint.Children.Add(new TextUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), TextContent = entry.stringifiedValue,
            Alignment = TextAlignmentOptions.Center
        });
    }
    
    
    
    
    
    
    
    // Melon Features
    private static void ApplyIntFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.GetEditedValueAsString(), RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.IntegerNumber, PlaceHolderContent = "modmenu.modconfig.enterint", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                if (string.IsNullOrEmpty(text)) text = "0";
                if (int.TryParse(text, out var value))
                {
                    entry.BoxedEditedValue = value;
                    category.SaveToFile(false);
                }
            })
        });
    }
    private static void ApplyFloatFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.GetEditedValueAsString(), RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.DecimalNumber, PlaceHolderContent = "modmenu.modconfig.enterfloat", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                if (string.IsNullOrEmpty(text)) text = "0.0";
                if (float.TryParse(text, out var value))
                {
                    entry.BoxedEditedValue = value;
                    category.SaveToFile(false);
                }
            })
        });
    }
    private static void ApplyDoubleFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.GetEditedValueAsString(), RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.DecimalNumber, PlaceHolderContent = "modmenu.modconfig.enterdouble", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                if (string.IsNullOrEmpty(text)) text = "0.0";
                if (double.TryParse(text, out var value))
                {
                    entry.BoxedEditedValue = value;
                    category.SaveToFile(false);
                }
            })
        });
    }
    private static void ApplyLongFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.GetEditedValueAsString(), RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.IntegerNumber, PlaceHolderContent = "modmenu.modconfig.enterlong", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                if (string.IsNullOrEmpty(text)) text = "0";
                if (long.TryParse(text, out var value))
                {
                    entry.BoxedEditedValue = value;
                    category.SaveToFile(false);
                }
            })
        });
    }
    private static void ApplyStringFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new InputUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), DefaultValue = entry.GetEditedValueAsString(), RestoreOriginalTextOnEscape = false,
            ContentType = TMP_InputField.ContentType.Standard, PlaceHolderContent = "modmenu.modconfig.enterstring", CornerRadius = 10,
            OnValueChanged = (text =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                entry.BoxedEditedValue = text;
                category.SaveToFile(false);
            })
        });
    }
    private static void ApplyBoolFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new CheckboxUIBlueprintV01()
        {
            Size = new (45, 45), Position = new (625, 0), DefaultValue = entry.BoxedEditedValue.ToString().ToLower() == "true", CornerRadius = 10,
            CheckColor = UIColor.AccentAlternate,
            OnValueChanged = (isOn =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                entry.BoxedEditedValue = isOn;
                category.SaveToFile(false);
            })
        });
    }
    private static void ApplyLKeyFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new ButtonUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), CornerRadius = 10,
            ButtonColors = UIColorBlock.AlternativeButtons,
            Children =
            [
                new TextUIBlueprintV01()
                {
                    TextContent = entry.GetEditedValueAsString(),
                    DisableAutoTranslation = true,
                    Alignment = TextAlignmentOptions.Center,
                    FontSize = 30,
                    Anchors = new Vector4(0,0,1,1),
                }
            ],
            OnClickButton = (button =>
            {
                var textMesh = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                textMesh.text = Tr("modmenu.modconfig.keylistening");
                _listeningType = 1;
                _listeningAction = (integer) =>
                {
                    var inputKey = (LKey) integer;
                    var key = inputKey == LKey.Escape ? LKey.None : inputKey;
                    if (entry.BoxedEditedValue is LKey)
                    {
                        textMesh.text = key.ToString();
                        entry.BoxedEditedValue = key;
                        category.SaveToFile(false);
                    }

                    _listeningAction = null;
                };
            })
        });
    }
    private static void ApplyKeyCodeFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new ButtonUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), CornerRadius = 10,
            ButtonColors = UIColorBlock.AlternativeButtons,
            Children =
            [
                new TextUIBlueprintV01()
                {
                    TextContent = entry.GetEditedValueAsString(),
                    DisableAutoTranslation = true,
                    Alignment = TextAlignmentOptions.Center,
                    FontSize = 30,
                    Anchors = new Vector4(0,0,1,1),
                }
            ],
            OnClickButton = (button =>
            {
                var textMesh = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                textMesh.text = Tr("modmenu.modconfig.keylistening");
                _listeningType = 2;
                _listeningAction = (integer) =>
                {
                    var inputKey = (KeyCode) integer;
                    var key = inputKey == KeyCode.Escape ? KeyCode.None : inputKey;
                    if (entry.BoxedEditedValue is KeyCode)
                    {
                        textMesh.text = key.ToString();
                        entry.BoxedEditedValue = key;
                        category.SaveToFile(false);
                    }

                    _listeningAction = null;
                };
            })
        });
    }
    private static void ApplyKeyFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new ButtonUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), CornerRadius = 10,
            ButtonColors = UIColorBlock.AlternativeButtons,
            Children =
            [
                new TextUIBlueprintV01()
                {
                    TextContent = entry.GetEditedValueAsString(),
                    DisableAutoTranslation = true,
                    Alignment = TextAlignmentOptions.Center,
                    FontSize = 30,
                    Anchors = new Vector4(0,0,1,1),
                }
            ],
            OnClickButton = (button =>
            {
                var textMesh = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                textMesh.text = Tr("modmenu.modconfig.keylistening");
                _listeningType = 3;
                _listeningAction = (integer) =>
                {
                    var inputKey = (Key) integer;
                    var key = inputKey == Key.Escape ? Key.None : inputKey;
                    if (entry.BoxedEditedValue is Key)
                    {
                        textMesh.text = key.ToString();
                        entry.BoxedEditedValue = key;
                        category.SaveToFile(false);
                    }

                    _listeningAction = null;
                };
            })
        });
    }
    private static void ApplyEnumFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new ButtonUIBlueprintV01()
        {
            Size = new(480, 45), Position = new(410, 0), CornerRadius = 10,
            ButtonColors = UIColorBlock.AlternativeButtons,
            Children =
            [
                new TextUIBlueprintV01()
                {
                    TextContent = entry.GetEditedValueAsString(),
                    DisableAutoTranslation = true,
                    Alignment = TextAlignmentOptions.Center,
                    FontSize = 30,
                    Anchors = new Vector4(0, 0, 1, 1),
                }
            ],
            OnClickButton = (button =>
            {
                try
                {
                    AudioEUtil.PlaySound(MenuSound.Click);
                    var textMesh = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    var enumType = entry.BoxedEditedValue.GetType();
                    var values = Enum.GetValues(enumType);
                    var currentIndex = Array.IndexOf(values, entry.BoxedEditedValue);
                    var nextValue = values.GetValue((currentIndex + 1) % values.Length);
                    textMesh.text = nextValue.ToString();
                    entry.BoxedEditedValue = nextValue;
                    category.SaveToFile(false);
                    UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
                }
                catch (Exception e) { LogError(e); }
            })
        });
    }
    private static void ApplyUnknownFeatures(MelonPreferences_Entry entry, UIBlueprint blueprint, MelonPreferences_Category category)
    {
        blueprint.Children.Add(new TextUIBlueprintV01()
        {
            Size = new (480, 45), Position = new (410, 0), TextContent = entry.GetEditedValueAsString(),
            Alignment = TextAlignmentOptions.Center
        });
    }
    
}