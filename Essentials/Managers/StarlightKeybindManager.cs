using System;
using System.Linq;
using Starlight.Enums;
using Starlight.Storage;
using Starlight.UI;

namespace Starlight.Managers;

// Shark:
// A manager so mods can easily register their own keybinds
// Example: Multiplayer Push To Talk :P
public static class StarlightKeybindManager
{
    private static readonly List<StarlightKeybind> Registered = new();

    /// <summary>
    /// Every keybind, in the order it was registered in
    /// </summary>
    public static List<StarlightKeybind> All() => new(Registered);

    /// <summary>
    /// Registers an action to a keybind
    /// </summary>
    /// <returns>
    /// NoError, DoesntExist (no id or no action), or AlreadyExists
    /// </returns>
    public static StarlightError Register(StarlightKeybind keybind)
    {
        if (keybind is null || string.IsNullOrWhiteSpace(keybind.Id) || keybind.Action is null)
            return StarlightError.DoesntExist;

        if (Registered.Any(existing => string.Equals(existing.Id, keybind.Id, StringComparison.Ordinal)))
            return StarlightError.AlreadyExists;

        Registered.Add(keybind);

        return StarlightError.NoError;
    }

    /// <summary>
    /// Registers an action to a keybind
    /// </summary>
    public static StarlightError Register(string id, string name, LKey defaultKey, Action action,
        string owner = "", bool inGameOnly = true)
        => Register(new StarlightKeybind
        {
            Id         = id,
            Name       = name,
            Default    = defaultKey,
            Action     = action,
            Owner      = owner,
            InGameOnly = inGameOnly
        });

    /// <summary>
    /// Unregisters an action from a keybind
    /// </summary>
    public static StarlightError Unregister(string id)
    {
        var removed = Registered.RemoveAll(existing => string.Equals(existing.Id, id, StringComparison.Ordinal));

        return removed > 0 ? StarlightError.NoError : StarlightError.DoesntExist;
    }

    /// <summary>
    /// The action with the specified id, or null
    /// </summary>
    public static StarlightKeybind Find(string id) =>
        Registered.FirstOrDefault(existing => string.Equals(existing.Id, id, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The active <see cref="LKey"/> an action is bound to
    /// </summary>
    public static LKey KeyOf(StarlightKeybind keybind)
    {
        if (keybind is null) return LKey.None;

        var saved = StarlightSaveManager.data?.modKeys;

        return saved is not null && saved.TryGetValue(keybind.Id, out var key) ? key : keybind.Default;
    }

    /// <summary>
    /// Rebinds an action to a different key
    /// </summary>
    public static StarlightError Rebind(string id, LKey key)
    {
        var keybind = Find(id);

        if (keybind is null) return StarlightError.DoesntExist;

        StarlightSaveManager.data.modKeys ??= new Dictionary<string, LKey>();
        StarlightSaveManager.data.modKeys[keybind.Id] = key;
        StarlightSaveManager.Save();
        
        var clash = Registered.FirstOrDefault(other =>
            !ReferenceEquals(other, keybind) && key != LKey.None && KeyOf(other) == key);

        if (clash != null)
            LogWarning($"'{keybind.Name}' shares key '{key}' with '{clash.Name}', both will be triggered.");

        return StarlightError.NoError;
    }

    /// <summary>
    /// Resets an action to the default keybind
    /// </summary>
    public static StarlightError Reset(string id)
    {
        var keybind = Find(id);

        if (keybind is null) return StarlightError.DoesntExist;

        StarlightSaveManager.data?.modKeys?.Remove(keybind.Id);
        StarlightSaveManager.Save();

        return StarlightError.NoError;
    }

    /// <summary>
    /// The action waiting for a key to be pressed to bound to, or null
    /// </summary>
    public static StarlightKeybind Capturing { get; private set; }

    /// <summary>
    /// Waits for the next pressed key and binds it
    /// </summary>
    public static StarlightError BeginCapture(string id)
    {
        var keybind = Find(id);

        if (keybind is null) return StarlightError.DoesntExist;

        Capturing = keybind;

        return StarlightError.NoError;
    }

    /// <summary>
    /// Cancels the rebinding process
    /// </summary>
    public static void CancelCapture() => Capturing = null;
    
    internal static void Update()
    {
        if (ImmediatePanel.Typing || IsTypingNatively()) return;

        if (Capturing is not null)
        {
            Capture();
            return;
        }

        for (var i = 0; i < Registered.Count; i++)
        {
            var keybind = Registered[i];
            var key = KeyOf(keybind);

            if (key == LKey.None) continue;
            if (keybind.InGameOnly && !inGame) continue;
            if (!key.OnKeyDown()) continue;

            try
            {
                keybind.Action();
            }
            catch (Exception exception)
            {
                LogError($"'{keybind.Name}' threw: {exception.Message}");
            }
        }
    }

    /// <summary>
    /// Whether a text field in the game's own UI currently has active keyboard control
    /// </summary>
    private static bool IsTypingNatively()
    {
        try
        {
            var selected = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject;

            if (selected == null) return false;

            var field = selected.GetComponent<Il2CppTMPro.TMP_InputField>();

            if (field != null) return field.isFocused;

            var legacy = selected.GetComponent<UnityEngine.UI.InputField>();

            return legacy != null && legacy.isFocused;
        }
        catch
        {
            return false;
        }
    }

    private static void Capture()
    {
        foreach (LKey key in Enum.GetValues(typeof(LKey)))
        {
            if (key == LKey.None || !key.OnKeyDown()) continue;

            var keybind = Capturing;

            Capturing = null;

            if (key == LKey.Escape)
            {
                Log($"'{keybind.Name}' left on {KeyOf(keybind)}.");
                return;
            }

            Rebind(keybind.Id, key);
            Log($"'{keybind.Name}' is now bound to {key}.");

            return;
        }
    }
}