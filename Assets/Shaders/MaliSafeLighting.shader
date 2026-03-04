Shader "Custom/MaliSafeLighting"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,1)) = 1
        _PatternType ("Pattern Type", Int) = 0
        _Speed ("Speed", Float) = 1
        _Size ("Size", Float) = 1
        _StrobeGate ("Strobe Gate", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _Intensity;
            int _PatternType;
            float _Speed;
            float _Size;
            float _StrobeGate;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float safeSize = max(_Size, 0.01);
                float time = _Time.y * _Speed;
                float2 centeredUv = (i.uv - 0.5) * 2.0;
                float radialDistance = length(centeredUv);

                // Build branchless pattern masks to avoid divergent control flow on mobile GPUs.
                float pattern = (float)_PatternType;
                float solidMask = 1.0 - step(0.5, abs(pattern - 0.0));
                float linearMask = 1.0 - step(0.5, abs(pattern - 1.0));
                float radialMask = 1.0 - step(0.5, abs(pattern - 2.0));
                float pulseMask = 1.0 - step(0.5, abs(pattern - 3.0));
                float barsMask = 1.0 - step(0.5, abs(pattern - 4.0));
                float beamMask = 1.0 - step(0.5, abs(pattern - 5.0));

                float linearBrightness = saturate((i.uv.x - 0.5) * safeSize + 0.5);
                float radialBrightness = saturate(1.0 - radialDistance * safeSize);
                float pulseBrightness = 0.5 + 0.5 * sin(time);
                float barsBrightness = step(0.5, frac(i.uv.x * safeSize * 8.0 + time));
                float beamBrightness = smoothstep(0.0, 1.0, 1.0 - abs(centeredUv.x) * safeSize);

                float brightness =
                    solidMask +
                    (linearMask * linearBrightness) +
                    (radialMask * radialBrightness) +
                    (pulseMask * pulseBrightness) +
                    (barsMask * barsBrightness) +
                    (beamMask * beamBrightness);

                return fixed4(_Color.rgb * brightness * _Intensity * _StrobeGate, 1);
            }
            ENDCG
        }
    }
}
