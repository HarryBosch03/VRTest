using Interactions;
using UnityEngine;

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

        public bool DetachedBinding { get; private set; }
        private float detachedBindingSpeed;

        public void Init(PlayerHand hand)
        {
            this.hand = hand;

            lines = hand.GetComponentInChildren<LineRenderer>();
        }

        public void UpdateBinding()
        {
            hand.gripAction.WasPressedThisFrame(OnGrip);

            if (!hand.currentBinding) return;

            if (DetachedBinding)
            {
                detachedBindingSpeed += detachedBindingAcceleration * Time.deltaTime;
                hand.currentBinding.Position += (hand.transform.position - hand.currentBinding.Position).normalized *
                                                (detachedBindingSpeed * Time.deltaTime);
                if ((hand.currentBinding.Position - hand.transform.position).magnitude <
                    detachedBindingSpeed * Time.deltaTime * 1.1f)
                {
                    DetachedBinding = false;
                }

                return;
            }

            hand.currentBinding.Position = hand.transform.position;
            hand.currentBinding.Rotation = hand.transform.rotation;
        }

        private void Bind(VRBindable pickup, bool detached)
        {
            if (detached && !pickup.CanCreateDetachedBinding) return;

            if (!hand.ignoreLastBindingCollision)
            {
                Utility.IgnoreCollision(pickup.gameObject, hand.gameObject, true);
                hand.ignoreLastBindingCollision = true;
            }

            hand.currentBinding = pickup.CreateBinding(throwForceScale);
            DetachedBinding = detached;
        }

        private void OnGrip(bool state)
        {
            if (hand.currentBinding != null)
            {
                hand.currentBinding.Throw(throwForceScale);
                detachedBindingSpeed = 0.0f;
            }

            if (!state) return;

            VRBindable pickup;
            if (hand.attractAction.action.State())
            {
                var ray = new Ray(hand.PointRef.position, hand.PointRef.forward);
                if (!Physics.Raycast(ray, out var hit)) return;
                pickup = hit.transform.GetComponentInParent<VRBindable>();
                if (!pickup) return;
                Bind(pickup, true);
            }
            else
            {
                pickup = VRBindable.GetPickup(hand.transform.position, pickupRange);
                if (!pickup) return;
                Bind(pickup, false);
            }
        }

        public void UpdateLines()
        {
            lines.enabled = !hand.currentBinding && hand.attractAction.action.State();
            if (!lines.enabled) return;

            var ray = new Ray(hand.PointRef.position, hand.PointRef.forward);
            var p = Physics.Raycast(ray, out var hit) ? hit.point : ray.GetPoint(100.0f);
            lines.SetPosition(1, hand.transform.InverseTransformPoint(p));
        }
    }
}