using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace URP
{
    public class EdgeDetection : ScriptableRendererFeature
    {
        public Material edgeDetectionMat;
        private EdgeDetectionRenderPass _renderPass;
        
        public override void Create()
        {
            _renderPass = new EdgeDetectionRenderPass(edgeDetectionMat);
            _renderPass.renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_renderPass);
        }
    }
}
