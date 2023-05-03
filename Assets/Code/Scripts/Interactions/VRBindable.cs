using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    public abstract class VRBindable : MonoBehaviour
    {
        public Rigidbody Rigidbody { get; private set; }
        public VRBinding ActiveBinding { get; set; }
        
        private static readonly List<VRBindable> Pickups = new();

        public abstract bool CanCreateDetachedBinding { get; }

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            Pickups.Add(this);
        }

        protected virtual void OnDisable()
        {
            Pickups.Remove(this);
        }
        
        public virtual Vector3 SetPosition(Vector3 position) => transform.position = position;
        public virtual Quaternion SetRotation(Quaternion rotation) => transform.rotation = rotation;
        
        public virtual Transform GetClosestAnchor(Vector3 point) => transform;

        // Gets the Pickup With the best score, does not return an element if their score is below :threshold:
        public static VRBindable GetPickup(Func<VRBindable, float> scoringMethod, float threshold = 0.0f)
        {
            VRBindable res = null;
            foreach (var pickup in Pickups)
            {
                var score = scoringMethod(pickup);
                if (score < threshold) continue;

                res = pickup;
                threshold = score;
            }

            return res;
        }
    }
}