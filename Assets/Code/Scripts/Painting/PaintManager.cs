using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRTest.Painting
{
    public static class PaintManager
    {
        private const int TextureSize = 512;
        
        private static readonly CommandBuffer Cmd = new();
        private static Material paintMaterial = new(Shader.Find("Hidden/Paint"));

        private static readonly Dictionary<Renderer, RenderTexture> TextureMap = new();
        
        private static readonly int BPos = Shader.PropertyToID("_BPos");
        private static readonly int BCol = Shader.PropertyToID("_BColor");
        private static readonly int BSize = Shader.PropertyToID("_BSize");
        private static readonly int BHardness = Shader.PropertyToID("_BHardness");
        private static readonly int PaintTex = Shader.PropertyToID("_PaintTex");

        public static RenderTexture GetTexture(Renderer renderer)
        {
            if (!TextureMap.ContainsKey(renderer)) TextureMap.Add(renderer, new RenderTexture(TextureSize, TextureSize, 0));
            return TextureMap[renderer];
        }
        
        public static void Paint(Brush brush, Renderer renderer)
        {
            var texture = GetTexture(renderer);
            
            paintMaterial.SetVector(BPos, brush.position);
            paintMaterial.SetColor(BCol, brush.color);
            paintMaterial.SetFloat(BSize, brush.radius);
            paintMaterial.SetFloat(BHardness, brush.hardness);

            foreach (var material in renderer.sharedMaterials)
            {
                material.SetTexture(PaintTex, texture);
            }
            
            Cmd.SetRenderTarget(texture);
            Cmd.DrawRenderer(renderer, paintMaterial);
            
            Graphics.ExecuteCommandBuffer(Cmd);
            Cmd.Clear();
        }
        
        [System.Serializable]
        public class Brush
        {
            public Vector3 position;
            public Color color;
            public float radius;
            [Range(0.0f, 1.0f)]public float hardness;

            public Brush(Vector3 position, Color color, float radius, float hardness)
            {
                this.position = position;
                this.color = color;
                this.radius = radius;
                this.hardness = hardness;
            }
        }
    }
}