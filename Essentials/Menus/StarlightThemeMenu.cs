using System;
using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Enums.Features;
using Starlight.Enums.Sounds;
using Starlight.Managers;
using Starlight.Storage;
using UnityEngine.UI;

namespace Starlight.Menus;

public class StarlightThemeMenu : StarlightMenu
{
    //Check valid themes for all menus EVERYWHERE
    public new static MenuIdentifier GetMenuIdentifier() => new ("thememenu",StarlightMenuFont.Native,StarlightMenuTheme.Native,"ThemeMenu");
    protected override bool createCommands => true;
    protected override bool inGameOnly => false;
    
    private GameObject _entryTemplate;
    private GameObject _buttonTemplate;
    private GameObject _dropdownTemplate;
    private Transform _content;
    private GameObject _warningText;

    protected override void OnAwake()
    {
        requiredFeatures = new List<FeatureFlag>() { EnableThemeMenu }.ToArray();
        openActions = new List<MenuActions> { MenuActions.PauseGame, MenuActions.HideMenus, MenuActions.OverrideInput }.ToArray();
        closeActions = new List<MenuActions> { MenuActions.UnPauseGame, MenuActions.UnHideMenus, MenuActions.DeOverrideInput, MenuActions.EnableInput }.ToArray();
    }

    protected override void OnClose()
    {
        _content.DestroyAllChildren();
    }
    protected override void OnOpen()
    {
        var identifiers = new List<MenuIdentifier>();
        foreach (var pair in StarlightEntryPoint.Menus)
        {
            var ident = pair.Key.GetMenuIdentifier();
            
            if (!string.IsNullOrEmpty(ident.saveKey)) identifiers.Add(ident);
        }
        foreach (var identifier in identifiers)
        {
            if (!identifier.supportsThemes) continue;
            try
            {
                if(identifier.GetMenu().IsPermanentlyDisabled()) continue;
            }
            catch { }
            var entry = Instantiate(_entryTemplate, _content);
            entry.SetActive(true);
            entry.GetObjectRecursively<TextMeshProUGUI>("Title").text = Tr(identifier.translationKey+".title");

            var contentRec = entry.GetObjectRecursively<Transform>("ContentRec");
            var dropDownObj = Instantiate(_dropdownTemplate, contentRec);
            dropDownObj.SetActive(true);
            var dropdown = dropDownObj.GetObjectRecursively<TMP_Dropdown>("Dropdown");
            dropDownObj.GetObjectRecursively<Canvas>("Canvas").overrideSorting=false;
            dropdown.ClearOptions();
            var options = new Il2CppSystem.Collections.Generic.List<string>();
            var fonts = new List<StarlightMenuFont>();
            var currValue = 0;
            var z = 0;
            foreach(StarlightMenuFont font in Enum.GetValues(typeof(StarlightMenuFont)))
            {
                fonts.Add(font);
                if (StarlightSaveManager.data.fonts[identifier.saveKey] == font) currValue = z;
                options.Add(font.ToString());
                z += 1;
            }
            dropdown.AddOptions(options);
            dropdown.value = currValue;
            dropdown.RefreshShownValue();
            dropdown.onValueChanged.AddListener((System.Action<int>)((value) =>
            {
                AudioEUtil.PlaySound(MenuSound.Click);
                StarlightSaveManager.data.fonts[identifier.saveKey]=fonts[value];
                StarlightSaveManager.Save();
                var menu = identifier.GetMenu();
                if (menu != null)
                    menu.ReloadFont();
            }));
            foreach (var theme in MenuEUtil.GetValidThemes(identifier.saveKey))
            {
                if (theme == StarlightMenuTheme.None) continue;
                var button = Instantiate(_buttonTemplate, contentRec);
                button.SetActive(true);
                button.transform.GetChild(0).GetComponent<Button>().onClick.AddListener((SystemAction)(() =>
                {
                    AudioEUtil.PlaySound(MenuSound.Click);
                    _warningText.SetActive(true);
                    for (int i = 0; i < contentRec.childCount; i++)
                        if(!contentRec.GetChild(i).HasComponent<CanvasGroup>())
                            contentRec.GetChild(i).GetComponent<Image>().color = contentRec.GetChild(i) == button.transform ? Color.green : Color.red;
                    StarlightSaveManager.data.themes[identifier.saveKey] = theme;
                    StarlightSaveManager.Save();
                    try { identifier.GetMenu().OnThemeChange(); } catch (Exception e) { LogError(e); }
                }));
                var texture = new Texture2D(3, 1, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                switch (theme)
                {
                    case StarlightMenuTheme.Starlight: if (true) {
                            if(ColorUtility.TryParseHtmlString("#303846FF", out var pixel0)) texture.SetPixel(0,0,pixel0);
                            if(ColorUtility.TryParseHtmlString("#2C6EC8FF", out var pixel1)) texture.SetPixel(1,0,pixel1);
                            if(ColorUtility.TryParseHtmlString("#1B1B1DFF", out var pixel2)) texture.SetPixel(2,0,pixel2);
                    } break;
                    case StarlightMenuTheme.Black: if (true) {
                            if(ColorUtility.TryParseHtmlString("#000000", out var pixel0)) texture.SetPixel(0,0,pixel0);
                            if(ColorUtility.TryParseHtmlString("#000000", out var pixel1)) texture.SetPixel(1,0,pixel1);
                            if(ColorUtility.TryParseHtmlString("#000000", out var pixel2)) texture.SetPixel(2,0,pixel2);
                    } break;
                    case StarlightMenuTheme.Melon: if (true) {
                        if(ColorUtility.TryParseHtmlString("#303846FF", out var pixel0)) texture.SetPixel(0,0,pixel0);
                        texture.SetPixel(1,0,new Color(0.1098f, 0.5314f, 0.2157f, 1f));
                        if(ColorUtility.TryParseHtmlString("#1B1B1DFF", out var pixel2)) texture.SetPixel(2,0,pixel2);
                    } break;
                    default: if (true) {
                        if(ColorUtility.TryParseHtmlString("#F0E1C8FF", out var pixel0)) texture.SetPixel(0,0,pixel0);
                        if(ColorUtility.TryParseHtmlString("#D2B394FF", out var pixel1)) texture.SetPixel(1,0,pixel1);
                        if(ColorUtility.TryParseHtmlString("#FFFFFFFF", out var pixel2)) texture.SetPixel(2,0,pixel2);
                    } break;
                }

                texture.Apply();
                button.transform.GetChild(0).GetComponent<Image>().sprite = texture.Texture2DToSprite();
                if (StarlightSaveManager.data.themes.TryGetValue(identifier.saveKey, out var dataTheme))
                {
                    if (dataTheme == theme)
                        button.GetComponent<Image>().color = Color.green;
                }
            }
        }
    }
    protected override void OnLateAwake()
    {
        _entryTemplate = transform.GetObjectRecursively<GameObject>("ThemeSelectorEntryRec");
        _buttonTemplate = transform.GetObjectRecursively<GameObject>("ThemeSelectorEntryButtonEntryRec");
        _dropdownTemplate = transform.GetObjectRecursively<GameObject>("ThemeSelectorEntryDropdownEntryRec");
        _warningText = transform.GetObjectRecursively<GameObject>("ThemeMenuRestartWarningRec");
        ToTranslate.Add(_warningText.GetComponent<TextMeshProUGUI>(),"thememenu.warning.restart");
        _content = transform.GetObjectRecursively<Transform>("ThemeMenuThemeSelectorContentRec");
        
        var button1 = transform.GetObjectRecursively<Image>("ThemeMenuThemeSelectorSelectionButtonRec");
        //button1.sprite = whitePillBg;
        
        ToTranslate.Add(button1.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),"thememenu.category.selector");
        ToTranslate.Add(transform.GetObjectRecursively<TextMeshProUGUI>("TitleTextRec"),"thememenu.title");
    }
    public override void OnCloseUIPressed()
    {
        if (MenuEUtil.isAnyPopUpOpen) return;
        
        Close();
    }

}