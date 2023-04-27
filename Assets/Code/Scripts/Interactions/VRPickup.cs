using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Scripts.Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRPickup : MonoBehaviour
    {
        private readonly List<Transform> anchors = new();
        
        public AnchorBinding ActiveBinding { get; set; }

        private static readonly List<VRPickup> Pickups = new();
        
        private void Awake()
        {
            var anchorParent = transform.DeepFind("Anchors");
            foreach (Transform anchor in anchorParent) anchors.Add(anchor);
        }

        private void OnEnable()
        {
            Pickups.Add(this);
        }

        private void OnDisable()
        {
            Pickups.Remove(this);
        }

        // Gets the Pickup With the best score, does not return an element if their score is below :threshold:
        public static VRPickup GetPickup(Func<VRPickup, float> scoringMethod, float threshold = 0.0f)
        {
            VRPickup res = null;
            foreach (var pickup in Pickups)
            {
                var score = scoringMethod(pickup);
                if (score < threshold) continue;
                
                res = pickup;
                threshold = score;
            }
            return res;
        }

        public Transform GetClosestAnchor(Vector3 point)
        {
            if (anchors.Count == 0) return null;
            
            var res = anchors[0];
            for (var i = 1; i < anchors.Count; i++)
            {
                float
                    d1 = (anchors[i].position - point).sqrMagnitude,
                    d2 = (res.position - point).sqrMagnitude;

                if (d1 < d2) res = anchors[i];
            }
            return res;
        }

        private void Update()
        {
            ActiveBinding?.Update(this);
        }

        public class AnchorBinding
        {
            public readonly Transform target;
            public readonly Transform anchor;
            public bool active;

            public AnchorBinding(Transform target, Transform anchor)
            {
                this.target = target;
                this.anchor = anchor;
                active = true;
            }

            public void Update(VRPickup pickup)
            {
                if (!active) return;
                
                var transform = pickup.transform;

                transform.position += target.position - anchor.position;
                transform.rotation *= target.rotation * Quaternion.Inverse(anchor.rotation);
            }

            public void Clear()
            {
                active = false;
            }
        }
    }
}
