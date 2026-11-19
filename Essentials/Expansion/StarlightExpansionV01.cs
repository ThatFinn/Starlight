using System;
using System.IO;
using Il2CppMonomiPark.SlimeRancher;
using MelonLoader.Utils;
using Starlight.Managers;
using Starlight.Saving;
using Starlight.Storage;
using Starlight.Storage.Prefs;

namespace Starlight.Expansion;

public abstract class StarlightExpansionV01 : StarlightExpansionVXX
{
    public string modDataPath {
        get
        {
            // This should error if it fails on purpose
            // ReSharper disable once PossibleInvalidOperationException
            if (_modDataPath == null)
                _modDataPath = Path.Combine(MelonEnvironment.UserDataDirectory, this.GetPackageInfoFromExpansion().Value!.ID);
            return _modDataPath;
        }
    }
    private string _modDataPath = null;
    
    public PackagePrefs prefs => _prefs;
    internal PackagePrefs _prefs = null;
    
    protected StarlightExpansionV01() {}
    
    #region Native Events

    /// <summary>
    /// Runs when the Expansion is registered. This callback should only be used as a constructor.
    /// </summary>
    public virtual void OnEarlyInitialize() { }
    
    /// <summary>
    /// Runs after the Expansion has registered. This waits until everything has been initialized.
    /// </summary>
    public virtual void OnInitialize() { }

    /// <summary>
    /// Runs after <see cref="OnInitialize"/>. This waits until Unity has started up.
    /// </summary>
    public virtual void OnLateInitialize() => OnLateInitializeMelon();

    /// <summary>
    /// Runs after <see cref="OnInitialize"/>. This waits until Unity has started up.
    /// </summary>
    [Obsolete("Use OnLateInitialize instead")] public virtual void OnLateInitializeMelon() { }
    
    /// <summary>
    /// Runs once per frame. Same as the one in "MonoBehavior".
    /// </summary>
    public virtual void OnUpdate() { }

    /// <summary>
    /// Runs once per frame, after <see cref="OnUpdate"/>. Same as the one in "MonoBehavior".
    /// </summary>
    public virtual void OnLateUpdate() { }
    
    /// <summary>
    /// Same as the one in "MonoBehavior".
    /// </summary>
    public virtual void OnFixedUpdate() { }
    
    /// <summary>
    /// Same as the one in "MonoBehavior".
    /// Forwarded from ML
    /// </summary>
    public virtual void OnGUI() { }

    /// <summary>
    /// Runs once the application quits
    /// </summary>
    public virtual void OnApplicationQuit() { }
    
    /// <summary>
    /// Runs when a new Scene is loaded.
    /// </summary>
    public virtual void OnSceneWasLoaded(int buildIndex, string sceneName) { }

    /// <summary>
    /// Runs once a Scene is initialized.
    /// </summary>
    public virtual void OnSceneWasInitialized(int buildIndex, string sceneName) { }

    /// <summary>
    /// Runs once a Scene unloads.
    /// </summary>
    public virtual void OnSceneWasUnloaded(int buildIndex, string sceneName) { }
    
    /// <summary>
    /// Runs when the Expansion is unloaded.
    /// </summary>
    public virtual void OnUnload() { }
    
    /// <summary>
    /// Runs when the PackagePrefs are created.
    /// </summary>
    public virtual void OnCreatePrefs() {}
    #endregion
    
    #region Starlight Scene Events
    
    /// <summary>
    /// Gets executed every time the scene "StandaloneEngagementPrompt" gets loaded.
    /// </summary>
    public virtual void OnStandaloneEngagementPromptLoad() { }
    /// <summary>
    /// Gets executed every time the scene "StandaloneEngagementPrompt" gets unloaded.
    /// </summary>
    public virtual void OnStandaloneEngagementPromptUnload() { }
    /// <summary>
    /// Gets executed every time the scene "StandaloneEngagementPrompt" gets initialized.
    /// </summary>
    public virtual void OnStandaloneEngagementPromptInitialize() { }
    
    /// <summary>
    /// Gets executed every time the scene "PlayerCore" gets loaded.
    /// </summary>
    public virtual void OnPlayerCoreLoad() { }
    /// <summary>
    /// Gets executed every time the scene "PlayerCore" gets unloaded.
    /// </summary>
    public virtual void OnPlayerCoreUnload() { }
    /// <summary>
    /// Gets executed every time the scene "PlayerCore" gets initialized.
    /// </summary>
    public virtual void OnPlayerCoreInitialize() { }
    
    /// <summary>
    /// Gets executed every time the scene "UICore" gets loaded.
    /// </summary>
    public virtual void OnUICoreLoad() { }
    /// <summary>
    /// Gets executed every time the scene "UICore" gets unloaded.
    /// </summary>
    public virtual void OnUICoreUnload() { }
    /// <summary>
    /// Gets executed every time the scene "UICore" gets initialized.
    /// </summary>
    public virtual void OnUICoreInitialize() { }
    
    /// <summary>
    /// Gets executed every time the scene "MainMenuUI" gets loaded.
    /// </summary>
    public virtual void OnMainMenuUILoad() { }
    /// <summary>
    /// Gets executed every time the scene "MainMenuUI" gets unloaded.
    /// </summary>
    public virtual void OnMainMenuUIUnload() { }
    /// <summary>
    /// Gets executed every time the scene "MainMenuUI" gets initialized.
    /// </summary>
    public virtual void OnMainMenuUIInitialize() { }
    
    /// <summary>
    /// Gets executed every time the scene "LoadScene" gets loaded.
    /// </summary>
    public virtual void OnLoadSceneLoad() { }
    /// <summary>
    /// Gets executed every time the scene "LoadScene" gets unloaded.
    /// </summary>
    public virtual void OnLoadSceneUnload() { }
    /// <summary>
    /// Gets executed every time the scene "LoadScene" gets initialized.
    /// </summary>
    public virtual void OnLoadSceneInitialize() { }
    
    /// <summary>
    /// Gets executed every time the scene "ZoneCore" gets loaded.
    /// </summary>
    public virtual void OnZoneCoreLoad() { }
    /// <summary>
    ///   Gets executed every time the scene "ZoneCore" gets initialized.
    /// </summary>
    public virtual void OnZoneCoreInitialize() { }
    /// <summary>
    /// Gets executed every time the scene "ZoneCore" gets unloaded.
    /// </summary>
    public virtual void OnZoneCoreUnloaded() { }
    #endregion
    
    #region Starlight Context & Directors Events
    /// <summary>
    /// Gets executed once SystemContext loads. In Postfix of the Start method
    /// </summary>
    public virtual void AfterSystemContext(SystemContext systemContext) { }
    
    /// <summary>
    /// Gets executed once GameContext loads. In Postfix of the Start method
    /// </summary>
    public virtual void AfterGameContext(GameContext gameContext) { }
    
    /// <summary>
    /// Gets executed once SceneContext loads. In Postfix of the Start method
    /// </summary>
    public virtual void AfterSceneContext(SceneContext sceneContext) { }
    
    /// <summary>
    /// Gets called before the AutoSaveDirector has been loaded
    /// </summary>
    public virtual void BeforeSaveDirector(AutoSaveDirector saveDirector) { }
    
    /// <summary>
    /// Gets called after the AutoSaveDirector has been loaded<br />
    /// You should use this method to add your translations to SR2<br />
    /// and to add your own buttons.
    /// </summary>
    public virtual void AfterSaveDirector(AutoSaveDirector saveDirector) { }
    #endregion
    
    #region Starlight
    
    /// <summary>
    /// Loads the custom SaveRoot from a save file<br />
    /// Check the type before proceeding<br />
    /// This gets executed as soon as the save file has been loaded<br />
    /// Even before the game has spawned all it's actors etc.
    /// </summary>
    public virtual void OnEarlyCustomSaveDataReceived(RootSave saveRoot, LoadingGameSessionData loadingGameSessionData) { }

    /// <summary>
    /// Same as OnEarlyCustomSaveDataReceived but only gets called if this mod has no custom save data<br />
    /// This gets executed as soon as the save file has been loaded<br />
    /// Even before the game has spawned all it's actors etc.
    /// </summary>
    public virtual void OnEarlyNoCustomSaveDataReceived(LoadingGameSessionData loadingGameSessionData) { }
    
    /// <summary>
    /// Loads the custom SaveRoot from a save file<br />
    /// Check the type before proceeding<br />
    /// </summary>
    public virtual void OnCustomSaveDataReceived(RootSave saveRoot, LoadingGameSessionData loadingGameSessionData) { }

    /// <summary>
    /// Same as OnCustomSaveDataReceived but only gets called if this mod has no custom save data
    /// </summary>
    public virtual void OnNoCustomSaveDataReceived(LoadingGameSessionData loadingGameSessionData) { }
    
    ///.
    /// Gets executed everytime a save file is being saved<br/>
    /// Return null if you don't use custom save data
    /// </summary>
    /// <returns>RootSave</returns>
    public virtual RootSave OnSaveCustomSaveData(SavingGameSessionData savingGameSessionData) { return null; }

    /// <summary>
    /// Gets executed once all commands get registered.<br />
    /// You should use this method to register your own commands,<br />
    /// with the StarlightCommand.CommandType.DontLoad flag,<br />
    /// by using StarlightConsole.RegisterCommand(new YourCommand()).
    /// </summary>
    public virtual void OnLoadCommands() { }
    
    #endregion
    
    #region Prism Events
    /// <summary>
    /// In this function you should add all of your base slimes, veggies, toys etc.<br />
    /// Requires Prism
    /// </summary>
    public virtual void OnPrismCreateAdditions() { }
    
    /// <summary>
    /// Use this if you want to do stuff with every e.g slime, veggie etc.<br />
    /// DO NOT add objects here, do that in <c>OnPrismCreateAdditions</c><br />
    /// This gets called after every mod ran OnPrismCreateAdditions()<br />
    /// Requires Prism
    /// </summary>
    public virtual void AfterPrismCreateAdditions() { }
    
    /// <summary>
    /// This gets called after all largos have been created<br />
    /// Requires Prism
    /// </summary>
    public virtual void AfterPrismLargosCreated() { }
    #endregion
}