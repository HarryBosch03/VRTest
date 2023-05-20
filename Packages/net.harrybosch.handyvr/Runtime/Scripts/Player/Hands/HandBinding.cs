using System.Collections.Generic;
using HandyVR.Interactions;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace HandyVR.Player.Hands
{
    [System.Serializable]
    public class HandBinding
    {
        [SerializeField] private float pickupRange = 0.2f;
        [SerializeField] private float detachedBindingAngle;
        [SerializeField] private float detachedBindingForce = 400.0f;
        [SerializeField] private float detachedBindingMinForce = 50.0f;

        private PlayerHand hand;
        private LineRenderer lines;

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
            if (XRController.rightHand != null && ((OculusTouchControllerProfile.OculusTouchController)XRController.rightHand).secondaryButton.isPressed)
            {
                hand.Rigidbody.isKinematic = false;
            }

            PointingAt = null;
            lines.enabled = false;

            hand.Input.Grip.ChangedThisFrame(OnGrip);

            if (hand.ActiveBinding) return;

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
            var direction = hand.PointRef.position - rb.position;
            var distance = direction.magnitude;
            direction /= distance;

            var delta = detachedBindingForce * Time.deltaTime / rb.mass;
            if (distance < delta * Time.deltaTime)
            {
                RemoveDetachedBinding(true);
                return;
            }

            var force = direction * (delta * (distance + detachedBindingMinForce / detachedBindingForce)) - rb.velocity;
            rb.AddForce(force, ForceMode.VelocityChange);
        }

        private void RemoveDetachedBinding(bool bind)
        {
            if (!DetachedBinding) return;
            Utility.Physics.IgnoreCollision(DetachedBinding.gameObject, hand.gameObject, false);

            if (bind) Bind(DetachedBinding);

            existingDetachedBindings.Remove(DetachedBinding);
            DetachedBinding = null;
        }

        private void Bind(VRBindable pickup)
        {
            Utility.Physics.IgnoreCollision(pickup.gameObject, hand.gameObject, true);

            ActiveBinding = pickup.CreateBinding(() => hand.Target.position, () => hand.Target.rotation, () => hand.Flipped);
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

            return Utility.Collections.Best(VRBindable.All, getScore, 1.0f / (detachedBindingAngle * 2.0f));
        }

        private bool CanSee(VRBindable other)
        {
            var ray = new Ray(hand.PointRef.position, other.transform.position - hand.PointRef.position);
            return Physics.Raycast(ray, out var hit) && other.transform.IsChildOf(hit.transform);
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

            hand.handModel.gameObject.SetActive(true);

            hand.Rigidbody.isKinematic = false;
            hand.Rigidbody.velocity = Vector3.zero;
            hand.Rigidbody.angularVelocity = Vector3.zero;
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
            existingDetachedBindings.Add(DetachedBinding);

            Utility.Physics.IgnoreCollision(DetachedBinding.gameObject, hand.gameObject, true);
        }
    }
}