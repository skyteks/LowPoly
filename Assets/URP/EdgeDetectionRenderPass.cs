using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URP
{
    public class EdgeDetectionRenderPass : ScriptableRenderPass
    {
        private FilteringSettings _filterSettings;
        private RenderStateBlock _renderStateBlock;
        private List<ShaderTagId> _shaderTagIds;
        
        private EdgeDetection.FeatureParams _featureParams;
        private RenderTexture _edgeDetectionTarget;
        private RenderTexture _normalPassTarget;

        private ProfilingSampler _normalProfilingSampler;

        public EdgeDetectionRenderPass(EdgeDetection.FeatureParams featureParams)
        {
            _featureParams = featureParams;

            _filterSettings = new FilteringSettings(RenderQueueRange.opaque, _featureParams.layerMask);
            _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            _shaderTagIds = new()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly")
            };

            _normalProfilingSampler = new ProfilingSampler("NormalPass");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            //Create Targets
            _edgeDetectionTarget = RenderTexture.GetTemporary(cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            _edgeDetectionTarget.filterMode = FilterMode.Point;
            
            _normalPassTarget = RenderTexture.GetTemporary(cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
            _normalPassTarget.filterMode = FilterMode.Point;
            
            //ConfigureTarget(_normalPassTarget);
            float clearDepth = 1000f;
            //ConfigureClear(ClearFlag.All, new Color(0.5f, 0.5f, 1f, clearDepth));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            base.profilingSampler = new ProfilingSampler(nameof(EdgeDetectionRenderPass));
            RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;

            DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIds, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            drawingSettings.overrideMaterial = _featureParams.normalPass;
            drawingSettings.overrideMaterialPassIndex = 0;
            
            CommandBuffer normalBuffer = CommandBufferPool.Get();
            using (new ProfilingScope(normalBuffer, _normalProfilingSampler))
            {
                context.ExecuteCommandBuffer(normalBuffer);
                normalBuffer.Clear();
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings,ref _filterSettings, ref _renderStateBlock);
            }
            context.ExecuteCommandBuffer(normalBuffer);
            CommandBufferPool.Release(normalBuffer);
            
            CommandBuffer commandBuffer = CommandBufferPool.Get("EdgeDetection");
            commandBuffer.Blit(source, _edgeDetectionTarget, _featureParams.edgeDetection);
            commandBuffer.Blit(_edgeDetectionTarget, renderingData.cameraData.renderer.cameraColorTarget, _featureParams.edgeBlend);
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            CommandBufferPool.Release(commandBuffer);
        }

        public override void FrameCleanup(CommandBuffer buffer)
        {
            //Release targets
            RenderTexture.ReleaseTemporary(_edgeDetectionTarget);
            RenderTexture.ReleaseTemporary(_normalPassTarget);
        }
    }
}
