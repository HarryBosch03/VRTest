using UnityEngine;

namespace HandyVR.Player.Hands
{
    [System.Serializable]
    public class HandAnimator
    {
        private static readonly int Trigger = Animator.StringToHash("trigger");
        private static readonly int Grip = Animator.StringToHash("grip");
        
        private PlayerHand hand;
        
        private Animator animator;
        
        public void Init(PlayerHand hand)
        {
            this.hand = hand;
            animator = hand.GetComponentInChildren<Animator>();
        }

        public void Update()
        {
            animator.SetFloat(Grip, hand.Input.Grip.Value);
            animator.SetFloat(Trigger, hand.Input.Trigger.Value);
        }
    }
}