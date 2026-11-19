using Il2CppKinematicCharacterController;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;
using Starlight.Commands;
using Starlight.Enums;
using Starlight.Storage;
using UnityEngine.InputSystem;

namespace Starlight.Components;

[InjectIntoIL]
internal class NoClipComponent : MonoBehaviour
{
    private float baseSpeed = 15f;
    private static float speedAdjust => StarlightEntryPoint.noclipAdjustSpeed;
    private float speed
    {
        get
        {
            if (isSprint)
                return baseSpeed * StarlightEntryPoint.noclipSpeedMultiplier;
            return baseSpeed;
        }
    }
    bool isSprint => LKey.LeftShift.OnKey() || (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);
    
    // If your stick drift is stronger than this, I am sorry for you :P
    private const float StickDeadzone = 0.15f;
    internal static Transform player => sceneContext.player.transform;
    internal static KCCSettings playerSettings => KinematicCharacterSystem.Settings;
    internal static SRCharacterController playerController => player.GetComponent<SRCharacterController>();
    internal static KinematicCharacterMotor playerMotor => player.GetComponent<KinematicCharacterMotor>();

    private void OnDestroy()
    {
        try
        {
            playerController.BaseVelocity = Vector3.zero;
            playerMotor.enabled = true;
            playerSettings.AutoSimulation = true;
            playerController.Position = player.position;
            playerMotor.Capsule.enabled = true;
            playerMotor.SetCapsuleCollisionsActivation(true);
        } catch { }
    }

    private void Awake()
    {
        playerController.ResetVelocity(false);
        playerController.BaseVelocity = Vector3.zero;

        playerMotor.enabled = false;
        playerSettings.AutoSimulation = false;
        playerMotor.SetCapsuleCollisionsActivation(false);
        playerMotor.Capsule.enabled = false;
    }

    private void Update()
    {
        var step = speed * Time.deltaTime;
        var move = Vector3.zero;

        if(NoClipCommand.horizontal!=null)
        {
            float horizontal = NoClipCommand.horizontal.ReadValue<float>();
            float vertical = NoClipCommand.vertical.ReadValue<float>();
            if(horizontal>0.01f||horizontal<-0.01f)
                move += transform.right * (horizontal*step);
            if(vertical>0.01f||vertical<-0.01f)
                move += transform.forward * (vertical*step);
        }
        else
        {
            if (LKey.A.OnKey() || LKey.LeftArrow.OnKey())
                move += -transform.right * step;
            if (LKey.D.OnKey() || LKey.RightArrow.OnKey())
                move += transform.right * step;
            if (LKey.W.OnKey() || LKey.UpArrow.OnKey())
                move += transform.forward * step;
            if (LKey.S.OnKey() || LKey.DownArrow.OnKey())
                move += -transform.forward * step;
        }
        
        if (LKey.Space.OnKey()) move += Vector3.up * step;
        if (LKey.LeftControl.OnKey() || LKey.C.OnKey()) move += Vector3.down * step;

        var pad = Gamepad.current;

        if (pad != null)
        {
            var stick = pad.leftStick.ReadValue();

            if (stick.sqrMagnitude > StickDeadzone * StickDeadzone)
            {
                move += transform.right * (stick.x * step);
                move += transform.forward * (stick.y * step);
            }
            
            move += Vector3.up * (pad.rightTrigger.ReadValue() * step);
            move += Vector3.down * (pad.leftTrigger.ReadValue() * step);
        }

        if (move != Vector3.zero) player.position += move;

        // What the player is doing now is what shots and thrown items should carry, not the last thing the motor did
        playerController.BaseVelocity = Time.deltaTime > 0f ? move / Time.deltaTime : Vector3.zero;

        if (Mouse.current.scroll.ReadValue().y > 0)
        {
            baseSpeed += (speedAdjust * Time.deltaTime);
        }
        if (Mouse.current.scroll.ReadValue().y < 0)
        {
            baseSpeed -= (speedAdjust * Time.deltaTime);
        }
        if (baseSpeed < 1)
        {
            baseSpeed = 1.01f;
        }
    }
}