using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;
using Starlight.Components;

namespace Starlight.Managers;

// Shark:
// I made it a manager so mods can use it as well
// Example is the BetterBuild free-cam placing :P
public static class StarlightNoClipManager
{
    private static Vector3? startPosition;
    private static bool returnOnDisable;

    /// <summary>
    /// Whether NoClip is currently enabled
    /// </summary>
    public static bool Enabled
    {
        get
        {
            try
            {
                var camera = sceneContext?.Camera;

                return camera != null && camera.GetComponent<NoClipComponent>() != null;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Whether the player will be teleported back after disabling NoClip, if enabled
    /// </summary>
    public static bool ReturnsPlayer => Enabled && returnOnDisable;

    /// <summary>
    /// Enables or Disabled NoClip
    /// </summary>
    /// <param name="on"> Whether to enable or disable it </param>
    /// <param name="returnOnExit">
    /// A "freecam" boolean, the player will be teleported back to where they took off from if true
    /// </param>
    public static bool Set(bool on, bool returnOnExit = false)
    {
        try
        {
            var camera = sceneContext?.Camera;

            if (camera == null)
            {
                LogWarning("No clip: camera is null");
                return false;
            }

            if (on == Enabled) return on;

            if (!on)
            {
                camera.RemoveComponent<NoClipComponent>();
                Land();

                return false;
            }
            
            returnOnDisable = returnOnExit;
            startPosition = returnOnExit ? sceneContext?.player?.transform?.position : null;

            camera.AddComponent<NoClipComponent>();

            return true;
        }
        catch (Exception exception)
        {
            LogWarning($"No clip: could not be set: {exception.Message}");
            return Enabled;
        }
    }

    /// <summary>
    /// Toggles NoClip
    /// </summary>
    /// <inheritdoc cref="Set(bool, bool)" path="/param[@name='returnOnExit']"/>
    public static bool Toggle(bool returnOnExit = false) => Set(!Enabled, returnOnExit);

    /// <summary>
    /// Enables NoClip
    /// </summary>
    /// <inheritdoc cref="Set(bool, bool)" path="/param[@name='returnOnExit']"/>
    public static bool Enable(bool returnOnExit = false) => Set(true, returnOnExit);

    /// <summary>
    /// Disables NoClip
    /// </summary>
    public static bool Disable() => Set(false);

    /// <summary>
    /// Returns the player to the position they started NoClipping from
    /// </summary>
    private static void Land()
    {
        var home = startPosition;
        var returning = returnOnDisable;

        startPosition = null;
        returnOnDisable = false;

        if (!returning || home is not { } position) return;

        try
        {
            var player = sceneContext?.player;

            if (player == null) return;

            player.transform.position = position;

            var controller = player.GetComponent<SRCharacterController>();

            if (controller == null) return;

            controller.Position = position;
            controller.BaseVelocity = Vector3.zero;
        }
        catch (Exception exception)
        {
            LogWarning($"No clip: you could not be returned to your original position: {exception.Message}");
        }
    }
}