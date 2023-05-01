using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Scripts.Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRPickup : MonoBehaviour
    {
        private const int ThrowSamples = 10;
        
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

        private void LateUpdate()
        {
            ActiveBinding?.LateUpdate();
        }

        private void FixedUpdate()
        {
            ActiveBinding?.FixedUpdate();
        }

        public class AnchorBinding
        {
            private const bool DebugThrow = false;

            public readonly Func<Pose> target;
            public readonly Transform anchor;
            public readonly VRPickup pickup;
            public bool active;

            private readonly List<Vector3> lastPositions = new();

            private static readonly List<GameObject> TmpDebug = new();

            public AnchorBinding(VRPickup pickup, Transform target, Transform anchor)
            {
                this.pickup = pickup;
                this.target = () => new Pose(target.position, target.rotation);
                this.anchor = anchor;
                active = true;
                
                pickup.Rigidbody.isKinematic = true;
            }

            public void LateUpdate()
            {
                if (!active) return;

                anchor.position = target().position;
                anchor.rotation = target().rotation * Quaternion.Euler(90.0f, 0.0f, 0.0f);
            }

            public void FixedUpdate()
            {
                if (!active) return;

                lastPositions.Add(anchor.position);
                while (lastPositions.Count > ThrowSamples) lastPositions.RemoveAt(0);
            }

            public void Debug()
            {
                if (!DebugThrow) return;
                
                foreach (var obj in TmpDebug)
                {
                    Destroy(obj);
                }
                TmpDebug.Clear();

                for (var i = 0; i < lastPositions.Count; i++)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(go.GetComponent<Collider>());

                    go.transform.position = lastPositions[i];
                    go.transform.localScale = Vector3.one * 0.05f;
                    TmpDebug.Add(go);
                }
            }
            
            public void Throw(float throwForceScale)
            {
                if (!active) return;
                
                var rb = pickup.Rigidbody;

                rb.isKinematic = false;

                var force = Vector3.zero;
                lastPositions.Add(anchor.position);

                var tw = 0.0f;
                for (var i = 1; i < lastPositions.Count; i++)
                {
                    var v = lastPositions[i] - lastPositions[i - 1];
                    var w = v.magnitude;
                    
                    force += v * w / Time.deltaTime;
                    tw += w;
                }
                force /= tw;

                Debug();
                
                force *= Mathf.Min(1.0f, throwForceScale / rb.mass);
                force -= rb.velocity;
                rb.AddForceAtPosition(force * throwForceScale, anchor.position, ForceMode.VelocityChange);
                
                active = false;
            }
        }
    }
}