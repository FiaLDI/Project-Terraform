Shader "Game/Abilities/Mine Radius Circle"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.25, 0.05, 0.65)
        _FillColor ("Fill Color", Color) = (1, 0.15, 0.05, 0.12)

        _LineWidth ("Line Width", Range(0.001, 0.25)) = 0.035
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.25)) = 0.025
        _FillAlpha ("Fill Alpha", Range(0, 1)) = 0.15

        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2
        _PulseStrength ("Pulse Strength", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _FillColor;

            float _LineWidth;
            float _EdgeSoftness;
            float _FillAlpha;

            float _PulseSpeed;
            float _PulseStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centeredUv = i.uv - 0.5;
                float dist = length(centeredUv);

                float outerRadius = 0.5;
                float innerRadius = outerRadius - _LineWidth;

                float outerMask = 1.0 - smoothstep(
                    outerRadius - _EdgeSoftness,
                    outerRadius,
                    dist
                );

                float innerMask = smoothstep(
                    innerRadius - _EdgeSoftness,
                    innerRadius,
                    dist
                );

                float ringMask = outerMask * innerMask;
                float fillMask = outerMask * (1.0 - innerMask);

                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseStrength;

                fixed4 ringColor = _Color;
                ringColor.a *= ringMask * pulse;

                fixed4 fillColor = _FillColor;
                fillColor.a *= fillMask * _FillAlpha;

                fixed4 finalColor = ringColor + fillColor;
                finalColor.a = saturate(ringColor.a + fillColor.a);

                clip(finalColor.a - 0.001);

                return finalColor;
            }
            ENDCG
        }
    }
}
