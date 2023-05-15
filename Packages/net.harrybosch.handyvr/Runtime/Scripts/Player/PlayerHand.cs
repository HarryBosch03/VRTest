using System;
using HandyVR.Interactions;
using HandyVR.Player.Hands;
using HandyVR.Player.Input;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace HandyVR.Player
{
    /// <summary>
    /// Behaviour that controls the hands, should be placed on a child
    /// of an Tracked Pose Driver.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PlayerHand : MonoBehaviour
    {
        [Space] [SerializeField] private Chirality chirality;
        [SerializeField] private Chirality defaultHandModelChirality;
        [SerializeField] private Vector3 flipAxis = Vector3.right;

        [Space] [SerializeField] private HandMovement movement;
        [SerializeField] private HandBinding binding;
        [SerializeField] private HandAnimator animator;

        [HideInInspector] public Transform handModel;

        [HideInInspector] public bool ignoreLastBindingCollision;

        private Transform pointRef;

        public HandInput Input { get; private set; }
        public HandBinding BindingController => binding;

        public VRBinding ActiveBinding => binding.ActiveBinding;
        public Rigidbody Rigidbody { get; private set; }
        public Transform Target { get; private set; }
        public Transform PointRef => pointRef ? pointRef : transform;
        public Collider[] Colliders { get; private set; }
        public bool Flipped => chirality != defaultHandModelChirality;

        private void Awake()
        {
            // Clear Parent to stop the transform hierarchy from fucking up physics.
            Target = transform.parent;
            transform.SetParent(null);

            Func<XRController> controller = chirality switch
            {
                Chirality.Left => () => XRController.leftHand,
                Chirality.Right => () => XRController.rightHand,
                _ => throw new ArgumentOutOfRangeException()
            };

            Input = new HandInput(controller);

            Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            Colliders = GetComponentsInChildren<Collider>();

            movement.Init(this);
            binding.Init(this);
            animator.Init(this);

            pointRef = transform.DeepFind("Point Ref");

            handModel = transform.DeepFind("Model");
            if (Flipped)
            {
                var scale = Vector3.Reflect(Vector3.one, flipAxis.normalized);
                handModel.localScale = scale;
            }
        }

        private void FixedUpdate()
        {
            movement.MoveTo(Target.position, Target.rotation);
            binding.FixedUpdate();
        }

        private void Update()
        {
            Input.Update();
            Target.position = Input.Position;
            Target.rotation = Input.Rotation;
            
            if (ActiveBinding)
            {
                handModel.gameObject.SetActive(false);
                ActiveBinding.bindable.Trigger(this, Input.Trigger);
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

        private void OnCollisionEnter(Collision collision)
        {
            movement.OnCollision(collision);
        }

        public enum Chirality
        {
            Left,
            Right,
        }
    }
}