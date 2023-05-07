using System;
using System.Collections.Generic;
using Animation;
using UnityEngine;

[SelectionBase]
[DisallowMultipleComponent]
public sealed class SpiderLegs : MonoBehaviour
{
    [SerializeField] private float stepDistance = 0.1f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float legLift = 0.01f;
    [SerializeField] private List<Leg> groupA, groupB;

    private Vector3 pointA, lPointA;
    private Vector3 pointB, lPointB;
    private Quaternion rotationA, lRotationA;
    private Quaternion rotationB, lRotationB;
    private float changeTimeA, changeTimeB;
    
    private void Awake()
    {
        pointA = transform.position;
        pointB = transform.position;

        rotationA = transform.rotation;
        rotationB = transform.rotation;
    }

    private void Update()
    {
        GetNewLegPosition(ref pointA, ref pointB, ref lPointB, ref rotationB, ref lRotationB, ref changeTimeB);
        GetNewLegPosition(ref pointB, ref pointA, ref lPointA, ref rotationA, ref lRotationA, ref changeTimeA);
    }

    private void LateUpdate()
    {
        void Group(List<Leg> group, Vector3 point, Vector3 lPoint, Quaternion rot, Quaternion lRot, float changeTime)
        {
            var t = Mathf.Clamp01((Time.time - changeTime) / smoothTime);
            
            foreach (var leg in group)
            {
                leg.Animate(Vector3.Lerp(lPoint, point, t) - transform.forward * (Mathf.Sin(t * Mathf.PI) * legLift), Quaternion.Slerp(lRot, rot, t));
            }
        }

        Group(groupA, pointA, lPointA, rotationA, lRotationA, changeTimeA);
        Group(groupB, pointB, lPointB, rotationB, lRotationB, changeTimeB);
    }

    private void GetNewLegPosition(ref Vector3 close, ref Vector3 far, ref Vector3 lPoint, ref Quaternion rot, ref Quaternion lRot, ref float changeTime)
    {
        var closeDist = (close - transform.position).magnitude;
        var farDist = (far - transform.position).magnitude;
        if (closeDist > farDist) return;

        var center = (close + far) / 2.0f;
        var radius = (close - center).magnitude;
        var centerDist = (center - transform.position).magnitude;

        if (centerDist < radius) return;

        lPoint = far;
        lRot = rot;
        changeTime = Time.time;
        
        var dir = (transform.position - center).normalized;
        far = center + dir * stepDistance;
        rot = transform.rotation;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        void DrawGroup(List<Leg> group, Color color)
        {
            if (group == null) return;
            
            Gizmos.color = color;
            foreach (var leg in group)
            {
                Gizmos.DrawLine(leg.root.position, leg.mid.position);
                Gizmos.DrawLine(leg.mid.position, leg.tip.position);
            }
        }

        DrawGroup(groupA, Color.red);
        DrawGroup(groupB, Color.green);

        Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        Gizmos.DrawSphere(pointA, stepDistance / 4.0f);
        Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 1.0f);
        Gizmos.DrawSphere(pointB, stepDistance / 4.0f);
    }

    [ContextMenu("Bake Legs")]
    private void BakeLegs()
    {
        groupA = new List<Leg>();
        groupB = new List<Leg>();
        
        foreach (Transform child in transform)
        {
            // L e g . 1 . 1 . R
            // 0 1 2 3 4 5 6 7 8

            if (child.name[..3] != "Leg") continue;

            var side = child.name[8] == 'L' ? 0 : 1;
            var index = int.Parse(child.name[4].ToString());

            var list = index % 2 == side ? groupA : groupB;
            list.Add(new Leg(transform, child));
        }
    }

    [System.Serializable]
    public class Leg
    {
        public Transform center;
        
        public Transform root;
        public Transform mid;
        public Transform tip;

        public IK ik;
        
        public Vector3 offset;
        public Vector3 start;

        public Leg(Transform center, Transform root)
        {
            this.center = center;
            this.root = root;
            
            mid = root.GetChild(0);
            tip = mid.GetChild(0);

            ik = new IK(root, mid, tip);

            start = Quaternion.Inverse(center.rotation) * (root.position - center.position);
            offset = Quaternion.Inverse(center.rotation) * (tip.position - center.position);
        }

        public void Animate(Vector3 point, Quaternion rotation)
        {
            ik.Solve(center.position + rotation * start, point + rotation * offset, -center.forward);
        }
    }
}