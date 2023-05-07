using UnityEngine;

namespace Player.Hands
{
    [System.Serializable]
    public class HandMovement
    {
        private PlayerHand hand;
        
        public void Init(PlayerHand hand)
        {
            this.hand = hand;
            foreach (var collider in hand.Colliders)
            {
                collider.isTrigger = false;
            }

            var rb = hand.Rigidbody;
            rb.useGravity = false;
            rb.angularDrag = 0.0f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.mass = 0.05f;
        }

        public void MoveTo(Vector3 newPosition, Quaternion newRotation)
        {
            var rb = hand.Rigidbody;
            if (hand.activeBinding)
            {
                rb.isKinematic = true;
                hand.transform.position = newPosition;
                hand.transform.rotation = newRotation;
                return;
            }

            CheckIgnoreLastBindingCollision();

            rb.isKinematic = false;
            rb.centerOfMass = Vector3.zero;

            var force = (newPosition - rb.position) / Time.deltaTime - rb.velocity;
            rb.AddForce(force, ForceMode.VelocityChange);

            var delta = newRotation * Quaternion.Inverse(rb.rotation);
            delta.ToAngleAxis(out var angle, out var axis);
            var torque = axis * (angle * Mathf.Deg2Rad / Time.deltaTime)- rb.angularVelocity;
            rb.AddTorque(torque , ForceMode.VelocityChange);
        }

        private void CheckIgnoreLastBindingCollision()
        {
            if (!hand.handModel.gameObject.activeInHierarchy) return;
            
            if (!hand.ignoreLastBindingCollision) return;
            if (hand.activeBinding == null) return;
            if (!hand.activeBinding.bindable) return;

            if (!hand.handModel.gameObject.activeInHierarchy) return;
            
            var collided = false;
            var listA = hand.Colliders;
            var listB = hand.activeBinding.bindable.GetComponentsInChildren<Collider>();
            foreach (var a in listA)
            {
                foreach (var b in listB)
                {
                    if (!Physics.ComputePenetration(a, a.transform.position, a.transform.rotation,
                            b, b.transform.position, b.transform.rotation,
                            out _, out _)) continue;

                    collided = true;
                    break;
                }
                if (collided) break;
            }
            
            
            if (collided) return;

            hand.ignoreLastBindingCollision = false;
            Utility.IgnoreCollision(hand.activeBinding.bindable.gameObject, hand.gameObject, false);
        }
    }
}