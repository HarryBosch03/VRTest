using UnityEngine;
using VRTest.Runtime.Scripts;
using VRTest.Runtime.Scripts.Input;
using VRTest.Runtime.Scripts.Interactions;
using VRTest.Runtime.Scripts.Interactions.Pickups;
using VRTest.Runtime.Scripts.Player;

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

        public void Trigger(PlayerHand hand, VRBindable bindable, InputWrapper input)
        {
            if (input.action.State())
            {
                spraying = true;
            }
        }
    }
}