using System;
using UnityEngine;
using UnityEngine.SpatialTracking;
using VRTest.Input;
using VRTest.Interactions;
using VRTest.Player.Hands;

namespace VRTest.Player
{
    /// <summary>
    /// Behaviour that controls the hands, should be placed on a child
    /// of an Tracked Pose Driver.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PlayerHand : MonoBehaviour
    {
        [Space] 
        [SerializeField] private Chirality defaultHandModelChirality;
        [SerializeField] private Vector3 flipAxis = Vector3.right;

        [Space] 
        [SerializeField] private HandMovement movement;
        [SerializeField] private HandBinding binding;
        [SerializeField] private HandAnimator animator;

        public InputWrapper gripAction;
        public InputWrapper triggerAction;

        [HideInInspector] public Transform handModel;

        [HideInInspector] public bool ignoreLastBindingCollision;
        
        private Action updateInputsCallback;
        private Chirality chirality;

        private Transform pointRef;

        public VRBinding ActiveBinding => binding.ActiveBinding;
        public Rigidbody Rigidbody { get; private set; }
        public Transform Target { get; private set; }
        public Transform PointRef => pointRef ? pointRef : transform;
        public Collider[] Colliders { get; private set; }
        
        /// <summary>
        /// Creates an Input Wrapper object for a XRController binding, with the correct controller chirality.
        /// </summary>
        /// <param name="binding">The Unity Input System name of the binding, must be a binding from a XRController device</param>
        /// <returns></returns>
        public InputWrapper CreateAction(string binding)
        {
            binding = "<XRController>{" + (chirality == Chirality.Left ? "LeftHand" : "RightHand") + "}/" + binding;
            var action = new InputWrapper(binding: binding, ref updateInputsCallback);
            return action;
        }

        private void Awake()
        {
            // Clear Parent to stop the transform hierarchy from fucking up physics.
            // TODO - actually test if this actually does anything.
            Target = transform.parent;
            transform.SetParent(null);

            SetChirality();

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

        private void SetChirality()
        {
            var trackedPoseDriver = Target.GetComponent<TrackedPoseDriver>();
            switch (trackedPoseDriver.poseSource)
            {
                case TrackedPoseDriver.TrackedPose.LeftPose:
                    chirality = Chirality.Left;
                    break;
                case TrackedPoseDriver.TrackedPose.RightPose:
                    chirality = Chirality.Right;
                    break;

                case TrackedPoseDriver.TrackedPose.LeftEye:
                case TrackedPoseDriver.TrackedPose.RightEye:
                case TrackedPoseDriver.TrackedPose.Center:
                case TrackedPoseDriver.TrackedPose.Head:
                case TrackedPoseDriver.TrackedPose.ColorCamera:
                case TrackedPoseDriver.TrackedPose.DepthCameraDeprecated:
                case TrackedPoseDriver.TrackedPose.FisheyeCameraDeprected:
                case TrackedPoseDriver.TrackedPose.DeviceDeprecated:
                case TrackedPoseDriver.TrackedPose.RemotePose:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FixedUpdate()
        {
            movement.MoveTo(Target.position, Target.rotation);
            binding.FixedUpdate();
        }

        private void Update()
        {
            updateInputsCallback();

            if (ActiveBinding)
            {
                handModel.gameObject.SetActive(false);
                ActiveBinding.bindable.Trigger(this, triggerAction);
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