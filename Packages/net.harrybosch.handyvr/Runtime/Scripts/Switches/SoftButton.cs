using UnityEngine;

namespace VRTest.Runtime.Scripts.Switches
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10)]
    public sealed class SoftButton : MonoBehaviour
    {
        private const float SkinWidth = 0.004f;

        [SerializeField] private float actuationDistance;
        [SerializeField] [Range(0.0f, 1.0f)] private float pressPoint = 0.5f;

        [SerializeField] private float buttonRadius;

        [SerializeField] private float buttonHeight = 0.075f;

        [Space] 
        [SerializeField] private float spring = 500.0f;
        [SerializeField] private float damper = 50.0f;

        private Transform actuationBody;

        private float lastPosition;
        private float position;
        private float velocity;
        private float acceleration;

        public bool State => position / -actuationDistance > pressPoint; 

        private void Awake()
        {
            actuationBody = transform.GetChild(0);
        }

        private void FixedUpdate()
        {
            lastPosition = position;
            actuationBody.localPosition = Vector3.down * actuationDistance;
            
            acceleration += -position * spring;
            acceleration += -velocity * damper;

            Integrate();
            Collide();
        }

        private void LateUpdate()
        {
            var t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            var p = Mathf.Lerp(lastPosition, position, t);
            actuationBody.transform.localPosition = ButtonPos(p);
        }

        private void Integrate()
        {
            position += velocity * Time.deltaTime;
            velocity += acceleration * Time.deltaTime;
            acceleration = 0.0f;
        }

        private bool Raycast(Ray ray, out RaycastHit hit, float maxDistance)
        {
            hit = new RaycastHit();
            var didHit = false;

            var results = Physics.RaycastAll(ray, maxDistance);
            foreach (var result in results)
            {
                if (result.transform.IsChildOf(transform))
                    if (result.distance > hit.distance && !didHit)
                        continue;

                didHit = true;
                hit = result;
            }

            return didHit;
        }

        private void Collide()
        {
            if (position < -actuationDistance)
            {
                position = -actuationDistance;
                if (velocity < 0.0f) velocity = -velocity * 0.4f;
            }
            
            var ray = new Ray(transform.position + transform.up * (buttonHeight - actuationDistance), transform.up);
            var dist = -position;
            var collided = false;

            const int xSamples = 6;
            const int ySamples = 24;

            for (var i = 0; i < xSamples; i++)
            {
                for (var j = 0; j < ySamples; j++)
                {
                    var d = i / (float)xSamples * buttonRadius;
                    var a = j / (float)ySamples * 2.0f * Mathf.PI;
                    var p = transform.TransformDirection(Mathf.Cos(a), 0.0f, Mathf.Sin(a)) * d;
                    var lRay = new Ray(ray.origin + p, ray.direction);

                    Vector3 point;
                    Color color;

                    if (Raycast(lRay, out var hit, actuationDistance))
                    {
                        var lDist = actuationDistance - hit.distance;
                        if (lDist > dist)
                        {
                            dist = lDist;
                            collided = true;
                        }

                        point = lRay.GetPoint(hit.distance);
                        color = Color.green;
                    }
                    else
                    {
                        point = lRay.GetPoint(actuationDistance);
                        color = Color.red;
                    }

                    Debug.DrawLine(point - transform.up * 0.001f, point, color);
                }
            }

            if (!collided) return;

            var newPos = -Mathf.Min(dist, actuationDistance);
            if (velocity > 0.0f) velocity = 0.0f;
            position = newPos;
        }

        private static Vector3 ButtonPos(float p) => Vector3.up * (p - SkinWidth);
    }
}