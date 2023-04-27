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
        private const float CollisionStepDistance = 0.05f;

        [SerializeField] private Chirality chirality;
        [SerializeField] private float handCollisionSize;
        [SerializeField] private float pickupRange;

        private InputAction
            grabAction;

        private VRPickup.AnchorBinding currentBinding;

        private Vector3 position;
        private Quaternion rotation;
        private Collider collider;

        private static readonly List<System.Type> CollisionIgnoreComponents = new List<System.Type>()
        {
            typeof(PlayerHand),
        };

        private InputAction Action(string binding, Action<bool> callback)
        {
            binding = "<XRController>{" + (chirality == Chirality.Left ? "LeftHand" : "RightHand") + "}/" + binding;
            var action = new InputAction(binding: binding, interactions: "Press(behavior=2)");
            action.Enable();
            action.performed += ctx => callback(ctx.ReadValueAsButton());
            return action;
        }

        private void Awake()
        {
            var collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = handCollisionSize;
            collider.isTrigger = true;
            this.collider = collider;
            
            grabAction = Action("gripPressed", OnGrab);
        }

        private void Update()
        {
            var target = transform.parent;
            MoveTo(target.position, target.rotation);

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

        private bool CheckPosition()
        {
            var res = false;
            for (var i = 0; i < 6; i++)
            {
                var bounds = collider.bounds;
                var broadPhase = Physics.OverlapBox(bounds.center - transform.position + position,
                    bounds.extents + Vector3.one * 0.1f, rotation);

                var offset = Vector3.zero;
                var collisions = 0;
                foreach (var other in broadPhase)
                {
                    if (other.transform.IsChildOf(transform)) continue;
                    if (IgnoreComponent(other)) continue;

                    if (!Physics.ComputePenetration(collider, position, rotation, other, other.transform.position,
                            other.transform.rotation, out var direction, out var depth)) continue;

                    offset += direction * depth;
                    collisions++;
                }

                if (collisions == 0) return res;
                
                Debug.Log(collisions);
            
                position += offset;
                res = true;
            }
            return res;
        }

        private void MoveTo(Vector3 target, Quaternion rotation)
        {
            this.rotation = rotation;
            do
            {
                position += (target - position).normalized * CollisionStepDistance;
                if ((target - position).sqrMagnitude < CollisionStepDistance * CollisionStepDistance) position = target;
                if (CheckPosition()) return;
            } while ((target - position).sqrMagnitude > CollisionStepDistance * CollisionStepDistance);
        }

        public enum Chirality
        {
            Left,
            Right,
        }

        private void OnGrab(bool state)
        {
            var renderer = GetComponentInChildren<Renderer>();
            renderer.material.color = state ? Color.green : Color.red;
            
            currentBinding?.Clear();
            if (!state) return;
            
            var pickup = VRPickup.GetPickup(e => 1.0f / (e.transform.position - transform.position).magnitude, 1.0f / pickupRange);
            if (!pickup) return;

            var handle = pickup.GetClosestAnchor(transform.position);
            if (!handle) return;
            currentBinding = new VRPickup.AnchorBinding(transform, handle);
            pickup.ActiveBinding = currentBinding;
        }
    }
}