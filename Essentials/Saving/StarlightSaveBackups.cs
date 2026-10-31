using System;
using System.IO;
using System.Linq;
using MelonLoader.Utils;

namespace Starlight.Saving;

/// <summary>
/// Save Backup utilities
/// </summary>
public static class StarlightSaveBackups
{
    private const string SaveExtension = ".sav";
    private const string TimeFormat = "yyyy-MM-dd_HH-mm-ss";

    /// <summary>
    /// Config Files
    /// </summary>
    private static readonly string[] ConfigFiles = { "slimerancher.prf", "slimerancher.cfg" };

    /// <summary>
    /// Store folders for the fallback
    /// </summary>
    private static readonly string[] StoreFolders = { "Steam", "Epic", "EpicGames", "Xbox", "Microsoft", "Standalone" };

    private static readonly Dictionary<string, BackupReason> reasons = new(StringComparer.Ordinal);

    private static readonly HashSet<string> SessionBackups = new(StringComparer.Ordinal);

    private static string saveDirectory;
    private static string currentSave;

    /// <summary>
    /// Where backups are stored
    /// </summary>
    public static string BackupRoot => Path.Combine(MelonEnvironment.UserDataDirectory, "Starlight", "SaveBackups");

    /// <summary>
    /// Raised after a backup is written
    /// </summary>
    public static event Action<SaveBackupInfo> BackupTaken;

    /// <summary>
    /// Every registered reason, by id
    /// </summary>
    public static IReadOnlyDictionary<string, BackupReason> Reasons => reasons;

    /// <summary>
    /// Registers a backup reason
    /// </summary>
    /// <remarks> Re-registering the same id replaces it </remarks>
    public static void Register(BackupReason reason)
    {
        if (reason == null || string.IsNullOrWhiteSpace(reason.Id)) return;

        reasons[reason.Id] = reason;
    }
    
    /// <summary>
    /// Forgets the last loaded save
    /// </summary>
    internal static void ResetSession()
    {
        currentSave = null;
        SessionBackups.Clear();
    }

    /// <summary>
    /// Tracks the currently loaded save
    /// </summary>
    internal static void TrackSave(string directory, string fileName)
    {
        saveDirectory = directory;
        currentSave = SaveIdentOf(fileName);
    }

    /// <summary>
    /// Takes a backup for a registered reason
    /// </summary>
    /// <returns> False when nothing was written </returns>
    public static bool Request(string reasonId)
    {
        if (!reasons.TryGetValue(reasonId ?? string.Empty, out var reason))
        {
            LogWarning($"SaveBackups: '{reasonId}' is not a registered reason; nothing was backed up");
            return false;
        }

        if (!TryGetSaveDirectory(out var directory))
        {
            LogWarning("SaveBackups: the save folder could not be located");
            return false;
        }

        var save = currentSave ?? MostRecentSave(directory);
        if (save == null)
        {
            LogWarning("SaveBackups: there are no saves to back up");
            return false;
        }

        return Backup(directory, save, reason);
    }

    /// <summary>
    /// Takes a backup of a specific save
    /// </summary>
    /// <param name="saveFileName"> Any file name of the save </param>
    public static bool Request(string reasonId, string saveFileName)
        => RequestSave(reasonId, string.IsNullOrWhiteSpace(saveFileName) ? null : SaveIdentOf(saveFileName));

    /// <summary>
    /// Takes a backup of a save by its identity
    /// </summary>
    public static bool RequestSave(string reasonId, string saveIdent)
    {
        if (!reasons.TryGetValue(reasonId ?? string.Empty, out var reason))
        {
            LogWarning($"[SaveBackups] '{reasonId}' is not a registered reason; nothing was backed up");
            return false;
        }

        if (string.IsNullOrWhiteSpace(saveIdent) || !TryGetSaveDirectory(out var directory))
        {
            LogWarning("[SaveBackups] the save folder or save name could not be determined");
            return false;
        }

        return Backup(directory, saveIdent, reason);
    }

    /// <summary>
    /// Finds the save folder
    /// </summary>
    public static bool TryGetSaveDirectory(out string path)
    {
        try
        {
            var provider = gameContext?.AutoSaveDirector?._storageProvider?.TryCast<FileStorageProvider>();
            var savePath = provider?.savePath;

            if (!string.IsNullOrWhiteSpace(savePath) && Directory.Exists(savePath))
            {
                saveDirectory = savePath;
                path = savePath;
                return true;
            }
        }
        catch (Exception exception)
        {
            Log($"SaveBackups: the storage provider could not be read: {exception.Message}");
        }

        saveDirectory ??= SearchSaveDirectory();
        path = saveDirectory;
        return path != null;
    }

    /// <summary>
    /// Every backup on disk, newest first
    /// </summary>
    public static IReadOnlyList<SaveBackupInfo> ListBackups(string saveIdent = null)
    {
        var found = new List<SaveBackupInfo>();
        if (!Directory.Exists(BackupRoot)) return found;

        foreach (var stampFolder in Directory.EnumerateDirectories(BackupRoot)
                     .OrderByDescending(Path.GetFileName, StringComparer.Ordinal))
        {
            var taken = ParseStamp(Path.GetFileName(stampFolder));

            foreach (var saveFolder in Directory.EnumerateDirectories(stampFolder))
            {
                var save = Path.GetFileName(saveFolder);
                if (saveIdent != null && !string.Equals(save, saveIdent, StringComparison.OrdinalIgnoreCase)) continue;

                foreach (var reasonFolder in Directory.EnumerateDirectories(saveFolder))
                {
                    var files = Directory.GetFiles(reasonFolder, "*", SearchOption.AllDirectories).Length;
                    found.Add(new SaveBackupInfo(save, Path.GetFileName(reasonFolder), taken, reasonFolder, files));
                }
            }
        }

        return found;
    }

    /// <summary>
    /// Restores a backup
    /// </summary>
    public static bool Restore(SaveBackupInfo backup)
    {
        if (backup == null || !Directory.Exists(backup.Folder)) return false;

        if (!TryGetSaveDirectory(out var directory))
        {
            LogWarning("SaveBackups: the save folder could not be located; nothing was restored");
            return false;
        }

        Register(new BackupReason("starlight.before_restore", "starlight", Keep: 5, Forced: true));
        Request("starlight.before_restore");

        try
        {
            var source = new DirectoryInfo(backup.Folder);
            var copied = 0;

            foreach (var file in source.GetFiles("*", SearchOption.AllDirectories))
            {
                File.Copy(file.FullName, Path.Combine(directory, file.Name), true);
                copied++;
            }

            Log($"SaveBackups: restored {copied} file(s) from {backup.Folder} into {directory}");
            return copied > 0;
        }
        catch (Exception exception)
        {
            LogError($"SaveBackups: the restore failed: {exception}");
            return false;
        }
    }

    /// <summary>
    /// Writes one backup
    /// </summary>
    private static bool Backup(string directory, string save, BackupReason reason)
    {
        if (!reason.Forced && !SessionBackups.Add($"{save}|{reason.Id}")) return false;

        try
        {
            var autosaves = Autosaves(directory, save);
            if (autosaves.Count == 0)
            {
                Log($"SaveBackups: no autosaves of {save} in {directory}.");
                return false;
            }

            var folder = Sanitise(save);

            if (!reason.Forced && reason.Periodic && IsUnchanged(folder, autosaves))
            {
                Log($"SaveBackups: {save} is unchanged since the last backup");
                return false;
            }

            var backupFolder = NewBackupFolder(folder);
            string target;

            try
            {
                target = Directory.CreateDirectory(
                    Path.Combine(backupFolder, Sanitise(reason.Id), RestorePath(directory))).FullName;

                foreach (var file in autosaves.Concat(Companions(directory)))
                    File.Copy(file.FullName, Path.Combine(target, file.Name), true);
            }
            catch
            {
                Discard(backupFolder);
                throw;
            }

            Prune(folder, reason);

            var info = new SaveBackupInfo(save, reason.Id, DateTime.UtcNow, target, autosaves.Count);
            Log($"SaveBackups: backed up {autosaves.Count} autosave(s) of {save} ({reason.Id})");

            try { BackupTaken?.Invoke(info); }
            catch (Exception exception) { LogWarning($"[SaveBackups] a listener threw: {exception.Message}"); }

            return true;
        }
        catch (Exception exception)
        {
            LogError($"SaveBackups: backup failed: {exception}");
            return false;
        }
    }

    /// <summary>
    /// Save identity; the file name with its autosave index stripped
    /// </summary>
    private static string SaveIdentOf(string fileName)
    {
        var name = fileName.EndsWith(SaveExtension, StringComparison.OrdinalIgnoreCase)
            ? fileName.Substring(0, fileName.Length - SaveExtension.Length)
            : fileName;

        var separator = name.LastIndexOf('_');
        return separator > 0 ? name.Substring(0, separator) : name;
    }

    private static string MostRecentSave(string directory)
        => new DirectoryInfo(directory).EnumerateFiles('*' + SaveExtension)
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Select(file => SaveIdentOf(file.Name))
            .FirstOrDefault();

    /// <summary>
    /// Fallback to find the save directory
    /// </summary>
    private static string SearchSaveDirectory()
    {
        var root = Application.persistentDataPath;
        if (!Directory.Exists(root)) return null;

        return StoreFolders.Select(store => Path.Combine(root, store))
            .Where(Directory.Exists)
            .Concat(Directory.EnumerateDirectories(root))
            .SelectMany(store => Directory.EnumerateDirectories(store).Prepend(store))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Prepend(root)
            .Select(candidate => (candidate, written: NewestAutosaveWrite(candidate)))
            .Where(entry => entry.written > DateTime.MinValue)
            .OrderByDescending(entry => entry.written)
            .Select(entry => entry.candidate)
            .FirstOrDefault();
    }

    private static DateTime NewestAutosaveWrite(string directory)
    {
        try
        {
            return new DirectoryInfo(directory).EnumerateFiles('*' + SaveExtension)
                .Select(file => file.LastWriteTimeUtc)
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();
        }
        catch (Exception)
        {
            return DateTime.MinValue;
        }
    }

    private static List<FileInfo> Autosaves(string directory, string save)
    {
        var prefix = save + '_';

        return new DirectoryInfo(directory).EnumerateFiles(prefix + '*' + SaveExtension)
            .Where(file => IsAutosave(file.Name, prefix))
            .OrderBy(file => file.Name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// True for <c>&lt;ident&gt;_&lt;digits&gt;.sav</c>.
    /// </summary>
    private static bool IsAutosave(string fileName, string prefix)
    {
        if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !fileName.EndsWith(SaveExtension, StringComparison.OrdinalIgnoreCase))
            return false;

        var index = fileName.Substring(prefix.Length, fileName.Length - prefix.Length - SaveExtension.Length);
        return index.Length > 0 && index.All(character => character is >= '0' and <= '9');
    }

    private static IEnumerable<FileInfo> Companions(string directory)
        => ConfigFiles.Select(name => new FileInfo(Path.Combine(directory, name))).Where(file => file.Exists);

    /// <summary>
    /// Preserves the save folder's <c>store/user</c> path
    /// </summary>
    private static string RestorePath(string directory)
    {
        var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        var user = Path.GetFileName(full);

        if (string.IsNullOrEmpty(user)) return "Saves";

        var store = Path.GetFileName(Path.GetDirectoryName(full) ?? string.Empty);
        return string.IsNullOrEmpty(store) ? user : Path.Combine(store, user);
    }

    /// <summary>
    /// True when every autosave is unchanged to the last backup
    /// </summary>
    private static bool IsUnchanged(string save, List<FileInfo> autosaves)
    {
        var newest = BackupFolders(save).FirstOrDefault();
        if (newest == null) return false;

        var backed = new DirectoryInfo(newest).GetFiles("*", SearchOption.AllDirectories)
            .GroupBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return autosaves.All(autosave => backed.TryGetValue(autosave.Name, out var copy)
                                         && copy.Length == autosave.Length
                                         && copy.LastWriteTimeUtc == autosave.LastWriteTimeUtc);
    }

    private static string NewBackupFolder(string save)
    {
        var stamp = DateTime.Now.ToString(TimeFormat);
        var folder = Path.Combine(BackupRoot, stamp, save);
        var attempt = 1;

        while (Directory.Exists(folder))
            folder = Path.Combine(BackupRoot, $"{stamp}_{attempt++}", save);

        Directory.CreateDirectory(folder);
        return folder;
    }

    /// <summary>
    /// Trims this reason's backups to its <c>Keep</c> amount
    /// </summary>
    private static void Prune(string save, BackupReason reason)
    {
        if (reason.Keep <= 0) return;

        var outdated = BackupFolders(save)
            .Where(folder => string.Equals(ReasonOf(folder), Sanitise(reason.Id), StringComparison.OrdinalIgnoreCase))
            .Skip(reason.Keep)
            .ToArray();

        foreach (var folder in outdated)
            Discard(folder);
    }

    /// <summary>
    /// Removes one backup
    /// </summary>
    private static void Discard(string folder)
    {
        try
        {
            Directory.Delete(folder, true);
        }
        catch (Exception exception)
        {
            LogWarning($"SaveBackups: could not delete {folder}: {exception.Message}");
            return;
        }

        DeleteIfEmpty(Path.GetDirectoryName(folder));
    }

    private static void DeleteIfEmpty(string folder)
    {
        try
        {
            if (folder != null && Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
                Directory.Delete(folder);
        }
        catch (Exception exception)
        {
            Log($"SaveBackups: could not remove {folder}: {exception.Message}");
        }
    }

    /// <summary>
    /// Every backup folder for one save, newest first
    /// </summary>
    private static IEnumerable<string> BackupFolders(string save)
        => (Directory.Exists(BackupRoot)
                ? Directory.EnumerateDirectories(BackupRoot).OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
                : Enumerable.Empty<string>())
            .Select(stamp => Path.Combine(stamp, save))
            .Where(Directory.Exists);

    private static string ReasonOf(string saveBackup)
        => Directory.EnumerateDirectories(saveBackup).Select(Path.GetFileName).FirstOrDefault() ?? string.Empty;

    private static DateTime ParseStamp(string folderName)
        => DateTime.TryParseExact(folderName, TimeFormat, null, System.Globalization.DateTimeStyles.None, out var parsed)
            ? parsed.ToUniversalTime()
            : DateTime.MinValue;

    private static string Sanitise(string name)
        => string.Join('_', name.Split(Path.GetInvalidFileNameChars()));
}