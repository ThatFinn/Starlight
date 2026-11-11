using Il2CppMonomiPark.SlimeRancher.Audio.Music;
using Starlight.Prism.Creators;
using Starlight.Storage;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Starlight.Prism.Patches;

/// <summary>
/// Plays a created track's sound file
/// </summary>
[PrismPatch]
[HarmonyPatch(typeof(PlayerSelectedMusicInstance), nameof(PlayerSelectedMusicInstance.HandlePlayTrackOnLoad))]
internal static class MusicFilePatch
{
    public static void Prefix(PlayerSelectedMusicInstance __instance, ref AsyncOperationHandle<SECTR_AudioCue> handle)
    {
        try
        {
            var track = __instance?._bufferedLoader?.CurrentTag;
            if (track == null) return;

            var cue = PrismMusicCreatorV02.CueFor(track, handle.Result);
            if (cue == null) return;

            handle = Addressables.ResourceManager.CreateCompletedOperation(cue, string.Empty);
        }
        catch (Exception ex)
        {
            LogError("Prism: Failed to add a track's sound file in the Music Player", ex);
        }
    }
}
