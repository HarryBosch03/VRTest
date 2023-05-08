using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRPickup : VRBindable
    {
        [SerializeField] private VRBindingType bindingType;
        [SerializeField] private Vector3 pOffset;
        [SerializeField] private Quaternion rOffset = Quaternion.identity;

        private readonly List<ColliderData> colliderData = new();
        
        private bool wasUsingGravity;
        
        private Vector3 tPosition;
        private Quaternion tRotation;

        public event Action BindingActiveEvent;
        public event Action BindingDeactiveEvent;
        
        private static PhysicMaterial overridePhysicMaterial;

        private static PhysicMaterial OverridePhysicMaterial
        {
            get
            {
                if (overridePhysicMaterial) return overridePhysicMaterial;
                
                overridePhysicMaterial = new PhysicMaterial();
                overridePhysicMaterial.name = "VR Pickup | Override Physics Material [SHOULD ONLY BE ON WHILE OBJECT IS HELD]";
                overridePhysicMaterial.bounciness = 0.0f;
                overridePhysicMaterial.dynamicFriction = 0.0f;
                overridePhysicMaterial.staticFriction = 0.0f;
                overridePhysicMaterial.bounceCombine = PhysicMaterialCombine.Multiply;
                overridePhysicMaterial.frictionCombine = PhysicMaterialCombine.Multiply;
                return overridePhysicMaterial;
            }
        }
        
        public VRBindingType BindingType => bindingType;

        public override void SetPosition(Vector3 position)
        {
            tPosition = position;
        }

        public override void SetRotation(Quaternion rotation)
        {
            tRotation = rotation;
        }

        public override void OnBindingActivated()
        {
            wasUsingGravity = Rigidbody.useGravity;
            Rigidbody.useGravity = false;

            foreach (var data in colliderData)
            {
                data.lastMaterial = data.collider.sharedMaterial;
                data.collider.sharedMaterial = OverridePhysicMaterial;
            }
            
            BindingActiveEvent?.Invoke();
        }

        public override void OnBindingDeactivated()
        {
            Rigidbody.useGravity = wasUsingGravity;

            foreach (var data in colliderData)
            {
                data.collider.sharedMaterial = data.lastMaterial;
            }
            
            BindingDeactiveEvent?.Invoke();
        }

        protected override void Awake()
        {
            base.Awake();

            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            var colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                colliderData.Add(new ColliderData(collider));
            }
        }

        private void FixedUpdate()
        {
            MoveIfBound();
        }

        private void MoveIfBound()
        {
            if (!ActiveBinding) return;

            var force = (tPosition + pOffset - Rigidbody.position) / Time.deltaTime - Rigidbody.velocity;
            Rigidbody.AddForce(force, ForceMode.VelocityChange);

            var delta = tRotation * rOffset.normalized * Quaternion.Inverse(Rigidbody.rotation);
            delta.ToAngleAxis(out var angle, out var axis);
            var torque = axis * (angle * Mathf.Deg2Rad / Time.deltaTime) - Rigidbody.angularVelocity;
            Rigidbody.AddTorque(torque, ForceMode.VelocityChange);
        }

        private class ColliderData
        {
            public Collider collider;
            public PhysicMaterial lastMaterial;

            public ColliderData(Collider collider)
            {
                this.collider = collider;
                this.lastMaterial = collider.sharedMaterial;
            }
        }
    }
}