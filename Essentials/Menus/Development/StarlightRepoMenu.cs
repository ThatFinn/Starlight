/*using System;
using Il2CppTMPro;
using Starlight.Enums;
using Starlight.Enums.Features;
using Starlight.Managers;
using Starlight.Popups;
using Starlight.Storage;
using UnityEngine.UI;

namespace Starlight.Menus.Development;

internal class StarlightRepoMenu : StarlightMenu
{
    //Check valid themes for all menus EVERYWHERE
    public new static MenuIdentifier GetMenuIdentifier() => new ("repomenu",StarlightMenuFont.Native,StarlightMenuTheme.Starlight,"RepoMenu");
    protected override bool createCommands => true;
    protected override bool inGameOnly => false;
    
    private Transform _repoPanel;
    private Transform _modPanel;
    protected override void OnAwake()
    {
        requiredFeatures = new List<FeatureFlag>() { EnableRepoMenu }.ToArray();
        openActions = new List<MenuActions> { MenuActions.PauseGame, MenuActions.HideMenus, MenuActions.OverrideInput }.ToArray();
        closeActions = new List<MenuActions> { MenuActions.UnPauseGame, MenuActions.UnHideMenus, MenuActions.DeOverrideInput, MenuActions.EnableInput }.ToArray();
    }


    protected override void OnClose()
    {
        gameObject.GetObjectRecursively<Button>("RepoMenuBrowseSelectionButtonRec").onClick.Invoke();
        transform.GetObjectRecursively<Transform>("RepoMenuBrowseContentRec").DestroyAllChildren();
        transform.GetObjectRecursively<Transform>("RepoMenuRepoContentRec").DestroyAllChildren();
    }
    
    protected override void OnOpen()
    {
        gameObject.GetObjectRecursively<Button>("RepoMenuBrowseSelectionButtonRec").onClick.Invoke();
        
    }
    
    protected override void OnLateAwake()
    {
        
        var button1 = transform.GetObjectRecursively<Image>("RepoMenuBrowseSelectionButtonRec");
        //button1.sprite = whitePillBg;
        button1.GetComponent<Button>().onClick.AddListener(selectCategorySound);
        button1.GetComponent<Button>().onClick.AddListener((SystemAction)OnBrowseTab);
        var button2 = transform.GetObjectRecursively<Image>("RepoMenuSourcesSelectionButtonRec");
        //button2.sprite = whitePillBg;
        button2.GetComponent<Button>().onClick.AddListener(selectCategorySound);
        button2.GetComponent<Button>().onClick.AddListener((SystemAction)OnRepoTab);
        var button3 = transform.GetObjectRecursively<Image>("RepoMenuInstalledSelectionButtonRec");
        //button3.sprite = whitePillBg;
        button3.GetComponent<Button>().onClick.AddListener(selectCategorySound);
        var button4 = transform.GetObjectRecursively<Image>("RepoMenuSettingsSelectionButtonRec");
        //button4.sprite = whitePillBg;
        button4.GetComponent<Button>().onClick.AddListener(selectCategorySound);
        _repoPanel = transform.GetObjectRecursively<Transform>("RepoViewPanelRec");
        _modPanel = transform.GetObjectRecursively<Transform>("ModViewPanelRec");
        //toTranslate.Add(button1.transform.GetObjectRecursively<TextMeshProUGUI>("ModViewNameTextRec"),"thememenu.category.selector");
        //toTranslate.Add(transform.GetObjectRecursively<TextMeshProUGUI>("TitleTextRec"),"repomenu.title");
    }

    public void OnRepoTab()
    {
        _modPanel.gameObject.SetActive(false);
        var buttonPrefab = transform.GetObjectRecursively<GameObject>("RepoMenuTemplateButton");
        var repoContent = transform.GetObjectRecursively<Transform>("RepoMenuRepoContentRec");
        repoContent.DestroyAllChildren();
        foreach (var repo in StarlightRepoManager.repos)
        {
            if (repo.Value == null)
            {
                var obj = Instantiate(buttonPrefab, repoContent);
                var b = obj.GetComponent<Button>();
                b.transform.GetObjectRecursively<TextMeshProUGUI>("ModViewNameTextRec").text = "BROKEN: "+repo.Key;
                b.transform.GetObjectRecursively<Image>("ModViewIconImageRec").sprite = null;
                obj.SetActive(true);
                var colorBlock = b.colors;
                colorBlock.normalColor = new Color(0.5f, 0.5f, 0.5f, 1);
                colorBlock.highlightedColor = new Color(0.7f, 0.7f, 0.7f, 1); 
                colorBlock.pressedColor = new Color(0.3f, 0.3f, 0.3f, 1); 
                colorBlock.selectedColor = new Color(0.6f, 0.6f, 0.6f, 1); 
                b.colors = colorBlock;
                    
                b.onClick.AddListener((SystemAction)(() =>
                {
                    //open repo infos
                    StarlightTextViewerPopUp.Open("There was an error fetching the repo!");
                }));
                continue;
            }
            try
            {
                var obj = Instantiate(buttonPrefab, repoContent);
                var b = obj.GetComponent<Button>();
                b.transform.GetObjectRecursively<TextMeshProUGUI>("ModViewNameTextRec").text = repo.Value.name;
                var listIcon = b.transform.GetObjectRecursively<Image>("ModViewIconImageRec");
                listIcon.sprite = null;
                if (!string.IsNullOrWhiteSpace(repo.Value.icon_url))
                    HttpEUtil.DownloadTexture2DIntoImageAsync(repo.Value.icon_url, listIcon,true,256,256);
                obj.SetActive(true);
                    
                b.onClick.AddListener((SystemAction)(() =>
                {
                    _repoPanel.gameObject.SetActive(true);
                    var repoName = _repoPanel.GetObjectRecursively<TextMeshProUGUI>("RepoViewNameTextRec");
                    var desc = _repoPanel.GetObjectRecursively<TextMeshProUGUI>("RepoViewDescriptionTextRec");
                    var hImage = _repoPanel.GetObjectRecursively<Image>("RepoViewHeaderImageRec");
                    hImage.sprite = null;
                    if (!string.IsNullOrWhiteSpace(repo.Value.header_url))
                        HttpEUtil.DownloadTexture2DIntoImageAsync(repo.Value.header_url,hImage);
                    
                    if(string.IsNullOrWhiteSpace(repo.Value.name)) repoName.gameObject.SetActive(false);
                    else {repoName.gameObject.SetActive(true); repoName.text = (repo.Value.name);}
                    
                    
                    if(string.IsNullOrWhiteSpace(repo.Value.description)) desc.gameObject.SetActive(false);
                    else {desc.gameObject.SetActive(true); desc.text = ("Description: "+repo.Value.description);}
                }));
            } catch { }
        }
    }
    public void OnBrowseTab()
    {
        _modPanel.gameObject.SetActive(false);
        var buttonPrefab = transform.GetObjectRecursively<GameObject>("RepoMenuTemplateButton");
        var browseContent = transform.GetObjectRecursively<Transform>("RepoMenuBrowseContentRec");
        browseContent.DestroyAllChildren();
        foreach (var repo in StarlightRepoManager.repos)
        {
            if (repo.Value == null) continue;
            foreach (var mod in repo.Value.mods)
            {
                if (mod == null) return;
                try
                {
                    var obj = Instantiate(buttonPrefab, browseContent);
                    var b = obj.GetComponent<Button>();
                    b.transform.GetObjectRecursively<TextMeshProUGUI>("ModViewNameTextRec").text = mod.name;
                    var listIcon = b.transform.GetObjectRecursively<Image>("ModViewIconImageRec");
                    listIcon.sprite = null;
                    if (!string.IsNullOrWhiteSpace(mod.icon_url))
                        HttpEUtil.DownloadTexture2DIntoImageAsync(mod.icon_url, listIcon,true,256,256);
                    obj.SetActive(true);
                    
                    b.onClick.AddListener((Action)(() =>
                    {
                        _modPanel.gameObject.SetActive(true);
                        var repoName = _modPanel.GetObjectRecursively<TextMeshProUGUI>("ModViewNameTextRec");
                        var author = _modPanel.GetObjectRecursively<TextMeshProUGUI>("ModViewAuthorTextRec");
                        var coauthors = _modPanel.GetObjectRecursively<TextMeshProUGUI>("ModViewCoAuthorTextRec");
                        var desc = _modPanel.GetObjectRecursively<TextMeshProUGUI>("ModViewDescriptionTextRec");
                        var company = _modPanel.GetObjectRecursively<TextMeshProUGUI>("ModViewCompanyTextRec");
                        var trademark = _modPanel.GetObjectRecursively<TextMeshProUGUI>("ModViewTrademarkTextRec");
                        var team = _modPanel.GetObjectRecursively<TextMeshProUGUI>("ModViewTeamTextRec");
                        var copyright = _modPanel.GetObjectRecursively<TextMeshProUGUI>("ModViewCopyrightTextRec");

                        var hImage = _modPanel.GetObjectRecursively<Image>("ModViewHeaderImageRec");
                        hImage.sprite = null;
                        if (!string.IsNullOrWhiteSpace(mod.header_url))
                            HttpEUtil.DownloadTexture2DIntoImageAsync(mod.header_url, hImage,true);

                        var iImage = _modPanel.GetObjectRecursively<Image>("ModViewIconImageRec");
                        iImage.sprite = null;
                        if (!string.IsNullOrWhiteSpace(mod.icon_url))
                            HttpEUtil.DownloadTexture2DIntoImageAsync(mod.icon_url, iImage,true,256,256);
                        
                        if(string.IsNullOrWhiteSpace(mod.name)) repoName.gameObject.SetActive(false);
                        else {repoName.gameObject.SetActive(true); repoName.text = (mod.name);}
                        
                        if(string.IsNullOrWhiteSpace(mod.author)) author.gameObject.SetActive(false);
                        else {author.gameObject.SetActive(true); author.text = ("Author: "+mod.author);}
                        
                        if(string.IsNullOrWhiteSpace(mod.coauthors)) coauthors.gameObject.SetActive(false);
                        else {coauthors.gameObject.SetActive(true); coauthors.text = ("Co-Authors: "+mod.coauthors);}
                        
                        if(string.IsNullOrWhiteSpace(mod.description)) desc.gameObject.SetActive(false);
                        else {desc.gameObject.SetActive(true); desc.text = ("Description: "+mod.description);}
                        
                        if(string.IsNullOrWhiteSpace(mod.company)) company.gameObject.SetActive(false);
                        else {company.gameObject.SetActive(true); company.text = ("Company: "+mod.company);}
                        
                        if(string.IsNullOrWhiteSpace(mod.trademark)) trademark.gameObject.SetActive(false);
                        else {trademark.gameObject.SetActive(true); trademark.text = ("Trademark: "+mod.trademark);}
                        
                        if(string.IsNullOrWhiteSpace(mod.team)) team.gameObject.SetActive(false);
                        else {team.gameObject.SetActive(true); team.text = ("Team: "+mod.team);}
                        
                        if(string.IsNullOrWhiteSpace(mod.copyright)) copyright.gameObject.SetActive(false);
                        else {copyright.gameObject.SetActive(true); copyright.text = ("Copyright: "+mod.copyright);}
                            
                            
                    }));
                } catch { }
            }
        }
    }
    public override void OnCloseUIPressed()
    {
        if (MenuEUtil.isAnyPopUpOpen) return;
        
        if(_repoPanel.gameObject.activeSelf)
            _repoPanel.gameObject.SetActive(false);
        else if(_modPanel.gameObject.activeSelf)
            _modPanel.gameObject.SetActive(false);
        else Close();
    }
    
}*/