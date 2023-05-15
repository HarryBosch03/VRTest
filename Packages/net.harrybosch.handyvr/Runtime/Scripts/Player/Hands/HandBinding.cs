using System.Collections.Generic;
using HandyVR.Interactions;
using UnityEngine;

namespace HandyVR.Player.Hands
{
    [System.Serializable]
    public class HandBinding
    {
        [SerializeField] private float pickupRange = 0.2f;
        [SerializeField] private float detachedBindingAngularDrag = 5.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float detachedBindingBounce = 0.3f;

        private PlayerHand hand;
        private LineRenderer lines;

        private float detachedBindingDistance;

        private static HashSet<VRBindable> existingDetachedBindings = new();
        public VRBinding ActiveBinding { get; private set; }
        public VRBindable DetachedBinding { get; private set; }
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
            if (DetachedBinding)
            {
                lines.SetLine(hand.PointRef.position, DetachedBinding.Rigidbody.position);
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
            if (!DetachedBinding) return;

            if (DetachedBinding.ActiveBinding)
            {
                RemoveDetachedBinding(false);
                return;
            }

            var dir = (hand.PointRef.position - DetachedBinding.Rigidbody.position);
            var l = dir.magnitude;
            dir /= l;

            var force = Vector3.zero;
            if (l - detachedBindingDistance > 0.0f)
            {
                force = dir * Mathf.Max(l - detachedBindingDistance, 0.0f) / Time.deltaTime;

                var dot = Vector3.Dot(dir, DetachedBinding.Rigidbody.velocity);
                if (dot < 0.0f) force -= dir * dot * (1.0f + detachedBindingBounce);
            }


            DetachedBinding.Rigidbody.AddForce(force, ForceMode.VelocityChange);
            DetachedBinding.Rigidbody.AddTorque(-DetachedBinding.Rigidbody.angularVelocity * detachedBindingAngularDrag,
                ForceMode.Acceleration);
        }

        private void RemoveDetachedBinding(bool bind)
        {
            if (!DetachedBinding) return;
            Utility.IgnoreCollision(DetachedBinding.gameObject, hand.gameObject, false);

            if (bind) Bind(DetachedBinding);

            existingDetachedBindings.Remove(DetachedBinding);
            DetachedBinding = null;
        }

        private void UpdateActiveBinding()
        {
            hand.ActiveBinding.Position = hand.Target.position;
            hand.ActiveBinding.Rotation = hand.Target.rotation;
            hand.ActiveBinding.Flipped = hand.Flipped;
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
            var ray = new Ray(hand.PointRef.position, hand.PointRef.forward);
            if (!Physics.Raycast(ray, out var hit)) return null;
            if (!hit.transform.TryGetComponent(out VRBindable bindable)) return null;
            if (existingDetachedBindings.Contains(bindable)) return null;
            if (bindable.ActiveBinding) return null;

            return bindable;
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
            if (pointingAt.ActiveBinding) return;
            if (existingDetachedBindings.Contains(pointingAt)) return;

            DetachedBinding = pointingAt;
            detachedBindingDistance = (hand.PointRef.position - DetachedBinding.Rigidbody.position).magnitude;
            existingDetachedBindings.Add(DetachedBinding);
            Utility.IgnoreCollision(DetachedBinding.gameObject, hand.gameObject, true);
        }
    }
}