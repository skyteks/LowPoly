Shader "Skyteks/EdgeBlendShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _outlineBrightness ("Outline Brighness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend One SrcAlpha

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
            uniform float _outlineBrightness;

            float4 frag (v2f i) : SV_Target
            {
                float4 outlines = tex2D(_MainTex, i.uv);

                float3 brigths = float3(0, 0, 0);
                
                float darks = 1;
                darks = lerp(darks, _outlineBrightness, outlines.r);

                return float4(brigths, darks);                
            }
            ENDHLSL
        }
    }
}
