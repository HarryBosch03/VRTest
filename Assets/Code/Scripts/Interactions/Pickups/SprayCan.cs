using HandyVR.Interactions;
using HandyVR.Interactions.Pickups;
using HandyVR.Player;
using HandyVR.Player.Input;
using UnityEngine;

namespace Interactions.Pickups
{
    [RequireComponent(typeof(VRPickup))]
    public class SprayCan : MonoBehaviour, IVRBindableListener
    {
        private ParticleSystem[] effects;

        private bool spraying;
        
        private void Awake()
        {
            var container = transform.DeepFind("Spray FX");
            effects = container.GetComponentsInChildren<ParticleSystem>();
        }

        private void Update()
        {
            foreach (var effect in effects)
            {
                switch (spraying)
                {
                    case true when !effect.isPlaying:
                        effect.Play();
                        break;
                    case false when effect.isPlaying:
                        effect.Stop();
                        break;
                }
            }
            spraying = false;
        }

        public void Trigger(PlayerHand hand, VRBindable bindable, HandInput.InputWrapper input)
        {
            if (input.Down)
            {
                spraying = true;
            }
        }
    }
}