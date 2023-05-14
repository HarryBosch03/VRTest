using System;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace HandyVR.Player.Input
{
    public sealed class HandInput
    {
        private const float RumbleFrequency = 20.0f;
        
        public Func<XRController> Controller { get; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public InputWrapper Grip { get; } = new();
        public InputWrapper Trigger { get; } = new();
        public InputWrapper ThumbstickX { get; } = new() { pressPoint = 0.1f, };
        public InputWrapper ThumbstickY { get; } = new() { pressPoint = 0.1f, };

        public HandInput(Func<XRController> controller)
        {
            Controller = controller;
        }
        
        public void Update()
        {
            var controller = Controller();
            if (controller == null) return;

            Position = controller.devicePosition.ReadValue();
            Rotation = controller.deviceRotation.ReadValue();

            switch (controller)
            {
                case OculusTouchController touchController:
                    Grip.Update(touchController.grip);
                    Trigger.Update(touchController.trigger);
                    ThumbstickX.Update(touchController.thumbstick, v => v.x);
                    ThumbstickY.Update(touchController.thumbstick, v => v.y);
                    break;
                case UnityEngine.XR.OpenXR.Features.Interactions.OculusTouchControllerProfile.OculusTouchController touchController:
                    Grip.Update(touchController.grip);
                    Trigger.Update(touchController.trigger);
                    ThumbstickX.Update(touchController.thumbstick, v => v.x);
                    ThumbstickY.Update(touchController.thumbstick, v => v.y);
                    break;
            }
        }

        public void Rumble(float amplitude, float duration)
        {
            var controller = Controller();
            if (controller is XRControllerWithRumble rumble)
            {
                rumble.SendImpulse(amplitude, 1.0f / RumbleFrequency);
            }
        }
        
        public class InputWrapper
        {
            public float pressPoint = 0.5f;

            private float lastValue;

            public float Value { get; private set; }
            public InputState State { get; private set; }
            public bool Down => Mathf.Abs(Value) > pressPoint;

            public void Update(InputControl<float> driver) => Update(driver.ReadValue());
            public void Update<T>(InputControl<T> driver, Func<T, float> getFloat) where T : struct => Update(getFloat(driver.ReadValue()));
            public void Update(float value)
            {
                Value = value;
                
                var state = Value > pressPoint;
                var lastState = lastValue > pressPoint;

                if (state)
                {
                    State = lastState ? InputState.Pressed : InputState.PressedThisFrame;
                }
                else
                {
                    State = lastState ? InputState.ReleasedThisFrame : InputState.Released;
                }
                
                lastValue = Value;
            }
            
            public enum InputState
            {
                PressedThisFrame,
                Pressed,
                ReleasedThisFrame,
                Released,
            }

            public void ChangedThisFrame(Action<bool> callback)
            {
                switch (State)
                {
                    case InputState.PressedThisFrame:
                        callback(true);
                        break;
                    case InputState.ReleasedThisFrame:
                        callback(false);
                        break;
                    case InputState.Pressed:
                    case InputState.Released:
                    default: break;
                }
            }
        }
    }
}