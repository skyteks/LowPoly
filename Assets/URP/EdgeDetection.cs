using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace URP
{
    public class EdgeDetection : ScriptableRendererFeature
    {
        [System.Serializable]
        public struct FeatureParams
        {
            public LayerMask layerMask;
            
            [Space]
            
            public Material edgeDetection;
            public Material edgeBlend;
            public Material normalPass;
        }
        
        public FeatureParams featureParams;
        private EdgeDetectionRenderPass _renderPass;
        
        public override void Create()
        {
            _renderPass = new EdgeDetectionRenderPass(featureParams);
            _renderPass.renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_renderPass);
        }
    }
}
