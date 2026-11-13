using UnityEngine;
#if ENABLE_INPUT_SYSTEM || !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace AlpineRush
{
    /// <summary>
    /// Reads the Input System when it is enabled, and the legacy axes when it is not.
    /// This project ships with the new Input System only (activeInputHandler = 1).
    /// </summary>
    public static class DrivingInput
    {
        public static float Steer { get; private set; }
        public static float Throttle { get; private set; }
        public static bool Drift { get; private set; }
        public static bool Nitro { get; private set; }
        public static bool RespawnPressed { get; private set; }
        public static bool PausePressed { get; private set; }

        public static void Poll()
        {
            float steer = 0f;
            float throttle = 0f;
            bool drift = false;
            bool nitro = false;
            bool respawn = false;
            bool pause = false;

#if !ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            steer = Input.GetAxis("Horizontal");
            throttle = Input.GetAxis("Vertical");
            drift = Input.GetKey(KeyCode.Space);
            nitro = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            respawn = Input.GetKeyDown(KeyCode.R);
            pause = Input.GetKeyDown(KeyCode.Escape);
#else
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) steer -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) steer += 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) throttle += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) throttle -= 1f;
                drift |= keyboard.spaceKey.isPressed;
                nitro |= keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                respawn |= keyboard.rKey.wasPressedThisFrame;
                pause |= keyboard.escapeKey.wasPressedThisFrame;
            }

            var pad = Gamepad.current;
            if (pad != null)
            {
                float x = pad.leftStick.ReadValue().x;
                if (Mathf.Abs(x) > Mathf.Abs(steer)) steer = x;

                float triggers = pad.rightTrigger.ReadValue() - pad.leftTrigger.ReadValue();
                float stickY = pad.leftStick.ReadValue().y;
                if (stickY > 0.2f) triggers = Mathf.Max(triggers, stickY);
                if (stickY < -0.2f) triggers = Mathf.Min(triggers, stickY);
                if (Mathf.Abs(triggers) > 0.05f) throttle = triggers;

                drift |= pad.buttonSouth.isPressed || pad.leftShoulder.isPressed;
                nitro |= pad.rightShoulder.isPressed || pad.buttonEast.isPressed;
                respawn |= pad.buttonWest.wasPressedThisFrame;
                pause |= pad.startButton.wasPressedThisFrame;
            }
#endif

            Steer = Mathf.Clamp(steer, -1f, 1f);
            Throttle = Mathf.Clamp(throttle, -1f, 1f);
            Drift = drift;
            Nitro = nitro;
            RespawnPressed = respawn;
            PausePressed = pause;
        }
    }
}
