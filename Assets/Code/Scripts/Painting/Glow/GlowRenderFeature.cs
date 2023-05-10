using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Painting.Glow
{
    public sealed class GlowRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private LayerMask renderMask;
        [SerializeField] private Material overrideMaterial;
        
        private GlowRenderPass glowRenderPass;
        
        public override void Create()
        {
            glowRenderPass = new GlowRenderPass(renderMask, overrideMaterial);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(glowRenderPass);
        }
    }
}
