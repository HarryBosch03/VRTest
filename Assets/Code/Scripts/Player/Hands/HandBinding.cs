using Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Hands
{
    [System.Serializable]
    public class HandBinding
    {
        [SerializeField] private float pickupRange = 0.2f;
        [SerializeField] private float throwForceScale = 1.0f;
        [SerializeField] private float detachedBindingAcceleration = 5.0f;

        private PlayerHand hand;
        
        private LineRenderer lines;

        private bool detachedBinding;
        private float detachedBindingSpeed;

        public void Init(PlayerHand hand)
        {
            this.hand = hand;
            
            lines = hand.GetComponentInChildren<LineRenderer>();
        }

        public bool MoveHandToHandleBinding()
        {
            if (!hand.currentBinding || hand.currentBinding.bindable is not VRHandle handle) return false;
            
            hand.transform.position = handle.HandPosition;
            hand.transform.rotation = handle.HandRotation;
            return true;
        }

        public void UpdateBinding()
        {
            hand.gripAction.WasPressedThisFrame(OnGrip);
            hand.attractAction.WasPressedThisFrame(OnAttract);
            
            if (!hand.currentBinding) return;

            if (detachedBinding)
            {
                detachedBindingSpeed += detachedBindingAcceleration * Time.deltaTime;
                hand.currentBinding.Position += (hand.position - hand.currentBinding.Position).normalized *
                                                (detachedBindingSpeed * Time.deltaTime);
                if ((hand.currentBinding.Position - hand.position).magnitude < detachedBindingSpeed * Time.deltaTime * 1.1f)
                {
                    detachedBinding = false;
                }

                return;
            }

            hand.currentBinding.Position = hand.position;
            hand.currentBinding.Rotation = hand.rotation;
        }
        
        private void Bind(VRBindable pickup, bool detached)
        {
            if (detached && !pickup.CanCreateDetachedBinding) return;

            var handle = pickup.GetClosestAnchor(hand.transform.position);
            hand.currentBinding = new VRBinding(pickup, hand, throwForceScale);
            detachedBinding = detached;
            pickup.ActiveBinding = hand.currentBinding;
        }

        private void OnGrip(bool state)
        {
            if (hand.currentBinding != null)
            {
                hand.currentBinding.Throw(throwForceScale);
                hand.collisionIgnores.Add(hand.currentBinding.bindable.gameObject);
                detachedBindingSpeed = 0.0f;
            }

            if (!state) return;

            VRBindable pickup;
            if (hand.attractAction.action.State())
            {
                var ray = new Ray(hand.transform.position, hand.PointDirection);
                if (!Physics.Raycast(ray, out var hit)) return;
                pickup = hit.transform.GetComponentInParent<VRBindable>();
                if (!pickup) return;
                Bind(pickup, true);
            }
            else
            {
                pickup = VRBindable.GetPickup(e => 1.0f / (e.transform.position - hand.transform.position).magnitude,
                    1.0f / pickupRange);
                if (!pickup) return;
                Bind(pickup, false);
            }
        }

        private void OnAttract(bool state)
        {
            lines.enabled = state;
        }

        public void UpdateLines()
        {
            if (!lines.enabled) return;
            
            var ray = new Ray(hand.transform.position, hand.PointDirection);
            var p = Physics.Raycast(ray, out var hit) ? hit.point : ray.GetPoint(100.0f);
            lines.SetPosition(1, hand.transform.InverseTransformPoint(p));
        }
    }
}