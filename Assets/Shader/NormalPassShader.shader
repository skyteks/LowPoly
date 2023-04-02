Shader "Skyteks/NormalPassShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

    }
    SubShader
    {
        Tags {"RenderType" = "Opaque"}
        
        Cull Back
        ZWrite On
        ZTest LEqual
        
        Pass
        {
            Name "RenderNormals"
            Tags {"Lightmode" = "RenderNormals"}
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normalWS : NORMAL;
                float3 positionWS : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                o.vertex = vertexInput.positionCS;
                o.normalWS = mul(UNITY_MATRIX_MV, float4(v.normalOS, 0)).xyz;
                o.positionWS = vertexInput.positionWS;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float depth = length(i.positionWS - _WorldSpaceCameraPos);
                float3 normal = normalize(i.normalWS) * 0.5 + 0.5;
                
                return float4(normal, depth);
            }
            ENDHLSL
        }
    }
}
