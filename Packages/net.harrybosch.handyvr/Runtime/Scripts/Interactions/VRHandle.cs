using UnityEngine;

namespace HandyVR.Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRHandle : VRBindable
    {
        [SerializeField] private float grabSpring = 500.0f;
        [SerializeField] private float grabDamper = 25.0f;

        private bool wasBound;
        private Vector3 offset;
        
        public override Rigidbody GetRigidbody() => GetComponentInParent<Rigidbody>();

        public override void OnBindingActivated(VRBinding binding)
        {
            offset = Handle.position - binding.position();
        }

        private void FixedUpdate()
        {
            if (!ActiveBinding) return;

            var diff = (BindingPosition + offset - Handle.position);
            var pointVelocity = Rigidbody.GetPointVelocity(Handle.position);
            var force = diff * grabSpring - pointVelocity * grabDamper;
            
            Rigidbody.AddForceAtPosition(force, Handle.position, ForceMode.Acceleration);
        }
    }
}