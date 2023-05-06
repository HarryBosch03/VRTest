using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRHinge : VRHandle
    {
        private const int MaxDeltaSamples = 3;

        [SerializeField] private Vector3 axis = Vector3.up;
        [SerializeField] private bool limitAngle;
        [SerializeField] private float minAngle;
        [SerializeField] private float maxAngle;
        [SerializeField] private float hingeDrag = 25.0f;

        [SerializeField] private float minHingeBounce = 0.3f;
        [SerializeField] private float maxHingeBounce = 0.3f;

        private float angle;
        private float velocity;

        private readonly List<float> deltaSamples = new();
        private bool wasBound;

        public override Vector3 HandPosition => Handle.position;
        public override Quaternion HandRotation => Handle.rotation;

        private Vector3 WorldAxis => transform.parent.TransformDirection(this.axis).normalized;

        public override void SetPosition(Vector3 position)
        {
            var axis = WorldAxis;
            var fwd = (Handle.position - transform.position).normalized;
            var target = (position - transform.position).normalized;

            var cross = Vector3.Dot(axis, Vector3.Cross(fwd, target));

            var delta = Mathf.Asin(cross) * Mathf.Rad2Deg;
            deltaSamples.Add(delta);
            while (deltaSamples.Count > MaxDeltaSamples) deltaSamples.RemoveAt(0);
            angle += delta;
        }

        private void Update()
        {
            transform.rotation = Quaternion.AngleAxis(angle, WorldAxis) * transform.parent.rotation;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!ActiveBinding && wasBound && deltaSamples.Count > 0)
            {
                var delta = 0.0f;
                foreach (var sample in deltaSamples)
                {
                    delta += sample / Time.deltaTime;
                }
                delta /= deltaSamples.Count;
                velocity = delta;
            }

            angle += velocity * Time.deltaTime;
            velocity -= velocity * hingeDrag * Time.deltaTime;

            if (angle < minAngle)
            {
                angle = minAngle;
                if (velocity < 0.0f) velocity = -velocity * minHingeBounce;
            }
            else if (angle > maxAngle)
            {
                angle = maxAngle;
                if (velocity > 0.0f) velocity = -velocity * maxHingeBounce;
            }

            wasBound = ActiveBinding;
        }

        public override void SetRotation(Quaternion rotation)
        {
        }
    }
}