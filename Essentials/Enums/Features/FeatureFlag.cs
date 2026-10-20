using System;

namespace Starlight.Enums.Features;

public enum FeatureFlag
{
    None=0,
    //Dev
    /// <summary>
    /// Activates Devmode
    /// </summary>
    DevMode=10,
    /// <summary>
    /// Enable test dev menu command
    /// </summary>
    DevTestMenu=11,
    /// <summary>
    /// Shows extra info in into MLLog
    /// </summary>
    DebugLogging=20,
    /// <summary>
    /// Show Unity related errors from Debug.Log, Debug.Warning and Debug.Error
    /// </summary>
    ShowUnityErrors=30,
    /// <summary>
    /// Show save game related errors and skips them
    /// </summary>
    IgnoreSaveErrors=40,
    /// <summary>
    /// Shows world populator errors
    /// </summary>
    ShowWorldPopulatorErrors=50,
    /// <summary>
    /// Tries to ignore world populator errors
    /// </summary>
    IgnoreWorldPopulatorErrors=60,
    /// <summary>
    /// Enable experimental features
    /// </summary>
    Experiments=70,
    /// <summary>
    /// Allow prism
    /// </summary>
    AllowPrism=80,
    /// <summary>
    /// Enable the enable dev key codes
    /// </summary>
    ExperimentalKeyCodes=90,
    /// <summary>
    /// Enables the repo manager
    /// </summary>
    [Obsolete("Deprecated")] EnableRepoManagement=100,
    /// <summary>
    /// Add test repo
    /// </summary>
    [Obsolete("Deprecated")] UseMockRepo=101, //
    
    //Commands+Dev
    /// <summary>
    /// Load dev-only commands
    /// </summary>
    CommandsLoadDevOnly=110, 
    /// <summary>
    /// Enable experimental commands
    /// </summary>
    CommandsLoadExperimental=120, 
    
    //Commands
    /// <summary>
    /// Load commands
    /// </summary>
    CommandsLoadCommands=130, //
    /// <summary>
    /// Load cheating commands
    /// </summary>
    CommandsLoadCheat=140, //
    /// <summary>
    /// Load binding commands
    /// </summary>
    CommandsLoadBinding=150, //
    /// <summary>
    /// Load warping commands
    /// </summary>
    CommandsLoadWarp=160, //
    /// <summary>
    /// Load common commands
    /// </summary>
    CommandsLoadCommon=170, //
    /// <summary>
    /// Load menu commands
    /// </summary>
    CommandsLoadMenu=180, //
    /// <summary>
    /// Load miscellaneous commands
    /// </summary>
    CommandsLoadMiscellaneous=190, //
    /// <summary>
    /// Load fun commands
    /// </summary>
    CommandsLoadFun=200, //
    /// <summary>
    /// Load landplot commands
    /// </summary>
    CommandsLoadLandPlot=201, //
    
    //Cheats and Mods
    /// <summary>
    /// Disable everything related to cheats
    /// </summary>
    DisableCheats=210,
    /// <summary>
    /// Allow adding the cheat menu button to the pause menu
    /// </summary>
    AddCheatMenuButton=220, //
    /// <summary>
    /// Enable infhealth command and its cheat menu equivalent
    /// </summary>
    EnableInfHealth=230, //
    /// <summary>
    /// Enable infenergy command and its cheat menu equivalent
    /// </summary>
    EnableInfEnergy=240, //
    
    //Misc
    /// <summary>
    /// Allow adding the mod menu button to the pause menu and main menu
    /// </summary>
    AddModMenuButton=250, //
    /// <summary>
    /// Allow loading and integrating Starlight expansions
    /// </summary>
    AllowExpansions=260, //
    /// <summary>
    /// Allow loading and integrating Starlight expansions of V1
    /// </summary>
    AllowExpansionsV1=270, //
    /// <summary>
    /// Modify the version text in the main menu
    /// </summary>
    EnableLocalizedVersionPatch=280, //
    /// <summary>
    /// Inject Translations into unity's localization system
    /// </summary>
    InjectTranslations=290, //
    /// <summary>
    /// Use the patch for il2cpp exceptions
    /// </summary>
    EnableIl2CppDetourExceptionReporting=300, //
    /// <summary>
    /// Enable the save export feature
    /// </summary>
    AllowSaveExport=310, //
    
    //Menus
    /// <summary>
    /// Enable the mod menu
    /// </summary>
    EnableModMenu=320, //
    /// <summary>
    /// Enable the cheat menu
    /// </summary>
    EnableCheatMenu=330, //
    /// <summary>
    /// Enable the console menu
    /// </summary>
    EnableConsole=340, //
    /// <summary>
    /// Enable the theme menu
    /// </summary>
    EnableThemeMenu=350, //
    /// <summary>
    /// Enable the repo menu
    /// </summary>
    [Obsolete("Deprecated")] EnableRepoMenu=360, //
    /// <summary>
    /// Enable the studio menu
    /// </summary>
    EnableStudioMenu=365, //
    /// <summary>
    /// Enable the EmberMode menubar
    /// </summary>
    EnableEmberMode=366, //
    
    //UI
    /// <summary>
    /// Inject buttons in the main menu
    /// </summary>
    InjectMainMenuButtons=370, //
    /// <summary>
    /// Inject test buttons in the main menu
    /// </summary>
    AddMockMainMenuButtons=371, //
    /// <summary>
    /// Inject buttons in the ranch house ui
    /// </summary>
    InjectRanchUIButtons=380, //
    /// <summary>
    /// Inject buttons in the pause menu
    /// </summary>
    InjectPauseButtons=390, //
    /// <summary>
    /// Inject buttons in the options ui
    /// </summary>
    InjectOptionsButtons=400,
    /// <summary>
    /// Inject test buttons in the OptionsUI
    /// </summary>
    AddMockOptionsUIButtons=401, //

    //Updates and Patches
    /// <summary>
    /// Allow checking for new Starlight updates
    /// </summary>
    CheckForUpdates=410, //
    /// <summary>
    /// Allow auto updating if an update has been found
    /// </summary>
    AllowAutoUpdate=420, //
    /// <summary>
    /// Make SystemContext.isModded true
    /// </summary>
    ChangeSystemContextIsModded=430, //
    /// <summary>
    /// Redirects the StorageProvider to a FileStorgeProvider
    /// As a result the save path will be changed
    /// It will be redirected to the Starlight folder
    /// </summary>
    RedirectSaveFiles=440, //
    /// <summary>
    /// Tries to restore Debug Abilities.<br/>
    /// This isn't a faithful recreation of the true SR2 Debug tools.<br/>
    /// However there are enough breadcrumbs in the code which.<br/>
    /// help to make sense of the missing code and try to implement it as best as possible.<br/>
    /// You need to enable the individual abilities
    /// </summary>
    RestoreDebugAbilities=500, //
    /// <summary>
    /// Tries to restore Debug Ability.<br/>
    /// This restore the FPS counter
    /// </summary>
    RestoreDebugFPSViewer=501, //
    /// <summary>
    /// Tries to restore Debug Ability.<br/>
    /// This restore the PlayerDebug ui
    /// </summary>
    RestoreDebugPlayerDebug=502, //
    /// <summary>
    /// Tries to restore Debug Ability.<br/>
    /// This restore the unity dev console
    /// </summary>
    RestoreDebugDevConsole=503, //
    /// <summary>
    /// Tries to restore Debug Ability.<br/>
    /// This restore the DebugUI
    /// </summary>
    RestoreDebugDebugUI=504, //
    /// <summary>
    /// This fixes loading SceneGroups with invalid scene references<br/>
    /// If it stumbles across an invalid scene, it will be skipped
    /// </summary>
    TryFixingInvalidSceneGroups=530, //
    /// <summary>
    /// It's for primarily for development of mods<br/>
    /// This will force load the main menu, however it will break things and thus is only for developing mods
    /// </summary>
    ForceLoadMainMenu=540, //
    /// <summary>
    /// This force loads volume presets with invalid components
    /// </summary>
    TryFixingInvalidVolumePresets=550, //
    /// <summary>
    /// This exports all volume presets after loading them
    /// </summary>
    ExportAllVolumePresets=560, //
    /// <summary>
    /// Support everything related to radiancy
    /// </summary>
    SupportRadiant=570, //
    
    
    
    

}