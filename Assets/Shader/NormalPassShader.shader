Shader "Hidden/NormalPassShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

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
                float3 normalOS : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normalWS : NORMAL;
            };

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS.xyz);
                o.vertex = vertexInput.positionCS;
                o.normalWS = normalInput.normalWS;
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            float SampleLinearDepth(float2 uv)
            {
                float depth = SampleSceneDepth(uv + 0.25 /_ScreenParams.xy); 
                return LinearEyeDepth(depth, _ZBufferParams); 
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 sample00 = tex2D(_MainTex, i.uv);
                float depth = i.vertex.z
                
                return float4(i.normalWS * 0.5 + 0.5, );
            }
            ENDHLSL
        }
    }
}
