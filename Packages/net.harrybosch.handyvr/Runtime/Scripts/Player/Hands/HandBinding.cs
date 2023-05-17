using System.Collections.Generic;
using HandyVR.Interactions;
using UnityEngine;

namespace HandyVR.Player.Hands
{
    [System.Serializable]
    public class HandBinding
    {
        [SerializeField] private float pickupRange = 0.2f;
        [SerializeField] private float detachedBindingAngle;
        [SerializeField] private float detachedBindingForce = 15.0f;
        
        private PlayerHand hand;
        private LineRenderer lines;

        private float detachedBindingSpeed;

        private Vector3 preBindingHandPosition;

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
            lines.enabled = false;

            hand.Input.Grip.ChangedThisFrame(OnGrip);

            if (hand.ActiveBinding)
            {
                UpdateActiveBinding();
                return;
            }

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

            var rb = DetachedBinding.Rigidbody;
            var dir = hand.PointRef.position - rb.position;
            var distance = dir.magnitude;
            dir /= distance;

            detachedBindingSpeed += detachedBindingForce * Time.deltaTime;

            if (distance < detachedBindingSpeed * Time.deltaTime)
            {
                RemoveDetachedBinding(true);
                return;
            }
            
            var force = dir * detachedBindingSpeed - rb.velocity;
            rb.AddForce(force, ForceMode.VelocityChange);
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

            preBindingHandPosition = pickup.transform.InverseTransformPoint(hand.Rigidbody.position);
            ActiveBinding = pickup.CreateBinding();
            UpdateActiveBinding();
            
        }

        private VRBindable GetPointingAt()
        {
            float getScore(VRBindable bindable)
            {
                if (!CanSee(bindable)) return -1.0f;
                
                var d1 = (bindable.transform.position - hand.PointRef.position).normalized;
                var d2 = hand.PointRef.forward;
                return 1.0f / (Mathf.Acos(Vector3.Dot(d1, d2)) * Mathf.Rad2Deg);
            }

            return Utility.Best(VRBindable.All, getScore, 1.0f / (detachedBindingAngle * 2.0f));
        }

        private bool CanSee(VRBindable other)
        {
            var ray = new Ray(hand.PointRef.position, other.transform.position - hand.PointRef.position);
            return Physics.Raycast(ray, out var hit) && hit.transform.IsChildOf(other.transform);
        }

        private void OnGrip(bool state)
        {
            DeactivateBinding();
            RemoveDetachedBinding(false);

            if (state) OnGripPressed();
        }

        private void DeactivateBinding()
        {
            if (!ActiveBinding) return;
            
            ActiveBinding.Deactivate();
            hand.Rigidbody.position = ActiveBinding.bindable.transform.TransformPoint(preBindingHandPosition);
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
            detachedBindingSpeed = 0.0f;
            existingDetachedBindings.Add(DetachedBinding);

            Utility.IgnoreCollision(DetachedBinding.gameObject, hand.gameObject, true);
        }
    }
}