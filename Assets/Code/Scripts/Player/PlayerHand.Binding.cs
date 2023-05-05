using Interactions;
using UnityEngine;

namespace Player
{
    public partial class PlayerHand
    {
        private void InitializeBinding()
        {
            Action("gripPressed", OnGrab);
            attractAction = Action("primaryButton", OnAttract);

            lines = GetComponentInChildren<LineRenderer>();

            pointRef = transform.DeepFind("Point Ref");
        }

        private bool MoveHandToHandleBinding()
        {
            if (!currentBinding || currentBinding.bindable is not VRHandle handle) return false;
            
            transform.position = handle.HandPosition;
            transform.rotation = handle.HandRotation;
            return true;
        }
        
        private void UpdateBinding()
        {
            if (!currentBinding) return;

            if (detachedBinding)
            {
                detachedBindingSpeed += detachedBindingAcceleration * Time.deltaTime;
                currentBinding.Position += (position - currentBinding.Position).normalized *
                                           (detachedBindingSpeed * Time.deltaTime);
                if ((currentBinding.Position - position).magnitude < detachedBindingSpeed * Time.deltaTime * 1.1f)
                {
                    detachedBinding = false;
                }

                return;
            }

            currentBinding.Position = position;
            currentBinding.Rotation = rotation;
        }
        
        private void Bind(VRBindable pickup, bool detached)
        {
            if (detached && !pickup.CanCreateDetachedBinding) return;

            var handle = pickup.GetClosestAnchor(transform.position);
            currentBinding = new VRBinding(pickup, this, handle);
            detachedBinding = detached;
            pickup.ActiveBinding = currentBinding;
        }

        private void OnGrab(bool state)
        {
            if (currentBinding != null)
            {
                currentBinding.Throw(throwForceScale);
                collisionIgnores.Add(currentBinding.bindable.gameObject);
                detachedBindingSpeed = 0.0f;
            }

            if (!state) return;

            VRBindable pickup;
            if (attractAction.ReadValue<float>() > 0.5f)
            {
                var ray = new Ray(transform.position, PointDirection);
                if (!Physics.Raycast(ray, out var hit)) return;
                pickup = hit.transform.GetComponentInParent<VRBindable>();
                if (!pickup) return;
                Bind(pickup, true);
            }
            else
            {
                pickup = VRBindable.GetPickup(e => 1.0f / (e.transform.position - transform.position).magnitude,
                    1.0f / pickupRange);
                if (!pickup) return;
                Bind(pickup, false);
            }
        }

        private void OnAttract(bool state)
        {
            lines.enabled = state;
        }
        
        private void UpdateLines()
        {
            if (!lines.enabled) return;
            
            var ray = new Ray(transform.position, PointDirection);
            var p = Physics.Raycast(ray, out var hit) ? hit.point : ray.GetPoint(100.0f);
            lines.SetPosition(1, transform.InverseTransformPoint(p));
        }
    }
}