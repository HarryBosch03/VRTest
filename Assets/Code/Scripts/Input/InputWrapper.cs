using System;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputWrapper
    {
        public readonly InputAction action;

        private bool lastState;
        private bool state;

        public InputWrapper(string binding, ref Action updateEvent)
        {
            action = new InputAction(binding: binding);
            action.Enable();

            updateEvent += Update;
        }

        public void Update()
        {
            lastState = state;
            state = action.State();
        }

        public void WasPressedThisFrame(Action<bool> callback)
        {
            if (state == lastState) return;
            callback(state);
        }
    }
}