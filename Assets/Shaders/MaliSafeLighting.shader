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
                float time = _Time.y * _Speed;
                float2 centeredUv = (i.uv - 0.5) * 2.0;
                float radialDistance = length(centeredUv);

                float3 patternColor = _Color.rgb;
                float brightness = 1.0;

                // 0 - Solid color
                if (_PatternType == 1)
                {
                    // 1 - Linear gradient
                    float gradient = saturate((i.uv.x - 0.5) * _Size + 0.5);
                    brightness = gradient;
                }
                else if (_PatternType == 2)
                {
                    // 2 - Radial gradient
                    brightness = saturate(1.0 - radialDistance * _Size);
                }
                else if (_PatternType == 3)
                {
                    // 3 - Pulse
                    brightness = 0.5 + 0.5 * sin(time);
                }
                else if (_PatternType == 4)
                {
                    // 4 - Moving bars
                    float bars = frac(i.uv.x * max(_Size, 0.01) * 8.0 + time);
                    brightness = step(0.5, bars);
                }
                else if (_PatternType == 5)
                {
                    // 5 - Soft edge beam
                    float beam = 1.0 - abs(centeredUv.x) * _Size;
                    brightness = smoothstep(0.0, 1.0, beam);
                }

                return fixed4(patternColor * brightness * _Intensity * _StrobeGate, 1);
            }
            ENDCG
        }
    }
}
