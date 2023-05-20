using System;
using System.Collections.Generic;
using HandyVR.Interactions.Pickups;
using HandyVR.Player;
using HandyVR.Player.Input;
using UnityEngine;

namespace HandyVR.Interactions
{
    public abstract class VRBindable : MonoBehaviour
    {
        // ReSharper disable once InconsistentNaming
        private Rigidbody rigidbody_DoNotUse;

        public Rigidbody Rigidbody
        {
            get
            {
                if (!rigidbody_DoNotUse) rigidbody_DoNotUse = GetRigidbody();
                return rigidbody_DoNotUse;
            }
        }
        public VRBinding ActiveBinding { get; private set; }

        public Transform Handle { get; set; }

        public static readonly List<VRBindable> All = new();

        public virtual Rigidbody GetRigidbody() => GetComponent<Rigidbody>();

        public Vector3 BindingPosition => ActiveBinding.position();
        public Quaternion BindingRotation => ActiveBinding.rotation();
        public bool BindingFlipped => ActiveBinding.flipped();

        protected virtual void Awake()
        {
            Handle = transform;
        }

        protected virtual void OnEnable()
        {
            All.Add(this);
        }

        protected virtual void OnDisable()
        {
            All.Remove(this);
        }

        public VRBinding CreateBinding(Func<Vector3> position, Func<Quaternion> rotation, Func<bool> flipped)
        {
            return ActiveBinding = new VRBinding(this, position, rotation, flipped);
        }

        public virtual void OnBindingActivated(VRBinding binding) { }
        public virtual void OnBindingDeactivated(VRBinding binding) { }

        public static VRBindable GetPickup(Vector3 from, float range)
        {
            VRBindable res = null;
            foreach (var pickup in All)
            {
                var d1 = (pickup.Handle.position - from).sqrMagnitude;
                if (d1 > range * range) continue;
                if (!res)
                {
                    res = pickup;
                    continue;
                }

                var d2 = (res.Handle.position - from).sqrMagnitude;
                if (d1 < d2)
                {
                    res = pickup;
                }
            }

            return res;
        }

        public void Trigger(PlayerHand hand, HandInput.InputWrapper action)
        {
            var listeners = GetComponentsInChildren<IVRBindableListener>();
            foreach (var listener in listeners)
            {
                listener.Trigger(hand, this, action);
            }
        }
    }
}