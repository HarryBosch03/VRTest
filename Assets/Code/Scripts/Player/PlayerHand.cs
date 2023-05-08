using System;
using Input;
using Interactions;
using Player.Hands;
using UnityEngine;

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

        public InputWrapper gripAction;
        public InputWrapper triggerAction;

        public VRBinding activeBinding;
        [HideInInspector] public Transform handModel;

        [HideInInspector] public bool ignoreLastBindingCollision;
        
        private Action updateInputs;

        private Transform pointRef;

        public Rigidbody Rigidbody { get; private set; }
        public Transform Target { get; private set; }
        public Transform PointRef => pointRef ? pointRef : transform;
        public Collider[] Colliders { get; private set; }
        
        public InputWrapper CreateAction(string binding)
        {
            binding = "<XRController>{" + (chirality == Chirality.Left ? "LeftHand" : "RightHand") + "}/" + binding;
            var action = new InputWrapper(binding: binding, ref updateInputs);
            return action;
        }

        private void Awake()
        {
            Target = transform.parent;
            transform.SetParent(null);

            Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            Colliders = GetComponentsInChildren<Collider>();
            
            gripAction = CreateAction("grip");
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
            movement.MoveTo(Target.position, Target.rotation);
            binding.FixedUpdate();
        }

        private void Update()
        {
            updateInputs();

            if (activeBinding)
            {
                handModel.gameObject.SetActive(false);
                activeBinding.bindable.Trigger(this, triggerAction);
            }
            else
            {
                handModel.gameObject.SetActive(true);
                animator.Update();
            }
        }

        private void LateUpdate()
        {
            binding.Update();
        }

        public enum Chirality
        {
            Left,
            Right,
        }
    }
}