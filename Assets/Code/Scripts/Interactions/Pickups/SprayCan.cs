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
        [SerializeField] private ParticleSystem spray;
        [SerializeField] private ParticleSystem residue;

        private bool spraying;

        private ParticleSystem.Particle[] sprayBuffer;

        private void Awake()
        {
            var container = transform.DeepFind("Spray FX");

            sprayBuffer = new ParticleSystem.Particle[spray.main.maxParticles];
        }

        private void Update()
        {
            if (spraying) SpawnResidue();

            switch (spraying)
            {
                case true when !spray.isPlaying:
                    spray.Play();
                    break;
                case false when spray.isPlaying:
                    spray.Stop();
                    break;
            }

            spraying = false;
        }

        private void SpawnResidue()
        {
            var numAlive = spray.GetParticles(sprayBuffer);
            if (numAlive == 0) return;

            var reference = sprayBuffer[Random.Range(0, numAlive)];

            var ray = new Ray(spray.transform.position, reference.velocity);
            var distance = (reference.startLifetime * reference.velocity).magnitude;
            if (!Physics.Raycast(ray, out var hit, distance)) return;

            var emitParams = new ParticleSystem.EmitParams();
            emitParams.position = hit.point;
            spray.Emit(emitParams, 1);
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