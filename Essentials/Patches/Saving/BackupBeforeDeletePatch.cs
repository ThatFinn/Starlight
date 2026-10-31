using Il2CppMonomiPark.SlimeRancher;
using Starlight.Saving;

namespace Starlight.Patches.Saving;

/// <summary>
/// Backs a save up before it is deleted
/// </summary>
internal static class BackupBeforeDeletePatch
{
    private const string ReasonId = "starlight.deleted";
    
    private static int _suppressed;

    private static bool _registered;

    private static void Enter() => _suppressed++;
    private static void Exit() { if (_suppressed > 0) _suppressed--; }

    private static void Backup(string fileName)
    {
        if (_suppressed > 0 || string.IsNullOrWhiteSpace(fileName)) return;

        if (!_registered)
        {
            StarlightSaveBackups.Register(new BackupReason(ReasonId, "starlight", Keep: 1000, Forced: true));
            _registered = true;
        }

        try { StarlightSaveBackups.Request(ReasonId, fileName); }
        catch (Exception e) { LogError(e); }
    }

    private static void BackupGame(string gameName)
    {
        if (_suppressed > 0 || string.IsNullOrWhiteSpace(gameName)) return;
        
        Backup(gameName + "_0");
    }

    /// <summary>
    /// Tracks the save that was just entered
    /// </summary>
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.Load))]
    internal static class TrackLoadedSavePatch
    {
        internal static void Prefix(GameSaveIdentifier identifier)
        {
            try
            {
                StarlightSaveBackups.ResetSession();

                var saveName = string.IsNullOrWhiteSpace(identifier.SaveName)
                    ? identifier.GameName + "_0"
                    : identifier.SaveName;

                if (string.IsNullOrWhiteSpace(saveName) || !StarlightSaveBackups.TryGetSaveDirectory(out var directory)) return;

                StarlightSaveBackups.TrackSave(directory, saveName);
            }
            catch (Exception e) { LogError(e); }
        }
    }

    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.DeleteGame))]
    internal static class DeleteGamePatch
    {
        internal static void Prefix(string gameName)
        {
            BackupGame(gameName);
            Enter();
        }

        [HarmonyFinalizer]
        internal static Exception Finalizer(Exception __exception)
        {
            Exit();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(AutoSaveDirector), "CleanupAutosaves")]
    internal static class CleanupAutosavesPatch
    {
        internal static void Prefix() => Enter();

        [HarmonyFinalizer]
        internal static Exception Finalizer(Exception __exception)
        {
            Exit();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(FileStorageProvider), nameof(FileStorageProvider.DeleteGameData))]
    internal static class DeleteGameDataPatch
    {
        internal static void Prefix(string fileName) => Backup(fileName);
    }

    [HarmonyPatch(typeof(FileStorageProvider), nameof(FileStorageProvider.DeleteGamesData))]
    internal static class DeleteGamesDataPatch
    {
        internal static void Prefix(Il2CppSystem.Collections.Generic.List<string> fileNames)
        {
            if (fileNames == null) return;

            var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var fileName in fileNames)
                if (seen.Add(fileName))
                    Backup(fileName);
        }
    }
}