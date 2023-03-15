Shader "Hidden/EdgeDetectionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _depthThreshold ("Depth Threshold", Range(0, 200)) = 50

    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                o.vertex = vertexInput.positionCS;
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            uniform float _depthThreshold;

            float SampleLinearDepth(float2 uv)
            {
                float depth = SampleSceneDepth(uv + 0.25 /_ScreenParams.xy); 
                return LinearEyeDepth(depth, _ZBufferParams); 
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 pixelOffset = float2(1,1) / _ScreenParams.xy;
                
                float4 sample00 = tex2D(_MainTex, i.uv);
                float4 sample10 = tex2D(_MainTex, i.uv - float2(0, pixelOffset.y));
                float4 sample01 = tex2D(_MainTex, i.uv - float2(pixelOffset.x, 0));
                float4 sample11 = tex2D(_MainTex, i.uv - pixelOffset);

                sample00.w = SampleLinearDepth(i.uv);
                sample10.w = SampleLinearDepth(i.uv - float2(0, pixelOffset.y));
                sample01.w = SampleLinearDepth(i.uv - float2(pixelOffset.x, 0));
                sample11.w = SampleLinearDepth(i.uv - pixelOffset);

                float4 horizontal = abs(sample00 - sample11);
                float4 vertical = abs(sample10 - sample01);

                float4 edge = step(_depthThreshold, max(horizontal, vertical));

                return edge.w;
            }
            ENDHLSL
        }
    }
}
