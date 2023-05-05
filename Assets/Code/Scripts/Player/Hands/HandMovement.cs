using System.Collections.Generic;
using UnityEngine;

namespace Player.Hands
{
    [System.Serializable]
    public class HandMovement
    {
        private PlayerHand hand;
        
        private Collider[] colliders;
        
        private readonly List<GameObject> lastCollisionIgnores = new();

        public void Init(PlayerHand hand)
        {
            this.hand = hand;
            
            colliders = hand.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders) collider.isTrigger = true;
        }

        public void Move()
        {
            hand.lastPosition = hand.position;
            hand.lastRotation = hand.rotation;

            var target = hand.transform.parent;

            hand.transform.rotation = target.rotation;
            hand.rotation = target.rotation;
            MoveTo(target.position);
            hand.position = hand.transform.position;
        }
        
        public bool FilterHit(Component hit)
        {
            if (hit.transform.IsChildOf(hand.transform)) return true;
            if (hand.currentBinding && hit.transform.IsChildOf(hand.currentBinding.bindable.transform)) return true;

            return false;
        }

        public bool IsOnIgnoreList(Transform other)
        {
            foreach (var ignore in lastCollisionIgnores)
            {
                if (!other.IsChildOf(ignore.transform)) continue;
                hand.collisionIgnores.Add(ignore);
                return true;
            }

            return false;
        }

        private void MoveTo(Vector3 position)
        {
            lastCollisionIgnores.Clear();
            lastCollisionIgnores.AddRange(hand.collisionIgnores);
            hand.collisionIgnores.Clear();

            if (colliders.Length == 0) return;

            var bounds = colliders[0].bounds;
            for (var i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            bounds.Expand(0.1f);

            var vec = position - hand.transform.position;
            const float step = 0.05f;
            for (var p = 0.0f; p <= 1.0000000001f; p += step)
            {
                var collided = false;
                var backstep = 0.0f;
                var broadPhase = Physics.OverlapBox(bounds.center, bounds.extents, hand.rotation);
                foreach (var other in broadPhase)
                {
                    foreach (var collider in colliders)
                    {
                        if (FilterHit(other)) continue;

                        if (!Physics.ComputePenetration(collider, collider.transform.position,
                                collider.transform.rotation,
                                other, other.transform.position, other.transform.rotation,
                                out var direction, out var depth)) continue;

                        if (IsOnIgnoreList(other.transform)) continue;

                        if (collider.attachedRigidbody)
                        {
                            collider.attachedRigidbody.position -= direction * depth;
                            collider.attachedRigidbody.AddForce(-direction * depth / Time.deltaTime, ForceMode.VelocityChange);
                            continue;
                        }
                        
                        collided = true;
                        backstep = Vector3.Dot(-vec.normalized, direction) * depth;
                        hand.transform.position += direction * depth;
                    }
                }

                if (collided)
                {
                    hand.transform.position -= (position - hand.transform.position).normalized * backstep;
                }
                hand.transform.position += vec * step;
            }
        }

        public void MovementInterpolation()
        {
            var t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            hand.transform.position = Vector3.Lerp(hand.lastPosition, hand.position, t);
            hand.transform.rotation = Quaternion.Slerp(hand.lastRotation, hand.rotation, t);
        }
    }
}