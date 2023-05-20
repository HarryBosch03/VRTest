using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HandyVR.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private float collisionSize = 0.1f;
        [SerializeField] private float dimSpeed = 12.0f;

        private Volume volume;
        private VolumeProfile volumeProfile;
        private new Camera camera;

        private float position;

        private void Awake()
        {
            camera = GetComponentInChildren<Camera>();
            
            volume = GetComponentInChildren<Volume>();
            if (!volume)
            {
                volume = new GameObject("Dimming Volume").AddComponent<Volume>();
                volume.transform.SetParent(transform);
            }

            volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            volumeProfile.hideFlags = HideFlags.HideAndDontSave;
            
            var colorAdjustments = volumeProfile.Add<ColorAdjustments>();
            colorAdjustments.colorFilter.Override(Color.black);

            volume.profile = volumeProfile;
            volume.priority = 100;
        }

        private void OnDestroy()
        {
            Destroy(volumeProfile);
        }

        private void FixedUpdate()
        {
            var dim = Physics.CheckSphere(camera.transform.position, collisionSize);
            var t = dim ? 1.0f : 0.0f;
            volume.weight += (t - volume.weight) * dimSpeed * Time.deltaTime;
        }
    }
}