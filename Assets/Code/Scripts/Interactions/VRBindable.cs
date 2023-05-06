using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    public abstract class VRBindable : MonoBehaviour
    {
        public Rigidbody Rigidbody { get; private set; }
        public VRBinding ActiveBinding { get; private set; }
        
        public Transform Handle { get; set; }
        
        private static readonly List<VRBindable> Pickups = new();

        public abstract bool CanCreateDetachedBinding { get; }

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();

            Handle = transform.DeepFind("Handle");
            if (!Handle) Handle = transform;
        }

        protected virtual void OnEnable()
        {
            Pickups.Add(this);
        }

        protected virtual void OnDisable()
        {
            Pickups.Remove(this);
        }
        
        public virtual void SetPosition(Vector3 position) => transform.position = position;
        public virtual void SetRotation(Quaternion rotation) => transform.rotation = rotation;

        public virtual VRBinding CreateBinding(float throwForce)
        {
            return ActiveBinding = new VRBinding(this, throwForce);
        }

        protected virtual void FixedUpdate()
        {
            if (ActiveBinding) ActiveBinding.FixedUpdate();
        }
        
        public static VRBindable GetPickup(Vector3 from, float range)
        {
            VRBindable res = null;
            foreach (var pickup in Pickups)
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
    }
}