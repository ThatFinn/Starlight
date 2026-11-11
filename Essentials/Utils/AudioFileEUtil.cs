using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Il2CppInterop.Runtime;
using UnityEngine.Bindings;
using UnityEngine.Networking;

namespace Starlight.Utils;

/// <summary>
/// A sound file the engine is reading
/// </summary>
public sealed class AudioFileLoad
{
    private readonly UnityWebRequest? _request;
    private readonly DownloadHandler? _handler;

    private AudioClip? _clip;
    private bool _finished;

    internal AudioFileLoad(string path, UnityWebRequest request, DownloadHandler handler)
    {
        Path = path;
        _request = request;
        _handler = handler;
    }

    internal AudioFileLoad(string path, string error)
    {
        Path = path;
        Error = error;
        _finished = true;
    }

    /// <summary>
    /// The file that was asked for
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Why the file couldn't be read
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Whether the engine is through with the file
    /// </summary>
    public bool IsDone => _finished || _request == null || _request.isDone;

    /// <summary>
    /// The file's audio clip
    /// </summary>
    public AudioClip? Clip
    {
        get
        {
            if (!_finished && IsDone) Finish();

            return _clip;
        }
    }

    /// <summary>
    /// Holds the main thread until the file is read or the time is up
    /// </summary>
    /// <returns> Whether the load is done </returns>
    public bool Wait(TimeSpan limit)
    {
        var watch = Stopwatch.StartNew();

        while (!IsDone && watch.Elapsed < limit)
            Thread.Sleep(1);

        return IsDone;
    }

    private void Finish()
    {
        _finished = true;

        try
        {
            if (_request!.result != UnityWebRequest.Result.Success)
            {
                Error = string.IsNullOrWhiteSpace(_request.error) ? "The file could not be read" : _request.error;
                return;
            }

            _clip = AudioFileEUtil.ClipOf(_handler!);
            if (_clip == null)
            {
                Error = "The engine can't play this kind of file. It reads wav, mp3, ogg and aiff";
                return;
            }
            
            _clip.name = System.IO.Path.GetFileNameWithoutExtension(Path)!;
            _clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}

/// <summary>
/// Reads sound files from disk
/// </summary>
public static class AudioFileEUtil
{
    /// <summary>
    /// Keeps mp3 and ogg compressed in memory and decodes them while they play
    /// </summary>
    /// <remarks>
    /// If enabled, a song costs the size of its file and not ten times that
    /// </remarks>
    public static bool KeepCompressed { get; set; } = true;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSpan
    {
        public IntPtr Begin;
        public int Length;
    }

    private delegate IntPtr CreateCall(IntPtr managed, ref NativeSpan url, AudioType type);
    private delegate IntPtr ClipCall(IntPtr handler);
    private delegate void FlagCall(IntPtr handler, [MarshalAs(UnmanagedType.U1)] bool value);

    private static CreateCall? create;
    private static ClipCall? clipOf;
    private static FlagCall? setCompressed;
    private static bool? available;

    /// <summary>
    /// Whether this build of the game still has the engine's audio file loader
    /// </summary>
    public static bool Available
    {
        get
        {
            if (available.HasValue) return available.Value;

            try
            {
                create = Resolve<CreateCall>("Create_Injected");
                clipOf = Resolve<ClipCall>("get_audioClip_Injected");
                setCompressed = Resolve<FlagCall>("set_compressed_Injected");

                available = create != null && clipOf != null;
            }
            catch (Exception)
            {
                available = false;
            }

            return available.Value;
        }
    }

    /// <summary>
    /// Starts reading a sound file
    /// </summary>
    public static AudioFileLoad Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return new AudioFileLoad(path, $"No sound file at '{path}'.");

        if (!Available)
            return new AudioFileLoad(path, "This version of the game has no audio file loader.");

        try
        {
            var url = new Uri(Path.GetFullPath(path)).AbsoluteUri;

            var handler = new DownloadHandler();
            var pinned = GCHandle.Alloc(url, GCHandleType.Pinned);

            try
            {
                var span = new NativeSpan { Begin = pinned.AddrOfPinnedObject(), Length = url.Length };
                handler.m_Ptr = create!(handler.Pointer, ref span, TypeOf(path));
            }
            finally
            {
                pinned.Free();
            }

            if (handler.m_Ptr == IntPtr.Zero)
                return new AudioFileLoad(path, "The engine could not open the file.");

            if (KeepCompressed) setCompressed?.Invoke(handler.m_Ptr, true);

            var request = new UnityWebRequest(url, "GET", handler, null);
            request.SendWebRequest();

            return new AudioFileLoad(path, request, handler);
        }
        catch (Exception ex)
        {
            return new AudioFileLoad(path, ex.Message);
        }
    }

    /// <summary>
    /// The clip of a finished load
    /// </summary>
    internal static AudioClip? ClipOf(DownloadHandler handler)
    {
        var managed = clipOf!(handler.m_Ptr);

        return managed == IntPtr.Zero ? null : Unmarshal.UnmarshalUnityObject<AudioClip>(managed);
    }

    private static T? Resolve<T>(string method) where T : Delegate
    {
        var address = IL2CPP.il2cpp_resolve_icall("UnityEngine.Networking.DownloadHandlerAudioClip::" + method);

        return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer<T>(address);
    }

    /// <summary>
    /// The engine guesses the kind of unknown file from its content
    /// </summary>
    private static AudioType TypeOf(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".wav"            => AudioType.WAV,
        ".mp3"            => AudioType.MPEG,
        ".ogg"            => AudioType.OGGVORBIS,
        ".aif" or ".aiff" => AudioType.AIFF,
        ".xm"             => AudioType.XM,
        ".it"             => AudioType.IT,
        ".mod"            => AudioType.MOD,
        ".s3m"            => AudioType.S3M,
        _                 => AudioType.UNKNOWN,
    };
}