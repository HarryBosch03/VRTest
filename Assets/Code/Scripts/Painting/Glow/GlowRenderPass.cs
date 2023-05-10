using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Painting.Glow
{
    public class GlowRenderPass : ScriptableRenderPass
    {
        private readonly LayerMask renderMask;
        private readonly Material overrideMaterial;

        private new readonly ProfilingSampler profilingSampler = new("Glow Pass");

        private readonly List<ShaderTagId> shaderTags = new()
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
        };

        public GlowRenderPass(LayerMask renderMask, Material overrideMaterial)
        {
            this.renderMask = renderMask;
            this.overrideMaterial = overrideMaterial;

            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cam = renderingData.cameraData.camera;

            if (!cam.TryGetCullingParameters(out var cullingParameters)) return;
            var cullResults = context.Cull(ref cullingParameters);
            
            var drawingSettings = CreateDrawingSettings(shaderTags, ref renderingData,
                SortingCriteria.CommonTransparent);
            drawingSettings.overrideMaterial = overrideMaterial;

            var filteringSettings = new FilteringSettings(RenderQueueRange.all, renderMask);

            context.DrawRenderers(cullResults, ref drawingSettings, ref filteringSettings);
        }
    }
}