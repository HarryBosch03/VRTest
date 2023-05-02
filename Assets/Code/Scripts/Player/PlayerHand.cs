using System;
using System.Collections.Generic;
using Code.Scripts.Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PlayerHand : MonoBehaviour
    {
        [SerializeField] private Chirality chirality;
        [SerializeField] private float handCollisionSize = 0.05f;
        [SerializeField] private float pickupRange = 0.2f;
        [SerializeField] private float throwForceScale = 1.0f;
        [SerializeField] private float detachedBindingAcceleration = 5.0f;

        private InputAction attractAction;

        private Transform pointRef;

        private readonly List<GameObject> lastCollisionIgnores = new();
        private readonly List<GameObject> collisionIgnores = new();

        private VRPickup.AnchorBinding currentBinding;
        private bool detachedBinding;
        private float detachedBindingSpeed;

        private Vector3 position, lastPosition;
        private Quaternion rotation, lastRotation;

        private Vector3 velocity;

        private new Collider collider;

        private LineRenderer lines;

        private static readonly List<Type> CollisionIgnoreComponents = new()
        {
            typeof(PlayerHand),
        };

        public Vector3 Forward => -transform.up;
        public Vector3 PointDirection => pointRef ? pointRef.forward : Forward;

        private InputAction Action(string binding, Action<bool> callback)
        {
            binding = "<XRController>{" + (chirality == Chirality.Left ? "LeftHand" : "RightHand") + "}/" + binding;
            var action = new InputAction(binding: binding, interactions: "Press(behavior=2)");
            action.Enable();
            if (callback != null) action.performed += ctx => callback(ctx.ReadValueAsButton());
            return action;
        }

        private void Awake()
        {
            var collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = handCollisionSize;
            collider.isTrigger = true;
            this.collider = collider;

            Action("gripPressed", OnGrab);
            attractAction = Action("primaryButton", OnAttract);

            lines = GetComponentInChildren<LineRenderer>();

            pointRef = transform.DeepFind("Point Ref");
        }

        private void FixedUpdate()
        {
            lastPosition = position;
            lastRotation = rotation;

            var target = transform.parent;
            MoveTo(target.position, target.rotation);

            if (lines.enabled)
            {
                var ray = new Ray(transform.position, PointDirection);
                var p = Physics.Raycast(ray, out var hit) ? hit.point : ray.GetPoint(100.0f);
                lines.SetPosition(1, transform.InverseTransformPoint(p));
            }
        }

        private void Update()
        {
            var t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

            transform.position = Vector3.Lerp(lastPosition, position, t);
            transform.rotation = Quaternion.Slerp(lastRotation, rotation, t);
        }

        private void LateUpdate()
        {
            UpdateBinding();
        }

        private void UpdateBinding()
        {
            if (!currentBinding) return;

            if (detachedBinding)
            {
                detachedBindingSpeed += detachedBindingAcceleration * Time.deltaTime;
                currentBinding.Position += (position - currentBinding.Position).normalized * (detachedBindingSpeed * Time.deltaTime);
                if ((currentBinding.Position - position).magnitude < detachedBindingSpeed * Time.deltaTime * 1.1f)
                {
                    detachedBinding = false;
                }
                
                return;
            }

            currentBinding.Position = position;
            currentBinding.Rotation = rotation;
        }

        private static bool IgnoreComponent(Component other)
        {
            foreach (var type in CollisionIgnoreComponents)
            {
                if (other.GetComponentInParent(type)) return true;
            }

            return false;
        }

        public bool FilterHit(Component hit)
        {
            if (hit.transform.IsChildOf(transform)) return true;
            if (IgnoreComponent(hit)) return true;
            if (currentBinding && hit.transform.IsChildOf(currentBinding.pickup.transform)) return true;

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

        private void CheckPosition(List<Rigidbody> dirtyList)
        {
            var bounds = collider.bounds;
            var broadPhase = Physics.OverlapBox(bounds.center - transform.position + position,
                bounds.extents + Vector3.one * 0.1f, rotation);

            var offset = Vector3.zero;
            var collisions = 0;
            foreach (var other in broadPhase)
            {
                if (FilterHit(other)) continue;

                if (!Physics.ComputePenetration(collider, position, rotation, other, other.transform.position,
                        other.transform.rotation, out var direction, out var depth)) continue;

                if (IsOnIgnoreList(other.transform)) continue;

                if (other.attachedRigidbody && !other.attachedRigidbody.isKinematic)
                {
                    if (dirtyList.Contains(other.attachedRigidbody)) continue;

                    other.transform.position -= direction * depth;

                    var p = position - direction * handCollisionSize;
                    var relVelocity = other.attachedRigidbody.velocity - velocity;
                    var dot = Vector3.Dot(direction, relVelocity);
                    if (dot < 0.0f) continue;
                    var force = -direction * dot;
                    other.attachedRigidbody.AddForceAtPosition(force, p, ForceMode.VelocityChange);

                    dirtyList.Add(other.attachedRigidbody);
                }
                else
                {
                    offset += direction * depth;
                    collisions++;
                }
            }

            if (collisions == 0) return;

            position += offset;
        }

        private void MoveTo(Vector3 target, Quaternion rotation)
        {
            this.rotation = rotation;
            velocity = (target - position) / Time.deltaTime;

            var offset = target - position;
            const float step = 0.02f;

            lastCollisionIgnores.Clear();
            lastCollisionIgnores.AddRange(collisionIgnores);
            collisionIgnores.Clear();

            var dirtyList = new List<Rigidbody>();

            for (var p = 0.0f; p <= 1.0f + step / 2.0f; p += step)
            {
                position += offset * step;
                CheckPosition(dirtyList);
            }
        }

        public enum Chirality
        {
            Left,
            Right,
        }

        private void Bind(VRPickup pickup, bool detached)
        {
            var handle = pickup.GetClosestAnchor(transform.position);
            currentBinding = new VRPickup.AnchorBinding(pickup, handle);
            detachedBinding = detached;
            pickup.ActiveBinding = currentBinding;
        }

        private void OnGrab(bool state)
        {
            if (currentBinding != null)
            {
                currentBinding.Throw(throwForceScale);
                collisionIgnores.Add(currentBinding.pickup.gameObject);
                detachedBindingSpeed = 0.0f;
            }

            if (!state) return;

            var pickup = VRPickup.GetPickup(e => 1.0f / (e.transform.position - transform.position).magnitude,
                1.0f / pickupRange);

            if (!pickup)
            {
                if (attractAction.ReadValue<float>() < 0.5f) return;

                var ray = new Ray(transform.position, PointDirection);
                if (!Physics.Raycast(ray, out var hit)) return;
                pickup = hit.transform.GetComponentInParent<VRPickup>();
                if (!pickup) return;
                Bind(pickup, true);
                return;
            }

            Bind(pickup, false);
        }

        private void OnAttract(bool state)
        {
            lines.enabled = state;
        }
    }
}