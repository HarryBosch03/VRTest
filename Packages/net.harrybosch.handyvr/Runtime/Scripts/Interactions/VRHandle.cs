using System;
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
        private Vector3 grabTarget;

        public Vector3 HandPosition => Handle.position;
        public Quaternion HandRotation => Handle.rotation;

        public override Rigidbody GetRigidbody() => GetComponentInParent<Rigidbody>();

        public override void SetPosition(Vector3 position)
        {
            grabTarget = position;
        }
        
        public override void SetRotation(Quaternion rotation) { }

        private void FixedUpdate()
        {
            if (!ActiveBinding) return;

            var diff = (grabTarget - Handle.position);
            var pointVelocity = Rigidbody.GetPointVelocity(Handle.position);
            var force = diff * grabSpring - pointVelocity * grabDamper;
            
            Rigidbody.AddForceAtPosition(force, Handle.position, ForceMode.Acceleration);
        }
    }
}