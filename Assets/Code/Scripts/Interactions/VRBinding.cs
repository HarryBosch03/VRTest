using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Interactions
{
    public class VRBinding
    {
        private const int ThrowSamples = 3;

        private const bool DebugThrow = false;

        public readonly VRBindable bindable;
        public readonly float bindTime;
        public bool active;

        private Vector3 lastPosition;
        
        private Vector3 lastValue;
        
        private readonly bool wasKinematic;

        private readonly List<Vector3> lastPositions = new();

        private static readonly List<GameObject> TmpDebug = new();

        public Vector3 Position
        {
            get => bindable.transform.position;
            set => bindable.SetPosition(value);
        }

        public Quaternion Rotation
        {
            get => bindable.transform.rotation;
            set => bindable.SetRotation(value);
        }
        
        public VRBinding(VRBindable bindable, float throwForceScale)
        {
            if (bindable.ActiveBinding) bindable.ActiveBinding.Throw(throwForceScale);

            this.bindable = bindable;
            bindTime = Time.time;
            active = true;

            if (!bindable.Rigidbody) return;
            
            wasKinematic = bindable.Rigidbody.isKinematic;
            bindable.Rigidbody.isKinematic = true;
        }

        public void FixedUpdate()
        {
            if (!active) return;

            lastPositions.Add(bindable.Handle.position);
            while (lastPositions.Count > ThrowSamples) lastPositions.RemoveAt(0);
        }

        public void Debug()
        {
            if (!DebugThrow) return;

            foreach (var obj in TmpDebug)
            {
                Object.Destroy(obj);
            }

            TmpDebug.Clear();

            for (var i = 0; i < lastPositions.Count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(go.GetComponent<Collider>());

                go.transform.position = lastPositions[i];
                go.transform.localScale = Vector3.one * 0.05f;
                TmpDebug.Add(go);
            }
        }

        public void Throw(float throwForceScale)
        {
            if (!active) return;
            active = false;

            var rb = bindable.Rigidbody;
            if (!rb) return;

            rb.isKinematic = wasKinematic;
            if (rb.isKinematic) return;

            var force = Vector3.zero;
            lastPositions.Add(bindable.Handle.position);

            if (lastPositions.Count > 1)
            {
                var tw = 0.0f;
                for (var i = 1; i < lastPositions.Count; i++)
                {
                    var v = lastPositions[i] - lastPositions[i - 1];
                    var w = 1.0f;

                    force += v * w / Time.deltaTime;
                    tw += w;
                }

                if (tw != 0.0f)
                {
                    force /= tw;

                    force *= Mathf.Min(1.0f, throwForceScale / rb.mass);
                    force -= rb.velocity;
                    rb.AddForceAtPosition(force * throwForceScale, bindable.Handle.position,
                        ForceMode.VelocityChange);
                }
            }

            Debug();
        }

        public bool Valid()
        {
            if (!active) return false;
            if (!bindable) return false;

            return true;
        }

        public static implicit operator bool(VRBinding binding)
        {
            return binding != null && binding.Valid();
        }
    }
}