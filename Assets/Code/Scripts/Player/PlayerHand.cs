using System;
using System.Collections.Generic;
using Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PlayerHand : MonoBehaviour
    {
        [SerializeField] private Chirality chirality;
        [SerializeField] private float pickupRange = 0.2f;
        [SerializeField] private float throwForceScale = 1.0f;
        [SerializeField] private float detachedBindingAcceleration = 5.0f;

        [Space] [SerializeField] private Chirality defaultHandModelChirality;

        [SerializeField] private Vector3 flipAxis = Vector3.right;

        private InputAction attractAction;

        private Transform pointRef;
        private Transform handModel;

        private readonly List<GameObject> lastCollisionIgnores = new();
        private readonly List<GameObject> collisionIgnores = new();

        private VRBinding currentBinding;
        private bool detachedBinding;
        private float detachedBindingSpeed;

        private Vector3 position, lastPosition;
        private Quaternion rotation, lastRotation;

        private Collider[] colliders;

        private LineRenderer lines;

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
            colliders = GetComponentsInChildren<Collider>();

            Action("gripPressed", OnGrab);
            attractAction = Action("primaryButton", OnAttract);

            lines = GetComponentInChildren<LineRenderer>();

            pointRef = transform.DeepFind("Point Ref");

            handModel = transform.DeepFind("Model");
            if (chirality != defaultHandModelChirality)
                handModel.localScale = Vector3.Reflect(Vector3.one, flipAxis.normalized);
        }

        private void FixedUpdate()
        {
            lastPosition = position;
            lastRotation = rotation;

            var target = transform.parent;

            transform.rotation = target.rotation;
            MoveTo(target.position);
            position = transform.position;

            if (lines.enabled)
            {
                var ray = new Ray(transform.position, PointDirection);
                var p = Physics.Raycast(ray, out var hit) ? hit.point : ray.GetPoint(100.0f);
                lines.SetPosition(1, transform.InverseTransformPoint(p));
            }
        }

        private void Update()
        {
            if (currentBinding && currentBinding.bindable is VRHandle handle)
            {
                transform.position = handle.HandPosition;
                transform.rotation = handle.HandRotation;
                return;
            }

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

        public enum Chirality
        {
            Left,
            Right,
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
    }
}