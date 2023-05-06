using UnityEngine;

namespace Player.Hands
{
    [System.Serializable]
    public class HandMovement
    {
        [SerializeField] private float forgetRange;
        
        private Rigidbody rigidbody;

        private PlayerHand hand;
        
        public void Init(PlayerHand hand)
        {
            this.hand = hand;
            foreach (var collider in hand.Colliders)
            {
                collider.isTrigger = false;
            }

            rigidbody = hand.gameObject.GetOrAddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.angularDrag = 0.0f;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.maxAngularVelocity = Mathf.Infinity;
        }

        public void MoveTo(Vector3 newPosition, Quaternion newRotation)
        {
            if (hand.currentBinding)
            {
                rigidbody.isKinematic = true;
                hand.transform.position = newPosition;
                hand.transform.rotation = newRotation;
                return;
            }

            CheckIgnoreLastBindingCollision();

            rigidbody.isKinematic = false;

            rigidbody.isKinematic = false;
            rigidbody.centerOfMass = Vector3.zero;

            var force = (newPosition - rigidbody.position) / Time.deltaTime - rigidbody.velocity;
            rigidbody.AddForce(force, ForceMode.VelocityChange);

            var delta = newRotation * Quaternion.Inverse(rigidbody.rotation);
            delta.ToAngleAxis(out var angle, out var axis);
            var torque = axis * (angle * Mathf.Deg2Rad / Time.deltaTime)- rigidbody.angularVelocity;
            rigidbody.AddTorque(torque , ForceMode.VelocityChange);
        }

        private void CheckIgnoreLastBindingCollision()
        {
            if (!hand.handModel.gameObject.activeInHierarchy) return;
            
            if (!hand.ignoreLastBindingCollision) return;
            if (hand.currentBinding == null) return;
            if (!hand.currentBinding.bindable) return;
            
            var collided = false;
            var queryList = Physics.OverlapSphere(hand.transform.position, forgetRange);
            foreach (var query in queryList)
            {
                if (!query.transform.IsChildOf(hand.currentBinding.bindable.transform)) continue;

                collided = true;
                break;
            }
            
            if (collided) return;

            hand.ignoreLastBindingCollision = false;
            Utility.IgnoreCollision(hand.currentBinding.bindable.gameObject, hand.gameObject, false);
        }
    }
}