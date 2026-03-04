Shader "Custom/MaliSafeLighting"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,1)) = 1
        _PatternType ("Pattern Type", Int) = 0
        _Speed ("Speed", Float) = 1
        _Size ("Size", Float) = 1
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float brightness = 1.0;

                if (_PatternType == 1) // Pulse pattern
                {
                    brightness = abs(sin(_Time.y * _Speed));
                }
                else if (_PatternType == 2) // Color shift
                {
                    float t = _Time.y * _Speed;
                    float r = 0.5f + 0.5f * sin(t);
                    float g = 0.5f + 0.5f * cos(t);
                    float b = 0.5f;
                    // Apply to color channel
                    brightness = 1.0;
                    // ... additional pattern logic
                }
                return fixed4(_Color * brightness * _Intensity, 1);
            }
            ENDCG
        }
    }
}

