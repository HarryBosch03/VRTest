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
        [SerializeField] private float detachedBindingSpring = 5.0f;
        [SerializeField] private float detachedBindingDamping = 5.0f;
        [SerializeField] private float detachedBindingAngularDrag = 5.0f;
        [SerializeField] private float maxPointAngle = 15.0f;

        private PlayerHand hand;
        private LineRenderer lines;
        private Vector3 lastHandVelocity;

        private VRBindable pointGrab;

        private static HashSet<VRBindable> existingPointBindings = new();
        public VRBinding ActiveBinding { get; private set; }

        public void Init(PlayerHand hand)
        {
            this.hand = hand;

            lines = hand.GetComponentInChildren<LineRenderer>();
            lines.enabled = false;
        }

        public void Update()
        {
            hand.Input.Grip.ChangedThisFrame(OnGrip);

            if (hand.ActiveBinding)
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
                (pointGrab.Rigidbody.velocity.magnitude * Time.deltaTime) * 1.01f)
            {
                DetachPointGrab(true);
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

        private void DetachPointGrab(bool bind)
        {
            if (!pointGrab) return;
            Utility.IgnoreCollision(pointGrab.gameObject, hand.gameObject, false);
            
            if (bind) Bind(pointGrab);

            existingPointBindings.Remove(pointGrab);
            pointGrab = null;
        }

        private void UpdateActiveBinding()
        {
            hand.ActiveBinding.Position = hand.Target.position;
            hand.ActiveBinding.Rotation = hand.Flipped ? hand.Target.rotation : hand.Target.rotation * Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }

        private void Bind(VRBindable pickup)
        {
            if (!hand.ignoreLastBindingCollision)
            {
                Utility.IgnoreCollision(pickup.gameObject, hand.gameObject, true);
                hand.ignoreLastBindingCollision = true;
            }

            ActiveBinding = pickup.CreateBinding(throwForceScale);
        }

        private VRBindable GetPointingAt()
        {
            float getAngle(Component pickup) => Vector3.Angle(pickup.transform.position - hand.PointRef.position,
                hand.PointRef.forward);

            VRBindable best = null;
            foreach (var other in VRBindable.All)
            {
                if (existingPointBindings.Contains(other)) continue;
                
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
            DetachPointGrab(false);

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
            Utility.IgnoreCollision(pointGrab.gameObject, hand.gameObject, true);
        }
    }
}