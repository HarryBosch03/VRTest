using System;
using HandyVR.Interactions;
using Interactions;
using UnityEngine;
using UnityEngine.Serialization;

namespace HandyVR.Switches
{
    public sealed class VRSlider : FloatDriver
    {
        [SerializeField] private float extents;
        
        private VRHandle handle;
        private new Rigidbody rigidbody;

        public override float Value => handle.transform.localPosition.z / extents * 0.5f + 0.5f;

        private void Awake()
        {
            rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            
            handle = GetComponentInChildren<VRHandle>();
            SetupHandle();
        }

        private void SetupHandle()
        {
            var rigidbody = handle.gameObject.GetOrAddComponent<Rigidbody>();
            rigidbody.mass = 0.02f;
            rigidbody.drag = 6.0f;
            rigidbody.useGravity = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            
            var joint = handle.gameObject.GetOrAddComponent<ConfigurableJoint>();
            joint.connectedBody = this.rigidbody;
            
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Limited;
            
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = Vector3.zero;
            
            joint.linearLimit = new SoftJointLimit()
            {
                limit = extents,
                bounciness = 0.0f,
                contactDistance = 0.0f,
            };
        }
    }
}