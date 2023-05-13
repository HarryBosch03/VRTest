using System.Collections.Generic;
using HandyVR.Interactions;
using UnityEngine;

namespace HandyVR.Player.Hands
{
    [System.Serializable]
    public class HandBinding
    {
        [SerializeField] private float pickupRange = 0.2f;
        [SerializeField] private float throwForceScale = 1.0f;
        [SerializeField] private float detachedBindingAngularDrag = 5.0f;
        [SerializeField] private float maxPointAngle = 15.0f;

        private PlayerHand hand;
        private LineRenderer lines;

        private VRBindable detachedBinding;
        private float detachedBindingDistance;

        private static HashSet<VRBindable> existingDetachedBindings = new();
        public VRBinding ActiveBinding { get; private set; }
        public bool DetachedBinding => detachedBinding;
        public VRBindable PointingAt { get; private set; }

        public void Init(PlayerHand hand)
        {
            this.hand = hand;

            lines = hand.GetComponentInChildren<LineRenderer>();
            lines.enabled = false;
        }

        public void Update()
        {
            PointingAt = null;

            hand.Input.Grip.ChangedThisFrame(OnGrip);

            if (hand.ActiveBinding)
            {
                UpdateActiveBinding();
                return;
            }

            lines.enabled = false;
            if (detachedBinding)
            {
                lines.SetLine(hand.PointRef.position, detachedBinding.Rigidbody.position);
            }
            else
            {
                PointingAt = GetPointingAt();
                if (PointingAt)
                {
                    lines.SetLine(hand.PointRef.position, PointingAt.transform.position);
                }
            }
        }

        public void FixedUpdate()
        {
            UpdateDetachedBinding();
        }

        private void UpdateDetachedBinding()
        {
            if (!detachedBinding) return;

            // if ((hand.transform.position - detachedBinding.Rigidbody.position).magnitude <
            //     (detachedBinding.Rigidbody.velocity.magnitude * Time.deltaTime) * 1.01f)
            // {
            //     RemoveDetachedBinding(true);
            //     return;
            // }

            var dir = (hand.PointRef.position - detachedBinding.Rigidbody.position);
            var l = dir.magnitude;
            dir /= l;

            var force = Vector3.zero;
            if (l - detachedBindingDistance > 0.0f)
            {
                force = dir * Mathf.Max(l - detachedBindingDistance, 0.0f) / Time.deltaTime;

                var dot = Vector3.Dot(dir, detachedBinding.Rigidbody.velocity);
                if (dot < 0.0f) detachedBinding.Rigidbody.velocity -= dir * dot;
            }


            detachedBinding.Rigidbody.AddForce(force, ForceMode.Acceleration);
            detachedBinding.Rigidbody.AddTorque(-detachedBinding.Rigidbody.angularVelocity * detachedBindingAngularDrag,
                ForceMode.Acceleration);
        }

        private void RemoveDetachedBinding(bool bind)
        {
            if (!detachedBinding) return;
            Utility.IgnoreCollision(detachedBinding.gameObject, hand.gameObject, false);

            if (bind) Bind(detachedBinding);

            existingDetachedBindings.Remove(detachedBinding);
            detachedBinding = null;
        }

        private void UpdateActiveBinding()
        {
            hand.ActiveBinding.Position = hand.Target.position;
            hand.ActiveBinding.Rotation = hand.Flipped
                ? hand.Target.rotation
                : hand.Target.rotation * Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }

        private void Bind(VRBindable pickup)
        {
            if (!hand.ignoreLastBindingCollision)
            {
                Utility.IgnoreCollision(pickup.gameObject, hand.gameObject, true);
                hand.ignoreLastBindingCollision = true;
            }

            ActiveBinding = pickup.CreateBinding(hand.Flipped);
        }

        private VRBindable GetPointingAt()
        {
            float getAngle(Component pickup) => Vector3.Angle(pickup.transform.position - hand.PointRef.position,
                hand.PointRef.forward);

            VRBindable best = null;
            foreach (var other in VRBindable.All)
            {
                if (existingDetachedBindings.Contains(other)) continue;

                var a1 = getAngle(other);
                if (a1 > maxPointAngle) continue;
                if (!CanSee(other)) continue;
                if (!best)
                {
                    best = other;
                    continue;
                }

                var a2 = getAngle(best);
                if (a1 > a2) continue;
                best = other;
            }

            return best;
        }

        private bool CanSee(VRBindable other)
        {
            var ray = new Ray(hand.PointRef.position, other.transform.position - hand.PointRef.position);
            return Physics.Raycast(ray, out var hit) && hit.transform.IsChildOf(other.transform);
        }

        private void OnGrip(bool state)
        {
            hand.ActiveBinding?.Deactivate();
            RemoveDetachedBinding(false);

            if (state) OnGripPressed();
        }

        private void OnGripPressed()
        {
            var pickup = VRBindable.GetPickup(hand.transform.position, pickupRange);
            if (!pickup)
            {
                TryGetDetachedBinding();
                return;
            }

            Bind(pickup);
        }

        private void TryGetDetachedBinding()
        {
            var pointingAt = GetPointingAt();
            if (!pointingAt) return;
            if (!pointingAt.Rigidbody) return;

            detachedBinding = pointingAt;
            detachedBindingDistance = (hand.PointRef.position - detachedBinding.Rigidbody.position).magnitude;
            existingDetachedBindings.Add(detachedBinding);
            Utility.IgnoreCollision(detachedBinding.gameObject, hand.gameObject, true);
        }
    }
}