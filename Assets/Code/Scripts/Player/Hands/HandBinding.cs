using System.Collections.Generic;
using Interactions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Hands
{
    [System.Serializable]
    public class HandBinding
    {
        [SerializeField] private float pickupRange = 0.2f;
        [SerializeField] private float throwForceScale = 1.0f;
        [SerializeField] private float detachedBindingSpring = 5.0f;
        [SerializeField] private float detachedBindingDamping = 5.0f;
        [SerializeField] private float detachedBindingAngularDrag = 5.0f;
        [SerializeField] private float maxPointAngle = 15.0f;

        private PlayerHand hand;
        private LineRenderer lines;
        private Vector3 lastHandVelocity;

        private VRBindable pointGrab;

        private static HashSet<VRBindable> existingPointBindings = new();

        public void Init(PlayerHand hand)
        {
            this.hand = hand;

            lines = hand.GetComponentInChildren<LineRenderer>();
            lines.enabled = false;
        }

        public void Update()
        {
            hand.gripAction.WasPressedThisFrame(OnGrip);

            if (hand.activeBinding)
            {
                UpdateActiveBinding();
                return;
            }

            lines.enabled = false;
            if (pointGrab)
            {
                lines.SetLine(hand.PointRef.position, pointGrab.Rigidbody.position);
            }
            else
            {
                var pointingAt = GetPointingAt();
                if (pointingAt)
                {
                    lines.SetLine(hand.PointRef.position, pointingAt.transform.position);
                }
            }
        }

        public void FixedUpdate()
        {
            UpdatePointGrab();
        }

        private void UpdatePointGrab()
        {
            if (!pointGrab) return;

            if ((hand.transform.position - pointGrab.Rigidbody.position).magnitude <
                (pointGrab.Rigidbody.velocity.magnitude * Time.deltaTime + pickupRange) * 1.01f)
            {
                Bind(pointGrab);
                existingPointBindings.Remove(pointGrab);
                pointGrab = null;
                return;
            }

            var dir = (hand.transform.position - pointGrab.Rigidbody.position).normalized;
            var force = dir * detachedBindingSpring - pointGrab.Rigidbody.velocity * detachedBindingDamping;

            force += (hand.Rigidbody.velocity - lastHandVelocity) / Time.deltaTime;
            lastHandVelocity = hand.Rigidbody.velocity;
            
            pointGrab.Rigidbody.AddForce(force, ForceMode.Acceleration);
            pointGrab.Rigidbody.AddTorque(-pointGrab.Rigidbody.angularVelocity * detachedBindingAngularDrag,
                ForceMode.Acceleration);
        }

        private void UpdateActiveBinding()
        {
            hand.activeBinding.Position = hand.Target.position;
            hand.activeBinding.Rotation = hand.Target.rotation;
        }

        private void Bind(VRBindable pickup)
        {
            if (!hand.ignoreLastBindingCollision)
            {
                Utility.IgnoreCollision(pickup.gameObject, hand.gameObject, true);
                hand.ignoreLastBindingCollision = true;
            }

            hand.activeBinding = pickup.CreateBinding(throwForceScale);
        }

        private VRBindable GetPointingAt()
        {
            float GetAngle(Component pickup) => Vector3.Angle(pickup.transform.position - hand.PointRef.position,
                hand.PointRef.forward);

            VRBindable best = null;
            foreach (var other in VRBindable.All)
            {
                if (existingPointBindings.Contains(other)) continue;
                
                var a1 = GetAngle(other);
                if (a1 > maxPointAngle) continue;
                if (!CanSee(other)) continue;
                if (!best)
                {
                    best = other;
                    continue;
                }

                var a2 = GetAngle(best);
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
            hand.activeBinding?.Deactivate();
            existingPointBindings.Remove(pointGrab);
            pointGrab = null;

            if (state) OnGripPressed();
        }

        private void OnGripPressed()
        {
            var pickup = VRBindable.GetPickup(hand.transform.position, pickupRange);
            if (!pickup)
            {
                TryGetPointGrab();
                return;
            }

            Bind(pickup);
        }

        private void TryGetPointGrab()
        {
            var pointingAt = GetPointingAt();
            if (!pointingAt) return;
            if (!pointingAt.Rigidbody) return;

            pointGrab = pointingAt;
            existingPointBindings.Add(pointGrab);
        }
    }
}