Shader "Hidden/Pixelation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _pixelsX("Resolution Width", int) = 1920
        _pixelsY ("Resolution Height", int) = 1080
        _pixelWidth("Pixel Width", float) = 64
        _pixelHeight("Pixel Height", float) = 64

    }

    SubShader
    {
        //Cull Off
        //ZWrite Off
        //ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                //float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                //float4 vertex : SV_POSITION;

                float4 pos : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float3 camRelativeWorldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = v.uv;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                //view direction vector -> not normalized
                o.camRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz - _WorldSpaceCameraPos;
                return o;
            }

            sampler2D _MainTex;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            sampler2D _CameraDepthNormalsTexture;
            float _pixelsX;
            float _pixelsY;
            float _pixelWidth;
            float _pixelHeight;
            float _dx;
            float _dy;

            float4 frag(v2f i) : SV_Target
            {
                float3 one = float3(1, 1, 1);
                
                _dx = _pixelWidth * (1 / _pixelsX);
                _dy = _pixelHeight * (1 / _pixelsY);
                float2 coord = float2(_dx * floor(i.screenPos.x / _dx), _dy * floor(i.screenPos.y / _dy));
                float4 col = tex2D(_MainTex, coord);

                /*
                // Depth grey
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
                float4 col = float4((1 - depth) * one, 1);
                */

                /*
                // Depth * Normal https://forum.unity.com/threads/decodedepthnormal-linear01depth-lineareyedepth-explanations.608452/
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
                float sceneZ = LinearEyeDepth(depth);
                float3 rayNorm = normalize(i.camRelativeWorldPos.xyz);
                //unity_WorldToCamera._m20_m21_m22 is z_cam
                float3 rayUnitDepth = rayNorm / dot(rayNorm, unity_WorldToCamera._m20_m21_m22);
                float3 worldPos = rayUnitDepth * sceneZ + _WorldSpaceCameraPos;
                //pad for rendering value directly into ARGBFloat RenderTexture
                float4 col = float4(worldPos, 1.0);
                */

                /*
                // depthNormal https://williamchyr.com/unity-shaders-depth-and-normal-textures-part-3/
                float3 normalValues;
                float depthValue;
                //extract depth value and normal values
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.screenPos.xy), depthValue, normalValues);
                float4 col =  float4(normalValues, 1);
                //float4 col = float4(depthValue.xxx, 1);
                */

                return col;
            }
            ENDCG
        }
    }
}
