using UnityEngine;

namespace HandyVR.Player.Hands
{
    [System.Serializable]
    public class HandAnimator
    {
        public const float Smoothing = 0.1f;

        private static readonly int Trigger = Animator.StringToHash("trigger");
        private static readonly int Grip = Animator.StringToHash("grip");

        private float gripValue, triggerValue;
        
        private PlayerHand hand;

        private Animator animator;

        public void Init(PlayerHand hand)
        {
            this.hand = hand;
            animator = hand.GetComponentInChildren<Animator>();
        }

        public void Update()
        {
            float tGripValue, tTriggerValue;
            
            if (hand.BindingController.DetachedBinding)
            {
                tTriggerValue = 1.0f;
                tGripValue = 1.0f;
            }
            else if (hand.BindingController.PointingAt)
            {
                tTriggerValue = 0.0f;
                tGripValue = 1.0f;
            }
            else
            {
                tGripValue = hand.Input.Grip.Value;
                tTriggerValue = hand.Input.Trigger.Value;
            }

            gripValue += Smoothing > 0.0f ? (tGripValue - gripValue) / Smoothing * Time.deltaTime : tGripValue;
            triggerValue += Smoothing > 0.0f ? (tTriggerValue - triggerValue) / Smoothing * Time.deltaTime : tTriggerValue;
            
            animator.SetFloat(Grip, gripValue);
            animator.SetFloat(Trigger, triggerValue);
        }
    }
}