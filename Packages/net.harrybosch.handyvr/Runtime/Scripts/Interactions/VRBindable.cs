﻿using System.Collections.Generic;
using HandyVR.Interactions.Pickups;
using HandyVR.Player;
using HandyVR.Player.Input;
using UnityEngine;

namespace HandyVR.Interactions
{
    public abstract class VRBindable : MonoBehaviour
    {
        public Rigidbody Rigidbody { get; private set; }
        public VRBinding ActiveBinding { get; private set; }

        public Transform Handle { get; set; }

        public static readonly List<VRBindable> All = new();

        public virtual Rigidbody GetRigidbody() => GetComponent<Rigidbody>();
        
        protected virtual void Awake()
        {
            Rigidbody = GetRigidbody();

            Handle = transform.DeepFind("Handle");
            if (!Handle) Handle = transform;
        }

        protected virtual void OnEnable()
        {
            All.Add(this);
        }

        protected virtual void OnDisable()
        {
            All.Remove(this);
        }

        public virtual void SetPosition(Vector3 position) => transform.position = position;
        public virtual void SetRotation(Quaternion rotation) => transform.rotation = rotation;
        public virtual void SetFlipped(bool flipped) { }

        public VRBinding CreateBinding(bool flipped)
        {
            return ActiveBinding = new VRBinding(this);
        }

        public virtual void OnBindingActivated() { }
        public virtual void OnBindingDeactivated() { }

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