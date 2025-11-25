Shader "Custom/UIOutlineGlow"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,1,1,1)
        _OutlineThickness ("Outline Thickness", Float) = 0.05
        _GlowSpeed ("Glow Speed", Float) = 4.0
        _GlowIntensity ("Glow Intensity", Float) = 1.0

        _Highlight ("Highlight Amount", Float) = 0.0

        // НЕОНОВАЯ ЛИНИЯ
        _ScanEnabled ("Scan Enabled", Float) = 0.0
        _ScanSpeed ("Scan Speed", Float) = 1.0
        _ScanWidth ("Scan Width", Float) = 0.1
        _ScanBrightness ("Scan Brightness", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        // ---------- PASS 1: OUTLINE + SCAN LINE ----------
        Pass
        {
            Name "OUTLINE"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _GlowSpeed;
            float _GlowIntensity;
            float _Highlight;

            float _ScanEnabled;
            float _ScanSpeed;
            float _ScanWidth;
            float _ScanBrightness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // Увеличение формы для контура
                float3 scaled = v.vertex.xyz;
                scaled.xy *= (1.0 + _OutlineThickness);

                o.pos = UnityObjectToClipPos(float4(scaled, 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Учитываем прозрачность спрайта — без квадрата
                float alpha = tex2D(_MainTex, i.uv).a;
                clip(alpha - 0.01);

                // Пульсирующий glow
                float glow = (sin(_Time.y * _GlowSpeed) * 0.5 + 0.5) * _GlowIntensity;
                float glowAlpha = _OutlineColor.a * glow * saturate(_Highlight);

                float scan = 0;

                // НЕОНОВАЯ ЛИНИЯ
                if (_ScanEnabled > 0.5)
                {
                    float pos = frac(_Time.y * _ScanSpeed);
                    float dist = abs(i.uv.y - pos);
                    scan = saturate(1.0 - dist / _ScanWidth) * _ScanBrightness;
                }

                float finalAlpha = glowAlpha + scan;

                return fixed4(_OutlineColor.rgb, finalAlpha);
            }
            ENDCG
        }

        // ---------- PASS 2: MAIN SPRITE ----------
        Pass
        {
            Name "SPRITE"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                return c;
            }
            ENDCG
        }
    }
}
