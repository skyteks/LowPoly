using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URP
{
    public class EdgeDetectionRenderPass : ScriptableRenderPass
    {
        private EdgeDetection.FeatureParams _featureParams;
        private RenderTexture _edgeDetectionTarget;

        public EdgeDetectionRenderPass(EdgeDetection.FeatureParams featureParams)
        {
            _featureParams = featureParams;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            _edgeDetectionTarget = RenderTexture.GetTemporary(cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            _edgeDetectionTarget.filterMode = FilterMode.Point;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;

            CommandBuffer commandBuffer = CommandBufferPool.Get("EdgeDetection");
            commandBuffer.Blit(source, _edgeDetectionTarget, _featureParams.edgeDetection);
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            commandBuffer.Blit(_edgeDetectionTarget, renderingData.cameraData.renderer.cameraColorTarget, _featureParams.edgeBlend);
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
            CommandBufferPool.Release(commandBuffer);
        }

        public override void FrameCleanup(CommandBuffer buffer)
        {
            RenderTexture.ReleaseTemporary(_edgeDetectionTarget);
        }
    }
}
