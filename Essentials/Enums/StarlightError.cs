namespace Starlight.Enums;

public enum StarlightError
{
    /// <summary>
    /// Returns when there is no error
    /// </summary>
    NoError=0,
    /// <summary>
    /// Returns when the action is only available while in a save file
    /// </summary>
    NotInGame=1,
    /// <summary>
    /// Returns when the player object is null
    /// </summary>
    PlayerNull=2,
    /// <summary>
    /// Returns when the teleportable player is null
    /// </summary>
    TeleportablePlayerNull=3,
    /// <summary>
    /// Returns when the character controller is null
    /// </summary>
    SRCharacterControllerNull=4,
    /// <summary>
    /// Returns when the SceneGroup is not supported
    /// </summary>
    SceneGroupNotSupported=5,
    /// <summary>
    /// Returns when an object under the same name already exists
    /// </summary>
    AlreadyExists=6,
    /// <summary>
    /// Returns when no object under said name exists
    /// </summary>
    DoesntExist=7,
    /// <summary>
    /// Returns when loading a save and the entire save file is invalid
    /// </summary>
    SaveInvalidGeneral=8,
    /// <summary>
    /// Returns when not in the main menu
    /// </summary>
    NotInMainMenu=9,
    /// <summary>
    /// Returns when the game hasn't fully loaded yet
    /// </summary>
    GameNotLoadedYet=10,
    /// <summary>
    /// Returns when the provided file path is null or invalid
    /// </summary>
    FilePathNull=11,
    /// <summary>
    /// Returns when one or more save id's failed, but not the main save
    /// </summary>
    SomeSaveIDFailed=12,
    /// <summary>
    /// Returns when the main save failed to load
    /// </summary>
    MainSaveIDFailed=13,
    /// <summary>
    /// Returns when the latest save is invalid
    /// </summary>
    LatestSaveInvalid=14,
    /// <summary>
    /// Returns when the "Experiments" FeatureFlag needs to be on
    /// </summary>
    NeedExperiment=15,
    /// <summary>
    /// Returns when the game name is invalid
    /// </summary>
    InvalidGameName=16,
    /// <summary>
    /// Returns when there are no valid saves
    /// </summary>
    NoValidSaves=17,
    /// <summary>
    /// Returns when there are no valid summaries
    /// </summary>
    NoValidSummaries=18,
    /// <summary>
    /// Returns when a FeatureFlag needs to be active
    /// </summary>
    NeedFlag=19,
    /// <summary>
    /// Returns when it is invalid
    /// </summary>
    Invalid=20,
}