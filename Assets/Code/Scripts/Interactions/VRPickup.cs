using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRPickup : VRBindable
    {
        private readonly List<Transform> anchors = new();
        
        public override bool CanCreateDetachedBinding => true;

        protected override void Awake()
        {
            base.Awake();
            
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

        public override Transform GetClosestAnchor(Vector3 point)
        {
            if (anchors.Count == 0) return base.GetClosestAnchor(point);

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

        private void FixedUpdate()
        {
            ActiveBinding?.FixedUpdate();
        }
    }
}