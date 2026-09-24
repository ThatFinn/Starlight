using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Starlight.Managers;
using Starlight.Storage;

namespace Starlight.Patches.Saving;

[HarmonyPriority(99999999)]
[HarmonyPatch(typeof(GameModelPullHelpers), nameof(GameModelPullHelpers.PullGame))]
internal static class CustomSaveDataSavePatch
{
    internal const string DataPrefix = "StarlightDataV01";
    internal const string DataPrefixOwn = "StarlightCoreDataV01";
    internal static void Postfix(GameModel gameModel,SavedGameInfoProvider savedGameInfoProvider,
        ISaveReferenceTranslation saveReferenceTranslation, GameMetadata metadata, ref dynamic __result )
    {
        try
        {
            var rootSave = StarlightSaveManager.OnInGameSave(new SavingGameSessionData(saveReferenceTranslation, saveReferenceTranslation.ToNonIVariant(), __result, gameModel,metadata,savedGameInfoProvider));
            if (rootSave != null)
            {
                var base128 = rootSave.ToBytes().EncodeToBase128();
                var finalEntry = $"{DataPrefixOwn}{base128}";
                __result.ZoneIndex = MiscEUtil.AddToNew(__result.ZoneIndex,finalEntry);
            }
        }
        catch (Exception e) { LogError(e); }
        
        
        foreach (var expansion in StarlightEntryPoint.ExpansionV01S)
        {
            try
            {
                var rootSave = expansion.OnSaveCustomSaveData(new SavingGameSessionData(saveReferenceTranslation,
                    saveReferenceTranslation.ToNonIVariant(), __result, gameModel,metadata,savedGameInfoProvider)); 
                if (rootSave == null) continue;
                var base128 = rootSave.ToBytes().EncodeToBase128();
                var md5Hash = expansion.GetPackageInfoFromExpansion()?.ID.CreateMD5();
                var finalEntry = $"{DataPrefix}{md5Hash}{base128}";
                __result.ZoneIndex = MiscEUtil.AddToNew(__result.ZoneIndex,finalEntry);
            }
            catch (Exception e)
            {
                LogError($"Failed to save custom save data for expansion {expansion.GetPackageInfoFromExpansion()?.Name}: {e}");
            }
        }
    }
}