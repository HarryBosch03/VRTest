using System;
using UnityEngine;

namespace HandyVR.Interactions
{
    public class VRBinding
    {
        public readonly VRBindable bindable;
        public bool active;

        private Vector3 lastPosition;
        private Vector3 lastValue;

        public readonly Func<Vector3> position;
        public readonly Func<Quaternion> rotation;
        public readonly Func<bool> flipped;
        
        public VRBinding(VRBindable bindable, Func<Vector3> position, Func<Quaternion> rotation, Func<bool> flipped)
        {
            if (bindable.ActiveBinding) bindable.ActiveBinding.Deactivate();

            this.position = position;
            this.rotation = rotation;
            this.flipped = flipped;
            
            this.bindable = bindable;
            active = true;

            bindable.OnBindingActivated(this);
        }

        public void Deactivate()
        {
            bindable.OnBindingDeactivated(this);
            
            if (!active) return;
            active = false;
        }

        public bool Valid()
        {
            if (!active) return false;
            if (!bindable) return false;

            return true;
        }

        public static implicit operator bool(VRBinding binding)
        {
            return binding != null && binding.Valid();
        }
    }
}