// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ShadowReceiver"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE 
            #pragma multi_compile _ _SHADOWS_SOFT 
            #pragma multi_compile _ _ADDTIONAL_LIGHT_SHADOWS

            #define ADDITIONAL_LIGHT_CALCULATE_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct a2v
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (a2v v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 color = 0.0;
                int pixelLightCount = GetAdditionalLightsCount();
                for (int lightIndex=0; lightIndex<pixelLightCount; lightIndex++) {
                    Light light = GetAdditionalLight(lightIndex, i.worldPos);
                    if (light.layerMask == 2) {
                        float shadow = AdditionalLightRealtimeShadow(lightIndex, i.worldPos);
                        color = float4(shadow, shadow, shadow, 1.0);
                    }
                }
                return color;
            }

            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "SSShadowMask" }
            ZWrite Off
            ZTest Always

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE 
            #pragma multi_compile _ _SHADOWS_SOFT 
            #pragma multi_compile _ _ADDTIONAL_LIGHT_SHADOWS

            #define ADDITIONAL_LIGHT_CALCULATE_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct a2v
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                // float2 screenUV : TEXCOORD1;
            };

            // sampler2D _CurrentSSShadowMask;

            v2f vert (a2v v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // o.screenUV = o.pos.xy / o.pos.w;
                // o.screenUV = o.screenUV * 0.5 + 0.5;

                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 color = 0.0;
                int pixelLightCount = GetAdditionalLightsCount();
                for (int lightIndex=0; lightIndex<pixelLightCount; lightIndex++) {
                    Light light = GetAdditionalLight(lightIndex, i.worldPos);
                    if (light.layerMask == 2) {
                        float shadow = AdditionalLightRealtimeShadow(lightIndex, i.worldPos);
                        if (shadow < 0.5) {
                            color = float4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1.0);
                        }
                        else {
                            color = float4(0.0, 0.0, 0.0, 0.0);
                        }
                    }
                }
                return color;
            }

            ENDHLSL
        }
    }
}
