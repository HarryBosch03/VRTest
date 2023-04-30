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
        [SerializeField] private float attractionForce = 5.0f;

        private InputAction
            grabAction,
            attractAction;

        private VRPickup.AnchorBinding currentBinding;

        private Vector3 position;
        private Quaternion rotation;
        private new Collider collider;

        private LineRenderer lines;

        private static readonly List<Type> CollisionIgnoreComponents = new()
        {
            typeof(PlayerHand),
        };

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

            grabAction = Action("gripPressed", OnGrab);
            attractAction = Action("primaryButton", OnAttract);

            lines = GetComponentInChildren<LineRenderer>();
        }

        private void FixedUpdate()
        {
            var target = transform.parent;
            MoveTo(target.position, target.rotation);

            if (lines.enabled)
            {
                var ray = new Ray(transform.position, -transform.up);
                var p = Physics.Raycast(ray, out var hit) ? hit.point : ray.GetPoint(100.0f);
                lines.SetPosition(1, transform.InverseTransformPoint(p));
            }
        }

        private void LateUpdate()
        {
            transform.position = position;
            transform.rotation = rotation;
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
            if (currentBinding != null && hit.transform.IsChildOf(currentBinding.pickup.transform)) return true;

            return false;
        }

        private void CheckPosition()
        {
            for (var i = 0; i < 6; i++)
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

                    offset += direction * depth;
                    collisions++;
                }

                if (collisions == 0) return;

                position += offset;
            }
        }

        private void MoveTo(Vector3 target, Quaternion rotation)
        {
            var offset = target - position;
            const float step = 0.02f;

            for (var p = 0.0f; p <= 1.0f + step / 2.0f; p += step)
            {
                position += offset * step;
                CheckPosition();
            }
        }

        public enum Chirality
        {
            Left,
            Right,
        }

        private void Bind(VRPickup pickup)
        {
            var handle = pickup.GetClosestAnchor(transform.position);
            currentBinding = new VRPickup.AnchorBinding(pickup, transform, handle);
            pickup.ActiveBinding = currentBinding;
        }

        private void OnGrab(bool state)
        {
            currentBinding?.Throw(throwForceScale);

            if (!state) return;

            var pickup = VRPickup.GetPickup(e => 1.0f / (e.transform.position - transform.position).magnitude,
                1.0f / pickupRange);

            if (!pickup)
            {
                if (attractAction.ReadValue<float>() < 0.5f) return;
                
                var ray = new Ray(transform.position, -transform.up);
                if (!Physics.Raycast(ray, out var hit)) return;
                pickup = hit.transform.GetComponentInParent<VRPickup>();
            }
            if (!pickup) return;

            Bind(pickup);
        }
        
        private void OnAttract(bool state)
        {
            lines.enabled = state;
        }
    }
}