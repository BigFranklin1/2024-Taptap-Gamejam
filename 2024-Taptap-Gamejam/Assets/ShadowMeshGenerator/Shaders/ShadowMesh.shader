Shader "Unlit/ShadowMesh"
{
    Properties
    {
        [NoScaleOffset]_DissolveTex ("Texture", 2D) = "white" {}
        _AppearProgress("Appear Progress", Range(0, 1)) = 0
        _EmissionIntensity("Emission Intensity", Range(1, 2)) = 1
        _Color("Color", Color) = (0.9607843, 0.7960784, 0.06666667, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent"}
        LOD 100

        Pass
        {
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            ZTest LEqual
            ZWrite Off

            // Stencil
            // {
            //     Ref 1
            //     Comp Always
            //     Pass Replace
            // }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 screenUV : TEXCOORD0;
            };

            sampler2D _DissolveTex;
            float _AppearProgress;
            float _EmissionIntensity;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenUV = o.pos.xy / o.pos.w;
                o.screenUV = o.screenUV * 0.5 + 0.5;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float alpha = clamp(tex2D(_DissolveTex, i.screenUV).r, 0.0, 1.0);
                alpha += 2 * _AppearProgress - 1;
                float4 color = _Color * _EmissionIntensity;
                return float4(color.rgb, saturate(alpha));
            }
            ENDHLSL
        }
    }
}
