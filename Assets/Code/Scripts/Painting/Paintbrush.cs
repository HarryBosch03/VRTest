using System;
using UnityEngine;

namespace VRTest.Painting
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class Paintbrush : MonoBehaviour
    {
        [SerializeField] private PaintManager.Brush brush;
        [SerializeField] private Renderer[] renderers;

        private void Update()
        {
            brush.position = transform.position;
            foreach (var renderer in renderers) PaintManager.Paint(brush, renderer);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = brush.color;
            Gizmos.DrawWireSphere(transform.position, brush.radius);
        }
    }
}