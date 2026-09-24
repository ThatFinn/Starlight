using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Starlight.Expansion;
using Starlight.Managers;
using Starlight.Saving;
using Starlight.Storage;

namespace Starlight.Patches.Saving;
   
[HarmonyPriority(99999999)]
[HarmonyPatch(typeof(GameModelPushHelpers), nameof(GameModelPushHelpers.PushGame))]
internal static class CustomSaveDataLoadPatch
{
    private static Dictionary<StarlightExpansionV01, (RootSave, LoadingGameSessionData)> _rootSaves = new();
    private static Dictionary<StarlightExpansionV01, LoadingGameSessionData> _noRootSaves = new();
    internal const string DataPrefix = "StarlightDataV01";
    internal const string DataPrefixOwn = "StarlightCoreDataV01";

    internal static void ExecSaveDataReceived()
    {
        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
            if (_rootSaves.ContainsKey(expansion))
                try { expansion.OnCustomSaveDataReceived(_rootSaves[expansion].Item1, _rootSaves[expansion].Item2); } 
                catch (Exception e) { LogError(e); }
            else if(_noRootSaves.ContainsKey(expansion))
                try { expansion.OnNoCustomSaveDataReceived(_noRootSaves[expansion]); }
                catch (Exception e) { LogError(e); }
        _rootSaves = new Dictionary<StarlightExpansionV01, (RootSave, LoadingGameSessionData)>();
        _noRootSaves = new Dictionary<StarlightExpansionV01, LoadingGameSessionData>();
    }
    internal static void Prefix(ActorIdProvider actorIdProvider, ISaveReferenceTranslation saveReferenceTranslation, dynamic gameState, GameModel gameModel)
    {
        bool hasExecutedOwn = false;
        foreach (var entry in MiscEUtil.ToNetArray(gameState.ZoneIndex))
            if (entry.StartsWith(DataPrefixOwn))
            {
                try
                {
                    string remaining = entry.Substring(DataPrefixOwn.Length);
                    var rawBytes = remaining.DecodeFromBase128();
                    var rootSave = RootSave.FromBytes<StarlightCustomInGameData>(rawBytes);
                    var sessionData = new LoadingGameSessionData(actorIdProvider, saveReferenceTranslation, saveReferenceTranslation.ToNonIVariant(), gameState, gameModel);
                    hasExecutedOwn = true;
                    StarlightSaveManager.OnInGameLoad(rootSave,sessionData);
                }catch (Exception e) { LogError(e); }
            }
        if(!hasExecutedOwn)
            try
            {
                var sessionData = new LoadingGameSessionData(actorIdProvider, saveReferenceTranslation, saveReferenceTranslation.ToNonIVariant(), gameState, gameModel);
                StarlightSaveManager.OnInGameLoad(null,sessionData);
            }catch (Exception e) { LogError(e); }
        
        
        _rootSaves = new Dictionary<StarlightExpansionV01, (RootSave, LoadingGameSessionData)>();
        _noRootSaves = new Dictionary<StarlightExpansionV01, LoadingGameSessionData>();
        var executedExpansions = new List<StarlightExpansionV01>();
        foreach (var entry in MiscEUtil.ToNetArray(gameState.ZoneIndex))
            if (entry.StartsWith(DataPrefix))
            {
                string remaining = entry.Substring(DataPrefix.Length);
                
                if (remaining.Length >= 32)
                {
                    string md5Hash = remaining.Substring(0, 32);

                    foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
                        try
                        {
                            var info = expansion.GetPackageInfoFromExpansion().Value;
                            if(info.ID.CreateMD5() == md5Hash)
                            {
                                var sessionData = new LoadingGameSessionData(actorIdProvider, saveReferenceTranslation, saveReferenceTranslation.ToNonIVariant(), gameState, gameModel);
                                RootSave rootSave = null; 
                                try
                                {
                                    var rawBytes = remaining.Substring(32).DecodeFromBase128();
                                    rootSave = RootSave.FromBytes(rawBytes);
                                    if (rootSave == null) throw new Exception("Save Data is null!");
                                    _rootSaves.Add(expansion, (rootSave, sessionData));
                                    executedExpansions.Add(expansion);
                                }
                                catch (Exception e)
                                {
                                    LogError($"Failed to save custom save data for expansion {info.Name}: {e}");
                                }
                                if(rootSave!=null)
                                    try
                                    {
                                        expansion.OnEarlyCustomSaveDataReceived(rootSave, sessionData);
                                    }
                                    catch (Exception e) { LogError(e); }
                            }
                        } catch { }
                }
                else LogError("An error occured while loading some custom save data!");
            }
        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
            if(!executedExpansions.Contains(expansion))
                try
                {
                    var sessionData = new LoadingGameSessionData(actorIdProvider, saveReferenceTranslation, saveReferenceTranslation.ToNonIVariant(), gameState, gameModel);
                    _noRootSaves.Add(expansion, sessionData);

                    expansion.OnEarlyNoCustomSaveDataReceived(sessionData);
                }
                catch (Exception e) { LogError(e); }
    }
}