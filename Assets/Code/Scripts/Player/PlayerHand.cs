using System;
using System.Collections.Generic;
using Input;
using Interactions;
using Player.Hands;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PlayerHand : MonoBehaviour
    {
        [SerializeField] private Chirality chirality;

        [Space] 
        [SerializeField] private Chirality defaultHandModelChirality;
        [SerializeField] private Vector3 flipAxis = Vector3.right;
        
        [Space]
        [SerializeField] private HandMovement movement;
        [SerializeField] private HandBinding binding;
        [SerializeField] private HandAnimator animator;

        [HideInInspector] public Vector3 position, lastPosition;
        [HideInInspector] public Quaternion rotation, lastRotation;

        public readonly List<GameObject> collisionIgnores = new();

        public InputWrapper gripAction;
        public InputWrapper triggerAction;
        public InputWrapper attractAction;

        private Action updateInputs;
        
        private Transform pointRef;
        private Transform handModel;
        public VRBinding currentBinding;

        public Vector3 Forward => -transform.up;
        public Vector3 PointDirection => pointRef ? pointRef.forward : Forward;

        public static Action<InputAction.CallbackContext> Switch(System.Action<bool> callback) => ctx => callback(ctx.ReadValueAsButton());
        
        public InputWrapper CreateAction(string binding)
        {
            binding = "<XRController>{" + (chirality == Chirality.Left ? "LeftHand" : "RightHand") + "}/" + binding;
            var action = new InputWrapper(binding: binding, ref updateInputs);
            return action;
        }
        
        public void BindAction(InputAction action, Action<bool> callback)
        {
            action.performed += Switch(callback);
        }

        private void Awake()
        {
            gripAction = CreateAction("grip");
            attractAction = CreateAction("primaryButton");
            triggerAction = CreateAction("trigger");

            movement.Init(this);
            binding.Init(this);
            animator.Init(this);
            
            pointRef = transform.DeepFind("Point Ref");

            handModel = transform.DeepFind("Model");
            if (chirality != defaultHandModelChirality)
                handModel.localScale = Vector3.Reflect(Vector3.one, flipAxis.normalized);
        }

        private void FixedUpdate()
        {
            movement.Move();
            binding.UpdateLines();
        }

        private void Update()
        {
            updateInputs();
            
            if (binding.MoveHandToHandleBinding()) return;
            movement.MovementInterpolation();
            animator.Update();
        }

        private void LateUpdate()
        {
            binding.UpdateBinding();
        }

        public enum Chirality
        {
            Left,
            Right,
        }
    }
}