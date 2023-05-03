using System;
using Interactions;
using UnityEngine;

namespace Switches
{
    public sealed class Slider : VRHandle
    {
        [SerializeField] private float sliderRange;
        [SerializeField] private int stops = 0;
        [SerializeField] private float stopSmoothTime = 0.05f;
        [SerializeField] [Range(0.0f, 1.0f)] private float softMovement = 0.1f;

        private Transform handle;
        private Transform grabPoint;

        private float position;
        private float tPosition;

        private float stopVelocity;

        public override Vector3 HandPosition => grabPoint ? grabPoint.position : handle.position;
        public override Quaternion HandRotation => grabPoint ? grabPoint.rotation : handle.rotation;

        protected override void Awake()
        {
            base.Awake();

            handle = transform.GetChild(0);
            grabPoint = handle.GetChild(0);
        }

        private void Update()
        {
            if (stops > 1)
            {
                position = Mathf.SmoothDamp(position, tPosition, ref stopVelocity, stopSmoothTime);
            }

            position = Mathf.Clamp(this.position, -sliderRange / 2.0f, sliderRange / 2.0f);
            handle.position = transform.position + transform.forward * position;
        }

        public override Vector3 SetPosition(Vector3 position)
        {
            var dot = Vector3.Dot(transform.forward, position) - Vector3.Dot(transform.forward, transform.position);

            if (stops > 1) ApplyStops(dot);
            else this.position = dot;

            return handle.position;
        }

        private void ApplyStops(float dot)
        {
            var p = dot / sliderRange + 0.5f;
            p = Mathf.Floor(p * (stops - 1)) / (stops - 1);

            var d = (p - 0.5f) * sliderRange;
            tPosition = d + (dot - d) * softMovement;
        }

        public override Quaternion SetRotation(Quaternion rotation)
        {
            return handle.rotation;
        }
    }
}