using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URP
{
    public class EdgeDetectionRenderPass : ScriptableRenderPass
    {
        private Material _edgeDetectionMat;
        private RenderTexture _edgeDetectionTarget;

        public EdgeDetectionRenderPass(Material mat)
        {
            _edgeDetectionMat = mat;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            _edgeDetectionTarget = RenderTexture.GetTemporary(cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            _edgeDetectionTarget.filterMode = FilterMode.Point;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get("EdgeDetection");

            RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;
            
            commandBuffer.Blit(source, _edgeDetectionTarget, _edgeDetectionMat);
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
