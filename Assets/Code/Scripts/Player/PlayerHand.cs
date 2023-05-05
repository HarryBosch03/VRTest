using System;
using System.Collections.Generic;
using Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed partial class PlayerHand : MonoBehaviour
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
            InitializeMovement();
            InitializeBinding();

            handModel = transform.DeepFind("Model");
            if (chirality != defaultHandModelChirality)
                handModel.localScale = Vector3.Reflect(Vector3.one, flipAxis.normalized);
        }

        private void FixedUpdate()
        {
            Move();
            UpdateLines();
        }

        private void Update()
        {
            if (MoveHandToHandleBinding()) return;
            MovementInterpolation();
        }

        private void LateUpdate()
        {
            UpdateBinding();
        }

        public enum Chirality
        {
            Left,
            Right,
        }
    }
}