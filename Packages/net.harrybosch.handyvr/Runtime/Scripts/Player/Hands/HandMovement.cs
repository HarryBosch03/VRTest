using UnityEngine;

namespace HandyVR.Player.Hands
{
    /// <summary>
    /// Subclass for PlayerHand that manages the movement and collision.
    /// </summary>
    [System.Serializable]
    public class HandMovement
    {
        [SerializeField] [Range(0.0f, 1.0f)] private float rumbleMagnitude;
        [SerializeField] [Range(0.0f, 1.0f)] private float forceScaling = 1.0f;

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
            rb.mass = 0.01f;
        }

        public void MoveTo(Vector3 newPosition, Quaternion newRotation)
        {
            // If the hand has a binding, just teleport hand to tracked position, the
            // bound object will have their own collision.
            var rb = hand.Rigidbody;
            if (hand.ActiveBinding)
            {
                rb.isKinematic = true;
                hand.transform.position = newPosition;
                hand.transform.rotation = newRotation;
                return;
            }

            CheckIgnoreLastBindingCollision();

            rb.isKinematic = false;
            rb.centerOfMass = Vector3.zero;

            // Add a force that effectively cancels out the current velocity, and translates the hand to the target position.
            // Using a force instead is purely for collision and stability, MovePosition ended up causing horrific desync.
            var force = (newPosition - rb.position) / Time.deltaTime - rb.velocity;
            rb.AddForce(force * forceScaling, ForceMode.VelocityChange);

            // Do the same with a torque, match the current target rotation.
            var delta = newRotation * Quaternion.Inverse(rb.rotation);
            delta.ToAngleAxis(out var angle, out var axis);
            var torque = axis * (angle * Mathf.Deg2Rad / Time.deltaTime) - rb.angularVelocity;
            rb.AddTorque(torque * forceScaling, ForceMode.VelocityChange);
        }

        public void OnCollision(Collision collision)
        {
            hand.Input.Rumble(rumbleMagnitude, 0.0f);
        }

        /// <summary>
        /// Checks if the last thing we held was colliding with this hand, but isn't this frame,
        /// if so, stop ignoring its collision.
        /// </summary>
        private void CheckIgnoreLastBindingCollision()
        {
            // Skip if the hand is inactive, band aid fix for order of operations.
            // if the hand is still inactive from holding the object, it will pass as,
            // "not colling" and re-enable collision, causing the held object to loose,
            // its thrown velocity by colliding with the hand on its way out.
            if (!hand.handModel.gameObject.activeInHierarchy) return;

            // Check we have a last bound object, and we are actually ignoring its collision.
            if (!hand.ignoreLastBindingCollision) return;
            if (hand.ActiveBinding == null) return;
            if (!hand.ActiveBinding.bindable) return;

            // Loop through all colliders in the held object and the hand. If one of them are
            // intersecting, they are intersecting and we continue to ignore the collision.
            var collided = false;
            var listA = hand.Colliders;
            var listB = hand.ActiveBinding.bindable.GetComponentsInChildren<Collider>();
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
            Utility.IgnoreCollision(hand.ActiveBinding.bindable.gameObject, hand.gameObject, false);
        }
    }
}