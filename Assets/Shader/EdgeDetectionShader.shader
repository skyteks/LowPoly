Shader "Skyteks/EdgeDetectionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _depthThreshold ("Depth Threshold", Range(0, 1)) = 0.2
        _angleThreshold ("Angle Threshold", Range(0, 10)) = 0.14
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
            uniform  float _angleThreshold;

            float4 frag (v2f i) : SV_Target
            {
                float2 pixelOffset = float2(1,1) / _ScreenParams.xy;
                
                float4 sampleMiddle = tex2D(_MainTex, i.uv);
                float4 sampleLeft   = tex2D(_MainTex, i.uv - float2(pixelOffset.x, 0));
                float4 sampleRight  = tex2D(_MainTex, i.uv + float2(pixelOffset.x, 0));
                float4 sampleTop    = tex2D(_MainTex, i.uv - float2(0, pixelOffset.y));
                float4 sampleBottom = tex2D(_MainTex, i.uv + float2(0, pixelOffset.y));

                sampleMiddle.xzy = sampleMiddle.xzy * 2 - 1;
                sampleLeft.xzy   = sampleLeft.xzy   * 2 - 1;
                sampleRight.xzy  = sampleRight.xzy  * 2 - 1;
                sampleTop.xzy    = sampleTop.xzy    * 2 - 1;
                sampleBottom.xzy = sampleBottom.xzy * 2 - 1;
                
                float left = sampleMiddle.w - sampleLeft.w;
                float right = sampleMiddle.w - sampleRight.w;
                float top = sampleMiddle.w - sampleTop.w;
                float bottom = sampleMiddle.w - sampleBottom.w;

                float outLine = max(max(left, right), max(top, bottom));
                float inLine = -min(min(left, right), min(top, bottom));
                outLine = step(_depthThreshold, outLine);
                inLine = step(_depthThreshold, inLine);

                float angleMiddle = atan2(sampleMiddle.x, sampleMiddle.y);
                float angleLeft = angleMiddle - atan2(sampleLeft.x, sampleLeft.y);
                float angleRight = angleMiddle - atan2(sampleRight.x, sampleRight.y);
                float angleTop = angleMiddle - atan2(sampleTop.x, sampleTop.y);
                float angleBottom = angleMiddle - atan2(sampleBottom.x, sampleBottom.y);

                float angleOutLine = max(max(angleLeft, angleRight), max(angleTop, angleBottom));
                float angleInLine = -min(min(angleLeft, angleRight), min(angleTop, angleBottom));
                angleOutLine = step(_angleThreshold, angleOutLine);

                angleOutLine *= (1 - inLine) * (1 - outLine);

                return float4(angleOutLine, 0, outLine, 1);                
            }
            ENDHLSL
        }
    }
}
