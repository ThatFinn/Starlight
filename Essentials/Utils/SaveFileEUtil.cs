using System.Linq;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppSystem.Linq;
using Starlight.Enums;
using Starlight.Storage;
using static Starlight.Enums.StarlightError;

namespace Starlight.Utils;

public static class SaveFileEUtil
{
    private static readonly Exception NoBoolException = new Exception("The value has to be a bool!");
    //private static readonly Exception NoIntException = new Exception("The value has to be an int!");
    //private static readonly Exception NoFloatException = new Exception("The value has to be a float!");
    //private static readonly Exception NoDoubleException = new Exception("The value has to be a double!");
    public static StarlightSaveFileV01 ExportSaveV01(Summary summary, bool sendErrorLogs = false) => ExportSaveV01(summary.Name, summary.SaveName,sendErrorLogs);
    public static StarlightError ExportSaveV01(Summary summary, out StarlightSaveFileV01 data) => ExportSaveV01(summary.Name, summary.SaveName,out data);

    public static StarlightSaveFileV01 ExportSaveV01(string gameName, string latestSaveName, bool sendErrorLogs = false)
    {
        var error = ExportSaveV01(gameName, latestSaveName, out StarlightSaveFileV01 data);
        if (error == NoError)
            return data;
        if(sendErrorLogs) Log("Error when exporting save: "+error);
        return null;
    }
    
    public static StarlightError ExportSaveV01(string gameName, string latestSaveName, out StarlightSaveFileV01 data)
    {
        data = null;
        if (!AllowSaveExport.HasFlag()) return NeedFlag;
        
        if (gameName.Split("_").Length !=2) return InvalidGameName;
        var stamp = gameName.Split("_")[0];
        if (!stamp.All(char.IsDigit)) return InvalidGameName;
        
        var summaries = autoSaveDirector.GetSavesByGameName(gameName);
        Dictionary<int,byte[]> savesData = new Dictionary<int,byte[]>();
        if(summaries.Count==0) return NoValidSummaries;
        var storageProvider = autoSaveDirector._storageProvider;
        
        var starlightSaveFile = new StarlightSaveFileV01(savesData,gameName.Split("_")[0], 0);
        starlightSaveFile.stamp = stamp;
        starlightSaveFile.StarlightCodeVersion = BuildInfo.CodeVersion;
        starlightSaveFile.StarlightDisplayVersion = BuildInfo.DisplayVersion;
        var hasLatest = false;
        foreach (var summary in summaries)
        {
            if(summary.IsInvalid) continue;
            var gameWithSaveIDName = summary.SaveName;
            var split = gameWithSaveIDName.Split("_");
            if (split.Length != 3) continue;
            var saveID = -1;
            try { saveID = int.Parse(split[2]); } catch { }

            if (saveID < 0) continue;
            //If save file names are messed up, prefer current one if the case:
            bool removeBefore = false;
            if (savesData.ContainsKey(saveID))
            {
                if(summary.SaveName!=latestSaveName) continue;
                removeBefore = true;
            }
            //In the storageProvider the game name also includes the auto save id
            //Normally gameName is without and SaveName with
            byte[] gameBytes = null;
            try
            {
                var stream = new Il2CppSystem.IO.MemoryStream();
                storageProvider.GetGameData(gameWithSaveIDName, stream);
                gameBytes = stream.ToArray();
                if (stream.CanRead) stream.Close();

            }
            catch (Exception e)
            {
                if(DebugLogging.HasFlag())
                {
                    LogError(e);
                    LogError("Error exporting save saveId: " + saveID);
                }
            }
            if (gameBytes == null || gameBytes.Length == 0) continue;
            if(removeBefore)
                savesData.Remove(saveID);
            savesData.Add(saveID, gameBytes);
            if (summary.SaveName == latestSaveName)
            {
                hasLatest = true;
                starlightSaveFile.latest = saveID;
                starlightSaveFile.metaGameIcon = summary.IconId.ReferenceId;
                starlightSaveFile.metaFeralEnabled = summary.FeralEnabled;
                starlightSaveFile.metaTarrEnabled = summary.TarrEnabled;
                starlightSaveFile.metaDisplayName = summary.DisplayName;
                starlightSaveFile.metaGameName = gameName;
                starlightSaveFile.metaSaveSlotIndex = summary.SaveSlotIndex;
                starlightSaveFile.metaLatestSaveNumber = summary.SaveNumber;
                starlightSaveFile.metaSR2Version = summary.Version;
            }
        }
        if (savesData.Count == 0) return NoValidSaves;
        if (!hasLatest) return LatestSaveInvalid;

        starlightSaveFile.savesData = savesData;
        data = starlightSaveFile;
        return NoError;
    }
    
    public static StarlightError ImportSaveV01(StarlightSaveFileV01 starlightSaveFile, int slotThatStartWithOne, bool loadMenuMenuOnSuccess)
    {
        if (!AllowSaveExport.HasFlag()) return NeedFlag;
        if(starlightSaveFile==null) return SaveInvalidGeneral;
        if (!starlightSaveFile.IsValid()) return SaveInvalidGeneral;
        if(!gameContext||!autoSaveDirector||autoSaveDirector._storageProvider==null) return GameNotLoadedYet;
        var storageProvider = autoSaveDirector._storageProvider;
        try
        {
            var summariesToDelete = new List<Summary>();
            foreach (var summary in autoSaveDirector.EnumerateAllSaveGamesIncludingBackups().ToList())
                if(summary.SaveSlotIndex==slotThatStartWithOne-1)
                    summariesToDelete.Add(summary);
            foreach (var summary in summariesToDelete)
            {
                try
                {
                    autoSaveDirector.DeleteGame(summary.Name);
                    autoSaveDirector._storageProvider.DeleteGameData(summary.SaveName);
                } catch { }
            }
        } catch { }

        bool failedSome = false;
        foreach (var pair in starlightSaveFile.savesData)
        {
            bool isMain = pair.Key == starlightSaveFile.latest;
            try
            {
                var stream = new Il2CppSystem.IO.MemoryStream(pair.Value);
                //var gameState = new GameV10;
                dynamic gameState = Il2CppSystem.Activator.CreateInstance(VersionedEUtil.GetLatestGameVXX());
                gameState.Load(stream);
                if (stream is { CanRead: true }) stream.Close();

                stream = new Il2CppSystem.IO.MemoryStream();

                var newDisplayName = slotThatStartWithOne.ToString();
                if (isMain&&starlightSaveFile.modifiers!=null)
                    foreach (var modifier in starlightSaveFile.modifiers)
                    {
                        if (string.IsNullOrWhiteSpace(modifier.Key)) continue;
                        try
                        {
                            switch (modifier.Key)
                            {
                                case "feralEnabled":
                                    if (!(modifier.Value is bool)) throw NoBoolException;
                                    var newFeralEnabled = modifier.Value.ToString()=="true";
                                    gameState.Summary.FeralEnabled = false;
                                    foreach (var option in gameState.GameSettings.OptionItems)
                                        if (option.PersistenceKey == "setting.FeralEnabled")
                                        {
                                            option.OptionValueKey = newFeralEnabled ? "on" : "off";
                                            break;
                                        }
                                    break;
                                case "tarrEnabled":
                                    if (!(modifier.Value is bool)) throw NoBoolException;
                                    var newTarrEnabled = modifier.Value.ToString()=="true";
                                    gameState.Summary.TarrEnabled = false;
                                    foreach (var option in gameState.GameSettings.OptionItems)
                                        if (option.PersistenceKey == "setting.TarrEnabled")
                                        {
                                            option.OptionValueKey = newTarrEnabled ? "on" : "off";
                                            break;
                                        }
                                    break;
                                case "displayName":
                                    newDisplayName = modifier.Value.ToString();
                                    break;
                                case "gameIcon":
                                    var newGameIcon = modifier.Value.ToString();
                                    var i = 0;
                                    bool foundIcon = false;
                                    foreach (var icon in gameState.GameIconIndex.IndexTable)
                                    {
                                        if (icon == newGameIcon)
                                        {
                                            foundIcon = true;
                                            break;
                                        }
                                        i++;
                                    }
                                    if (!foundIcon) throw new Exception("Icon doesn't exist!");
                                    gameState.Summary.IconId = i;
                                    gameState.GameSettings.GameIconId = i;
                                    foreach (var option in gameState.GameSettings.OptionItems)
                                        if (option.PersistenceKey == "setting.Gameicon")
                                        {
                                            option.OptionValueKey = i.ToString();
                                            break;
                                        }
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            LogError(e);
                            LogError("Error applying modifier: "+modifier.Key);
                        }
                    }
                var newGameName = starlightSaveFile.stamp + "_" + newDisplayName;
                gameState.DisplayName = newDisplayName;
                gameState.GameName = newGameName;
                gameState.SaveSlotIndex = slotThatStartWithOne-1;
                
                gameState.Write(stream);
                var gameBytes = stream.ToArray();
                if (stream != null && stream.CanRead) stream.Close();

                stream = new Il2CppSystem.IO.MemoryStream(gameBytes);
                storageProvider.StoreGameData(newDisplayName,
                    newGameName + "_" + pair.Key, stream);
                if (stream != null && stream.CanRead) stream.Close();
            }
            catch (Exception e)
            {
                failedSome = true;
                if(isMain||DebugLogging.HasFlag())
                {
                    LogError(e);
                    LogError("Error loading saveId: " + pair.Key);
                }
                if (isMain)
                {
                    foreach (var pair2 in starlightSaveFile.savesData)
                    {
                        try
                        {
                            var newDisplayName = slotThatStartWithOne.ToString();
                            var newGameName = starlightSaveFile.stamp + "_" + newDisplayName;
                            storageProvider.DeleteGameData(newGameName + "_" + pair2.Key);
                        } catch { }
                    }
                    return MainSaveIDFailed;
                }

            }
        }
        if(loadMenuMenuOnSuccess) ExecuteInTicks(() => {
            systemContext.SceneLoader.LoadMainMenuSceneGroup();
        }, 1);
        if (failedSome) return SomeSaveIDFailed;
        return NoError;
    }
    
    public static Summary GetSummaryBySlotIndex(int slotIndex)
    {
        if (!gameContext || autoSaveDirector == null) return null;
        try
        {
            Summary latestSummary = null;
            foreach (var summary in autoSaveDirector.EnumerateAllSaveGamesIncludingBackups().ToList())
            {
                if (summary.IsInvalid) continue;
                if (summary.SaveSlotIndex != slotIndex) continue;
                latestSummary = summary;
            }
            return latestSummary;
        }
        catch (Exception e)
        {
            if (DebugLogging.HasFlag()) LogError(e);
            return null;
        }
    }

    /// <summary>
    /// Skips the intro cutscene of the next new game that is created
    /// </summary>
    public static void SkipNextIntro() => Patches.MainMenu.SkipIntro.Enabled = true;

    public static StarlightError LoadSaveBySlotIndex(int slotIndex)
    {
        if (!gameContext || autoSaveDirector == null) return GameNotLoadedYet;
        
        var summary = GetSummaryBySlotIndex(slotIndex);
        if (summary == null) return NoValidSummaries;
        
        try
        {
            autoSaveDirector.Load(summary.SaveIdentifier,false);
            return NoError;
        }
        catch (Exception e)
        {
            LogError(e);
            return SaveInvalidGeneral;
        }
    }

    public static StarlightError DeleteSaveBySlotIndex(int slotIndex)
    {
        if (!gameContext || autoSaveDirector == null || autoSaveDirector._storageProvider == null) return GameNotLoadedYet;
        
        try
        {
            var summariesToDelete = new List<Summary>();
            foreach (var summary in autoSaveDirector.EnumerateAllSaveGamesIncludingBackups().ToList())
                if (summary.SaveSlotIndex == slotIndex)
                    summariesToDelete.Add(summary);
            
            if (summariesToDelete.Count == 0) return NoValidSaves;

            foreach (var summary in summariesToDelete)
            {
                try
                {
                    autoSaveDirector.DeleteGame(summary.Name);
                    autoSaveDirector._storageProvider.DeleteGameData(summary.SaveName);
                }
                catch (Exception e)
                {
                    if (DebugLogging.HasFlag()) LogError(e);
                }
            }
            
            return NoError;
        }
        catch (Exception e)
        {
            LogError(e);
            return SaveInvalidGeneral;
        }
    }
}