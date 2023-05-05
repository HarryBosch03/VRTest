using UnityEngine;

namespace Player
{
    public partial class PlayerHand
    {
        private void InitializeMovement()
        {
            colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders) collider.isTrigger = true;
        }
        
        private void Move()
        {
            lastPosition = position;
            lastRotation = rotation;

            var target = transform.parent;

            transform.rotation = target.rotation;
            MoveTo(target.position);
            position = transform.position;
        }
        
        public bool FilterHit(Component hit)
        {
            if (hit.transform.IsChildOf(transform)) return true;
            if (currentBinding && hit.transform.IsChildOf(currentBinding.bindable.transform)) return true;

            return false;
        }

        public bool IsOnIgnoreList(Transform other)
        {
            foreach (var ignore in lastCollisionIgnores)
            {
                if (!other.IsChildOf(ignore.transform)) continue;
                collisionIgnores.Add(ignore);
                return true;
            }

            return false;
        }

        private void MoveTo(Vector3 position)
        {
            lastCollisionIgnores.Clear();
            lastCollisionIgnores.AddRange(collisionIgnores);
            collisionIgnores.Clear();

            if (colliders.Length == 0) return;

            var bounds = colliders[0].bounds;
            for (var i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            bounds.Expand(0.1f);

            var vec = position - transform.position;
            const float step = 0.05f;
            for (var p = 0.0f; p <= 1.0000000001f; p += step)
            {
                var collided = false;
                var backstep = 0.0f;
                var broadPhase = Physics.OverlapBox(bounds.center, bounds.extents, rotation);
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
                        transform.position += direction * depth;
                    }
                }

                if (collided)
                {
                    transform.position -= (position - transform.position).normalized * backstep;
                }
                transform.position += vec * step;
            }
        }
        
        private void MovementInterpolation()
        {
            var t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            transform.position = Vector3.Lerp(lastPosition, position, t);
            transform.rotation = Quaternion.Slerp(lastRotation, rotation, t);
        }
    }
}