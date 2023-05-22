using UnityEngine;

namespace HandyVR.Interactions
{
    /// <summary>
    /// A handle that can be added to a child of a physics object with constraints,
    /// allowing for complex behaviours like hinges or sliders.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu("HandyVR/Handle", Reference.AddComponentMenuOrder.Components)]
    public sealed class VRHandle : VRBindable
    {
        [Tooltip("Spring Constant used to match the handle position")]
        [SerializeField] private float grabSpring = 500.0f;
        [Tooltip("Damping Constant used to match the handle velocity")]
        [SerializeField] private float grabDamper = 25.0f;
        [Tooltip("Scaling to apply to force when converting it to torque")]
        [SerializeField] private float grabAngle = 10.0f;

        private bool wasBound;
        private Vector3 translationOffset;
        private Quaternion rotationOffset;
        
        // Look for Rigidbody in parents as well.
        public override Rigidbody GetRigidbody() => GetComponentInParent<Rigidbody>();

        public override void OnBindingActivated(VRBinding binding)
        {
            // Calculate the offset when we grab the handle, this stops the handle
            // rocketing towards the hands actual position.
            translationOffset = Handle.position - binding.position();
            rotationOffset = Handle.rotation * Quaternion.Inverse(binding.rotation());
        }

        private void FixedUpdate()
        {
            if (!ActiveBinding) return;

            // Match the hands position through a simple spring damper, applied to the handles parent at the handles position.
            var diff = (BindingPosition + translationOffset - Handle.position);
            var pointVelocity = Rigidbody.GetPointVelocity(Handle.position);
            var force = diff * grabSpring - pointVelocity * grabDamper;
            
            Rigidbody.AddForceAtPosition(force, Handle.position, ForceMode.Acceleration);
            
            var delta = BindingRotation * rotationOffset * Quaternion.Inverse(Rigidbody.rotation);
            delta.ToAngleAxis(out var angle, out var axis);
            
            // Calculate a torque to move the rigidbody to the target rotation with zero angular velocity.
            var torque = axis * (angle * Mathf.Deg2Rad * grabSpring) - Rigidbody.angularVelocity * grabDamper;
            Rigidbody.AddTorque(torque * grabAngle, ForceMode.Acceleration);
        }
    }
}