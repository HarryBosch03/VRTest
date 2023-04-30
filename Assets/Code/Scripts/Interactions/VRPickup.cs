using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Scripts.Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRPickup : MonoBehaviour
    {
        private const int ThrowSamples = 4;
        
        private readonly List<Transform> anchors = new();

        public AnchorBinding ActiveBinding { get; set; }
        public Rigidbody Rigidbody { get; private set; }

        private static readonly List<VRPickup> Pickups = new();

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            
            var anchorParent = transform.DeepFind("Anchors");

            if (anchorParent)
            {
                foreach (Transform anchor in anchorParent) anchors.Add(anchor);
            }
            else
            {
                anchors.Add(transform);
            }
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
            ActiveBinding?.Update();
        }

        private void FixedUpdate()
        {
            ActiveBinding?.FixedUpdate();
        }

        public class AnchorBinding
        {
            public readonly Transform target;
            public readonly Transform anchor;
            public readonly VRPickup pickup;
            public bool active;

            private readonly List<Vector3> lastPositions = new();

            public AnchorBinding(VRPickup pickup, Transform target, Transform anchor)
            {
                this.pickup = pickup;
                this.target = target;
                this.anchor = anchor;
                active = true;
                
                pickup.Rigidbody.isKinematic = true;
            }

            public void Update()
            {
                if (!active) return;

                anchor.position = target.position;
                anchor.rotation = target.rotation * Quaternion.Euler(90.0f, 0.0f, 0.0f);
            }

            public void FixedUpdate()
            {
                if (!active) return;

                lastPositions.Add(anchor.position);
                while (lastPositions.Count > ThrowSamples) lastPositions.RemoveAt(0);
            }

            public void Throw(float throwForceScale)
            {
                if (!active) return;
                
                var rb = pickup.Rigidbody;

                rb.isKinematic = false;

                var force = Vector3.zero;
                lastPositions.Add(anchor.position);
                for (var i = 1; i < lastPositions.Count; i++)
                {
                    force += (lastPositions[i] - lastPositions[i - 1]) / Time.deltaTime;
                }
                force /= lastPositions.Count;
                
                force *= Mathf.Min(1.0f, throwForceScale / rb.mass);
                force -= rb.velocity;
                rb.AddForceAtPosition(force * throwForceScale, anchor.position, ForceMode.VelocityChange);
                
                active = false;
            }
        }
    }
}