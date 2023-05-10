using UnityEngine;

namespace HandyVR.Switches
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class HardButton : MonoBehaviour
    {
        [SerializeField] private Bounds queryBounds;
        [SerializeField] private Vector3 inactivePose;
        [SerializeField] private Vector3 activePose;
        [SerializeField] private Transform articulation;
        [SerializeField] private float smoothTime;

        private float percent;
        private float velocity;
    
        public bool State { get; set; }

        private void Update()
        {
            State = GetState();

            var target = State ? 1.0f : 0.0f;
            percent = Mathf.SmoothDamp(percent, target, ref velocity, smoothTime);
            articulation.localPosition = Vector3.LerpUnclamped(inactivePose, activePose, percent);
        }

        private bool GetState()
        {
            var queries = Physics.OverlapBox(transform.position + transform.rotation * queryBounds.center,
                queryBounds.extents, transform.rotation);

            foreach (var query in queries)
            {   
                if (query.transform.IsChildOf(transform)) continue;
                return true;
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(queryBounds.center, queryBounds.size);
            Gizmos.color *= new Color(1.0f, 1.0f, 1.0f, 0.1f);
            Gizmos.DrawCube(queryBounds.center, queryBounds.size);
        }
    }
}