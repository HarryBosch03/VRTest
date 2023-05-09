using UnityEngine;

namespace VRTest.Runtime.Scripts.Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRHinge : VRBindable
    {
        [SerializeField] private float grabSpring = 25.0f;
        [SerializeField] private float grabDamper = 5.0f;

        private new Rigidbody rigidbody;
        private bool wasBound;
        private Vector3? grabTarget;

        public Vector3 HandPosition => Handle.position;
        public Quaternion HandRotation => Handle.rotation;

        protected override void Awake()
        {
            base.Awake();

            rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
        }

        public override void SetPosition(Vector3 position)
        {
            grabTarget = position;
        }

        private void FixedUpdate()
        {
            if (!grabTarget.HasValue) return;

            var force = (grabTarget.Value - Handle.position) * grabSpring - rigidbody.GetPointVelocity(Handle.position) * grabDamper;
            rigidbody.AddForceAtPosition(force, Handle.position, ForceMode.Acceleration);
            grabTarget = null;
        }

        public override void SetRotation(Quaternion rotation)
        {
            
        }
    }
}