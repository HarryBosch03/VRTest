using UnityEngine;

namespace VRTest.Interactions
{
    public class VRBinding
    {
        private const int ThrowSamples = 3;

        public readonly VRBindable bindable;
        public bool active;
        
        private Vector3 lastPosition;
        
        private Vector3 lastValue;

        public Vector3 Position
        {
            get => bindable.Rigidbody ? bindable.Rigidbody.position : bindable.transform.position;
            set => bindable.SetPosition(value);
        }

        public Quaternion Rotation
        {
            set => bindable.SetRotation(value);
        }

        public VRBinding(VRBindable bindable)
        {
            if (bindable.ActiveBinding) bindable.ActiveBinding.Deactivate();

            this.bindable = bindable;
            active = true;

            bindable.OnBindingActivated();
        }

        public void Deactivate()
        {
            if (!active) return;
            active = false;

            bindable.OnBindingDeactivated();
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