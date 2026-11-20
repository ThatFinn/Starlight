using System;
using System.Linq;
using UObject = UnityEngine.Object;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Event.Query;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Starlight.Prism.Lib;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

namespace Starlight.Prism.Creators;

/// <summary>
/// A request to create a music track
/// </summary>
public record MusicRequest
{
    /// <summary>
    /// The music track's name
    /// </summary>
    /// <remarks> Identifier </remarks>
    public required string Name { get; init; }

    /// <summary>
    /// The title of the music track
    /// </summary>
    /// <remarks> Display name </remarks>
    public required LocalizedString Title { get; init; }

    /// <summary>
    /// The track to copy playback settings from
    /// </summary>
    /// <remarks> Null = any template </remarks>
    public MusicPlayerMusicDefinition? Template { get; init; }

    /// <summary>
    /// The music track's icon
    /// </summary>
    /// <remarks> Null = the standard icon </remarks>
    public Sprite? Icon { get; init; }

    /// <summary>
    /// The sound file to play (wav, mp3, ogg or aiff)
    /// </summary>
    /// <example>
    /// <code>
    /// // Slime Rancher 2/UserData/com.author.modname/music/audio.wav
    /// AudioFile = Path.Combine(modDataPath, "music", "audio.wav")
    /// </code>
    /// </example>
    public string? AudioFile { get; init; }

    /// <summary>
    /// The cue to play
    /// </summary>
    public AssetReferenceT<SECTR_AudioCue>? Cue { get; init; }

    /// <summary>
    /// What decides whether the track is available or not
    /// </summary>
    public CompositeQueryComponent? AvailableCondition { get; init; }
}

/// <summary>
/// A music track created by Prism
/// </summary>
public record CreatedMusic(MusicPlayerMusicDefinition Track, string ReferenceId, string ObjectName);

/// <summary>
/// The music creator, used to add music tracks to the game
/// </summary>
/// <remarks>
/// If you wanna add audio without it being labeled as music,
/// see <see cref="AudioFileEUtil"/> or <see cref="AudioEUtil"/>
/// </remarks>
public static class PrismMusicCreatorV02
{
    private const string Kind = "Music";

    private static readonly Dictionary<string, MusicPlayerMusicDefinition> Created = new(StringComparer.Ordinal);

    private static readonly Dictionary<string, string> LegacyGuids = new(StringComparer.Ordinal);

    /// <summary>
    /// The files corresponding to their reference id
    /// </summary>
    private static readonly Dictionary<string, AudioFile> Files = new(StringComparer.Ordinal);

    private sealed class AudioFile
    {
        public required string Path { get; init; }
        public required AudioFileLoad Loading { get; init; }
        public AudioClip? Clip { get; set; }
        public SECTR_AudioCue? Cue { get; set; }
    }

    /// <summary>
    /// The base game music tracks
    /// </summary>
    public static IEnumerable<MusicPlayerMusicDefinition> Native =>
        TracksIn(lookupDirector._configuration?.AllMusicTracks, new HashSet<IntPtr>())
            .Where(track => !Created.ContainsValue(track));

    private static IEnumerable<MusicPlayerMusicDefinition> TracksIn(
        Il2CppMonomiPark.SlimeRancher.Audio.Music.MusicPlayerMusicDefinitionList? list, HashSet<IntPtr> visited)
    {
        if (list == null || !visited.Add(list.Pointer)) yield break;

        foreach (var track in list._items)
            if (track != null) yield return track;

        foreach (var subList in list._subLists)
        foreach (var track in TracksIn(subList, visited))
            yield return track;
    }

    /// <summary>
    /// Creates a music track out of a <see cref="MusicRequest"/>
    /// </summary>
    public static CreatedMusic? Create(MusicRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsGameReady)
        {
            LogError($"Prism: Can't create music track '{request.Name}': the game isn't ready");
            return null;
        }

        var template = request.Template ?? Native.FirstOrDefault() ?? GetAny<MusicPlayerMusicDefinition>();
        if (template == null)
        {
            LogError($"Prism: Can't create music track '{request.Name}': the game has no music track to copy");
            return null;
        }

        var stripped = ContentNames.WithoutKindSuffix(request.Name, Kind);
        var objectName = ContentNames.Unique(stripped, Kind) + Kind;
        var referenceId = ContentNames.ReferenceId(
            nameof(MusicPlayerMusicDefinition), ContentNames.StableName(request.Name, Kind), Kind);

        if (Created.ContainsKey(referenceId))
        {
            LogError($"Prism: Can't create music track '{objectName}' as '{referenceId}': another music track already has that id");
            return null;
        }

        var track = UObject.Instantiate(template);
        track.hideFlags = HideFlags.DontUnloadUnusedAsset;
        track.name = objectName;
        
        track.Guid = referenceId;
        LegacyGuids[referenceId] = ContentNames.LegacyReferenceId(
            nameof(MusicPlayerMusicDefinition), ContentNames.StableName(request.Name, Kind), Kind);

        track._trackTitle = request.Title;
        if (request.Icon != null) track._trackIcon = SpriteEUtil.Hold(request.Icon);
        if (request.Cue != null) track._cue = request.Cue;
        if (request.AvailableCondition != null) track._availableCondition = request.AvailableCondition;

        if (!string.IsNullOrWhiteSpace(request.AudioFile))
        {
            if (!File.Exists(request.AudioFile))
                LogWarning($"Prism: Music track '{objectName}': the sound file '{request.AudioFile}' wasn't found. It will play its template's music instead");
            else
            {
                var loading = AudioFileEUtil.Load(request.AudioFile);
                
                if (loading.Error != null)
                    LogWarning($"Prism: Music track '{objectName}': the sound file '{request.AudioFile}' can't be read ({loading.Error}). It will play its template's music instead");
                else
                    Files[referenceId] = new AudioFile { Path = request.AudioFile, Loading = loading };
            }
        }

        Created[referenceId] = track;
        Register(track);

        return new CreatedMusic(track, referenceId, objectName);
    }

    /// <summary>
    /// The cue to play for a track made from a sound file
    /// </summary>
    internal static SECTR_AudioCue? CueFor(MusicPlayerMusicDefinition? track, SECTR_AudioCue? loaded)
    {
        if (track == null || loaded == null || !Files.TryGetValue(track.Guid, out var file)) return null;
        if (file.Cue != null) return file.Cue;

        try
        {
            if (!file.Loading.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Timed out while reading the file.");

            file.Clip ??= file.Loading.Clip;
            if (file.Clip == null) throw new InvalidOperationException(file.Loading.Error ?? "The file has no sound in it.");

            var cue = UObject.Instantiate(loaded);
            cue.hideFlags = HideFlags.DontUnloadUnusedAsset;
            cue.name = track.name;
            cue.AudioClips.Clear();
            cue.AudioClips.Add(new SECTR_AudioCue.ClipData(file.Clip));

            file.Cue = cue;
            return cue;
        }
        catch (Exception ex)
        {
            Files.Remove(track.Guid);
            LogError($"Prism: Couldn't play '{file.Path}' for music track '{track.name}'. It plays its template's music instead", ex);
            return null;
        }
    }

    /// <summary>
    /// Re-registers all music tracks
    /// </summary>
    internal static void Refresh()
    {
        foreach (var track in Created.Values.ToList())
            Register(track);
    }

    /// <summary>
    /// Registers a music track
    /// </summary>
    private static void Register(MusicPlayerMusicDefinition track)
    {
        try
        {
            var list = lookupDirector._configuration?.AllMusicTracks;
            if (list != null && !list._items.Contains(track))
                list._items.Add(track);

            SaveTables.Register(SaveTables.Translation?._musicPlayerTranslation, track.ReferenceId, track);

            if (LegacyGuids.TryGetValue(track.ReferenceId, out var legacyGuid))
                SaveTables.RegisterLegacy(SaveTables.Translation?._musicPlayerTranslation, legacyGuid, track);

            AddToPlayer(track);
        }
        catch (Exception ex)
        {
            LogError($"Prism: Failed to register track '{track.name}'", ex);
        }
    }

    /// <summary>
    /// Adds a music track to the Slimepedia's music player
    /// </summary>
    private static void AddToPlayer(MusicPlayerMusicDefinition track)
    {
        foreach (var category in GetAll<MusicPediaCategory>())
        {
            var runtime = category.GetTypedRuntimeCategory()?.TryCast<MusicPediaRuntimeCategory>();
            runtime?.AddDynamicItem(track);
        }
    }
}