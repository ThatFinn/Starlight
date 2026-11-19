using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Damage;
using Il2CppMonomiPark.SlimeRancher.Economy;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Options;
using Il2CppMonomiPark.SlimeRancher.UI.Popup;
using Starlight.Buttons;
using Starlight.Commands;
using Starlight.Components.Debug;
using Starlight.Enums;
using Starlight.Enums.Sounds;
using Starlight.Managers;
using Starlight.Menus;
using Starlight.Patches.Options;
using Starlight.Popups;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace Starlight.Patches.Context;

[HarmonyPatch(typeof(GameContext), nameof(GameContext.Start))]
internal class GameContextPatch
{
    internal static CustomPauseMenuButton CheatMenuButton;
    internal static GameObject SlimeRendererPrefab;
    internal static ISlimeAppearanceObjectProvider RendererPool;
    internal static InputEvent InputDown;
    internal static InputEvent InputUp;
    internal static void Postfix(GameContext __instance)
    {
        InputDown = Get<InputEvent>("ItemDown");
        InputUp = Get<InputEvent>("ItemUp");
        StarlightEntryPoint.GameContextStarted = true;
        var damageSource = ScriptableObject.CreateInstance<DamageSourceDefinition>();
        damageSource._logMessage = "Modded.StarlightKill";
        damageSource.name = "ModdedStarlightDamage";
        damageSource.hideFlags |= HideFlags.HideAndDontSave;
        _killDamage = new Damage
        {
            Amount = 99999999, DamageSource = damageSource,
        };
        OptionsUIRootApplyPatch.Postfix();
        foreach (var particle in Resources.FindObjectsOfTypeAll<ParticleSystemRenderer>())
        {
            var pname = particle.gameObject.name.Replace(' ', '_');
            if (!LookupEUtil.FXLibrary.ContainsKey(particle.gameObject))
                LookupEUtil.FXLibrary.Add(particle.gameObject, (particle, pname));
            if (!LookupEUtil.FXLibraryReversable.ContainsKey(pname))
                LookupEUtil.FXLibraryReversable.Add(pname, (particle, particle.gameObject));
        }
        
        try
        {
            SlimeRendererPrefab = new GameObject("SlimeRenderer");
            SlimeRendererPrefab.SetActive(false);
            SlimeRendererPrefab.MakePrefab();
            var pink = Get<SlimeDefinition>("Pink");
            var instance = GameObject.Instantiate(pink.prefab,SlimeRendererPrefab.transform);
            foreach (var obj in instance.GetChildren()) if(obj.name!="Appearance") Object.Destroy(obj);
            foreach (var comp in instance.GetComponents<Component>())
            {
                if (comp is Transform || comp.TryCast<SlimeAppearanceApplicator>()) continue;
                Object.Destroy(comp);
            }
            var app = instance.GetComponent<SlimeAppearanceApplicator>();
            app.SlimeDefinition = pink;
            app.Appearance = pink.AppearancesDefault[0];
        }
        catch (Exception e) { LogError(e); }

        try
        {
            var objectPool = new ObjectPool();
            objectPool.config = ScriptableObject.CreateInstance<ObjectPoolConfig>();
            objectPool.config.loggingEnabled = false;
            objectPool.config.startupPoolMode = ObjectPoolConfig.StartupPoolMode.Awake;
            objectPool.config.startupPools = new Il2CppReferenceArray<ObjectPoolConfig.StartupPool>(0);
            objectPool.poolRoot = new GameObject("SlimeRendererPool");
            objectPool.poolRoot.MakePrefab();
            objectPool.CreateStartupPools();
            RendererPool = new PooledSlimeAppearanceObjectProvider(objectPool).TryCast<ISlimeAppearanceObjectProvider>();
        }
        catch (Exception e) { LogError(e); }
        
        if (!StarlightEntryPoint.AddedButtons)
        {
            StarlightEntryPoint.AddedButtons = true;
            if (AddModMenuButton.HasFlag())
            {
                LocalizedString label = AddTranslationFromStarlight("buttons.mods.label", "b.button_mods_starlight", "UI");
                new CustomMainMenuButton(label, EmbeddedResourceEUtil.LoadSprite("Assets.modsMenuIcon.png").CopyWithoutMipmaps(), 4, (SystemAction)(() => { MenuEUtil.GetMenu<StarlightModMenu>().Open(); }));
                new CustomPauseMenuButton(label, 3, (SystemAction)(() => { MenuEUtil.GetMenu<StarlightModMenu>().Open(); }));
                
            }
            if (AddMockMainMenuButtons.HasFlag())
            {
                var con = new CustomMainMenuContainerButton(AddTranslation("SubMenu"), null, 3, null);
                con.AddSubButton(new CustomMainMenuButton(AddTranslation("InSubmenu1"), null, 0, (SystemAction)(() => { StarlightTextViewerPopUp.Open("This is a button in the submenu1"); })));
                con.AddSubButton(new CustomMainMenuButton(AddTranslation("InSubmenu2"), null, 0, (SystemAction)(() => { StarlightTextViewerPopUp.Open("This is a button in the submenu2"); })));
                con.AddSubButton(new CustomMainMenuButton(AddTranslation("InSubmenu3"), null, 0, (SystemAction)(() => { StarlightTextViewerPopUp.Open("This is a button in the submenu3"); })));
                con.AddSubButton(new CustomMainMenuButton(AddTranslation("InSubmenu4"), null, 0, (SystemAction)(() => { StarlightTextViewerPopUp.Open("This is a button in the submenu4"); })));
                con.AddSubButton(new CustomMainMenuButton(AddTranslation("InSubmenu5"), null, 0, (SystemAction)(() => { StarlightTextViewerPopUp.Open("This is a button in the submenu5"); })));
                con.AddSubButton(new CustomMainMenuButton(AddTranslation("InBoth"), null, 0, (SystemAction)(() => { StarlightTextViewerPopUp.Open("This is a button in both"); })), false);
                // Sub Sub menus dont work :(
                //var three = new CustomMainMenuButton(AddTranslation("InAllThree"), null, 0, (SystemAction)(() => { StarlightTextViewer.Open("InAllThree"); }));
                //con.AddSubButton(three,false);
                //var subsub = new CustomMainMenuContainerButton(AddTranslation("SubSubMenu"), null, 0, null);
                //con.AddSubButton(subsub);
                //subsub.AddSubButton(new CustomMainMenuButton(AddTranslation("InSubSubmenu1"), null, 0, (SystemAction)(() => { StarlightTextViewer.Open("This is a button in the subsubmenu1"); })));
                //subsub.AddSubButton(new CustomMainMenuButton(AddTranslation("InSubSubmenu2"), null, 0, (SystemAction)(() => { StarlightTextViewer.Open("This is a button in the subsubmenu2"); })));
                //subsub.AddSubButton(three,false);

            }
            if (AddCheatMenuButton.HasFlag()) CheatMenuButton = new CustomPauseMenuButton(AddTranslationFromStarlight("buttons.cheatmenu.label", "b.button_cheatmenu_starlight", "UI"), 4, (SystemAction)(() => { MenuEUtil.GetMenu<StarlightCheatMenu>().Open(); }));
            new CustomPauseMenuButton(AddTranslationFromStarlight("buttons.debugplayer.label", "b.debug_player_starlight", "UI"), 3, (SystemAction)(StarlightDebugUI.DebugStatsManager.TogglePlayerDebugUI));
            if (AddMockOptionsUIButtons.HasFlag())
            {
                var testCategory1 = new CustomOptionsUICategory(AddTranslation("AllTheTime"), 4,null, OptionsCategoryVisibleState.AllTheTime);
                var testCategory2 = new CustomOptionsUICategory(AddTranslation("MainOnly"), 4,null, OptionsCategoryVisibleState.MainMenuOnly);
                var testCategory3 = new CustomOptionsUICategory(AddTranslation("InGameOnly"), 4,null, OptionsCategoryVisibleState.InGameOnly);
                testCategory1.AddButton(new CustomOptionsButtonValues
                (AddTranslation("GlobalTest"),AddTranslation("This is an example description"),
                    "starlight.mock.global1",1,true,false,false, ((value) =>
                    {
                        Log("It has been changed to "+value);
                    }), OptionsButtonType.OptionsUI,
                    AddTranslation("Value0"),AddTranslation("Value1"),AddTranslation("Value2"),AddTranslation("Value3")
                    ));
                testCategory2.AddButton(new CustomOptionsButtonValues
                (AddTranslation("MainMenuOnlyTest"),AddTranslation("This wraps around!"),
                    "starlight.mock.mainmenuonly1",1,true,true,false, ((value) =>
                    {
                        Log("It has been changed to "+value);
                    }), OptionsButtonType.OptionsUI,
                    AddTranslation("G0"),AddTranslation("G1"),AddTranslation("G2"),AddTranslation("G3")
                ));
                testCategory3.AddButton(new CustomOptionsButtonValues
                (AddTranslation("InGameOnlyTest"),AddTranslation("This doesn't apply immediately!"),
                    "starlight.mock.ingameonly1",1,false,false,false, ((value) =>
                    {
                        Log("It has been changed to "+value);
                    }), OptionsButtonType.InGameOptionsUIOnly,
                    AddTranslation("Value0"),AddTranslation("Value1"),AddTranslation("Value2"),AddTranslation("Value3")
                ));
                var coolButton = new CustomOptionsButtonValues
                (AddTranslation("MultiCategoryTest"), AddTranslation("This is in multiple categories! And shouldnt be 2 times in MainMenuOnly!"),
                    "starlight.mock.multitest", 1, true, false, false, ((value) =>
                    {
                        Log("It has been changed to " + value);
                    }), OptionsButtonType.OptionsUI,
                    AddTranslation("Value0"), AddTranslation("Value1"), AddTranslation("Value2"),
                    AddTranslation("Value3")
                );
                testCategory1.AddButton(coolButton);
                testCategory2.AddButton(coolButton);
                testCategory2.AddButton(coolButton);
                testCategory3.AddButton(coolButton);
            }
        }

        Time.timeScale = 1f;
        try
        {
            LookupEUtil.CloseInput = Get<InputEvent>("Close");
            if(LookupEUtil.CloseInput != null)
                LookupEUtil.CloseInput.add_Performed((System.Action<InputEventData>)((_) =>
                {
                    var menu = MenuEUtil.GetOpenMenu();
                    if(menu!=null) menu.OnCloseUIPressed();
                    try
                    {
                        var allow = true;
                        foreach (var prompt in GetAllInScene<PopupPrompt>())
                        {
                            if (prompt.name.Contains("Clone"))
                            {
                                allow = false;
                                break;
                            }
                        }
                        if(allow) GetAnyInScene<OptionsUIRoot>().Close();
                    }
                    catch { }
                }));
            LookupEUtil.ActionMaps = new Dictionary<string, InputActionMap>();
            LookupEUtil.MainGameActions = new Dictionary<string, InputAction>();
            LookupEUtil.PausedActions = new Dictionary<string, InputAction>();
            LookupEUtil.DebugActions = new Dictionary<string, InputAction>();
            foreach (var map in gameContext.InputDirector._inputActions.actionMaps)
                LookupEUtil.ActionMaps.Add(map.name, map);
            foreach (InputAction action in LookupEUtil.ActionMaps["MainGame"].actions)
                LookupEUtil.MainGameActions.Add(action.name, action);
            foreach (InputAction action in LookupEUtil.ActionMaps["Paused"].actions)
                LookupEUtil.PausedActions.Add(action.name, action);
            foreach (InputAction action in LookupEUtil.ActionMaps["Debug"].actions)
                LookupEUtil.DebugActions.Add(action.name, action);
        }
        catch (Exception e)
        {
            LogError(e);
            LogError("There was a problem loading SR2 action maps!");
        }

        try
        {
            AudioEUtil.MenuSounds.Add(MenuSound.Click,Get<SECTR_AudioCue>("Click1"));
            AudioEUtil.MenuSounds.Add(MenuSound.SelectCategory,Get<SECTR_AudioCue>("Click2"));
            AudioEUtil.MenuSounds.Add(MenuSound.Apply,Get<SECTR_AudioCue>("Click3"));
            AudioEUtil.MenuSounds.Add(MenuSound.Pop,Get<SECTR_AudioCue>("Click4"));
            AudioEUtil.MenuSounds.Add(MenuSound.ButtonFocused,Get<SECTR_AudioCue>("Click5"));
            AudioEUtil.MenuSounds.Add(MenuSound.Hover,Get<SECTR_AudioCue>("ClickRollover"));
            AudioEUtil.MenuSounds.Add(MenuSound.Error,Get<SECTR_AudioCue>("ClickError"));
            AudioEUtil.MenuSounds.Add(MenuSound.OpenMenu,Get<SECTR_AudioCue>("UIOpen"));
            AudioEUtil.MenuSounds.Add(MenuSound.CloseMenu,Get<SECTR_AudioCue>("UIClose"));
            AudioEUtil.MenuSounds.Add(MenuSound.OpenPopup,Get<SECTR_AudioCue>("UIOpen2"));
            AudioEUtil.MenuSounds.Add(MenuSound.ClosePopup,Get<SECTR_AudioCue>("UIClose2"));
            //AudioEUtil._defaultMenuSounds = Get<UIAudioTable>("DefaultMenuSounds");
        }
        catch (Exception e)
        {
            LogError(e);
            LogError("There was a problem loading sounds!");
        }

        if(RestoreDebugFPSViewer.HasFlag()) foreach (var display in GetAllInScene<FPSDisplay>())
            display.AddComponent<FPSDisplayFixer>();
        if (RestoreDebugDebugUI.HasFlag()) __instance.AddComponent<DebugDirectorFixer>();
        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
            try { expansion.AfterGameContext(__instance); }
            catch (Exception e) { LogError(e); }
        StarlightCallEventManager.ExecuteWithArgs(CallEvent.AfterGameContextLoad, ("gameContext", __instance));
        foreach (var pair in StarlightEntryPoint.Menus)
            try { pair.Key.AfterGameContext(__instance); }
            catch (Exception e) { LogError(e); }
        foreach (var pair in StarlightCommandManager.Commands)
            try { pair.Value.AfterGameContext(__instance); }
            catch (Exception e) { LogError(e); }

        foreach (var def in GetAny<CurrencyList>()._currencies.ToNetArray())
        {
            var cmd = new CurrencyCommand();
            cmd.refID = def.ReferenceId.Replace("CurrencyDefinition.","");
            cmd.name = def.ReferenceId.Replace("CurrencyDefinition.","");
            if(cmd.name.EndsWith("s")) cmd.name=cmd.name.Substring(0, cmd.name.Length - 1);
            StarlightCommandManager.RegisterCommand(cmd);
        }
    }
}