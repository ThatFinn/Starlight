namespace Starlight.Enums;

public enum CallEvent : long
{
    None=0,
    
    //Negative values have custom arguments
    //Positive values don't
    
    /// <summary>
    /// Gets called in the postfix of SystemContext.Start()<br />
    /// Custom Arguments:<br />
    /// - systemContext of type SystemContext
    /// </summary>
    AfterSystemContextLoad = -1,
    /// <summary>
    /// Gets called in the postfix of GameContext.Start()<br />
    /// Custom Arguments:<br />
    /// - gameContext of type GameContext
    /// </summary>
    AfterGameContextLoad = -2,
    /// <summary>
    /// Gets called in the postfix of SceneContext.Start()<br />
    /// Custom Arguments:<br />
    /// - sceneContext of type SceneContext
    /// </summary>
    AfterSceneContextLoad = -3,
    /// <summary>
    /// Gets called in the prefix of AutoSaveDirector.Start()<br />
    /// Custom Arguments:<br />
    /// - saveDirector of type AutoSaveDirector
    /// </summary>
    BeforeSaveDirectorLoad = -4,
    /// <summary>
    /// Gets called in the postfix of AutoSaveDirector.Start()<br />
    /// Custom Arguments:<br />
    /// - saveDirector of type AutoSaveDirector
    /// </summary>
    AfterSaveDirectorLoad = -5,
    
    OnSceneStandaloneEngagementPromptLoad = 1,
    OnScenePlayerCoreLoad = 2,
    OnSceneUICoreLoad = 3,
    OnSceneMainMenuUILoad = 4,
    OnSceneLoadSceneLoad = 5,
    OnSceneZoneCoreLoad = 6,
    OnSceneStandaloneEngagementPromptUnload = 7,
    OnScenePlayerCoreUnload = 8,
    OnSceneUICoreUnload = 9,
    OnSceneMainMenuUIUnload = 10,
    OnSceneLoadSceneUnload = 11,
    OnSceneZoneCoreUnload = 12,
    OnSceneStandaloneEngagementPromptInitialize = 13,
    OnScenePlayerCoreInitialize = 14,
    OnSceneUICoreInitialize = 15,
    OnSceneMainMenuUIInitialize = 16,
    OnSceneLoadSceneInitialize = 17,
    OnSceneZoneCoreInitialize = 18,
    
    OnLoadCommands=19,
    
    //OnPrismCreateAdditions=20,
    //AfterPrismCreateAdditions=21,
    //AfterPrismLargosCreated=22
    
}