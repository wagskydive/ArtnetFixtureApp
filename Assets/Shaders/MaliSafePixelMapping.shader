Shader "Custom/MaliSafePixelMapping"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,1)) = 1
        _StrobeGate ("Strobe Gate", Range(0,1)) = 1
        _Rows ("Pixel Rows", Range(1,32)) = 8
        _Columns ("Pixel Columns", Range(1,32)) = 8
        _PixelDataTex ("Pixel Data Texture", 2D) = "white" {}
        _UsePixelDataTex ("Use Pixel Data Texture", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

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
            float _StrobeGate;
            float _Rows;
            float _Columns;
            sampler2D _PixelDataTex;
            float _UsePixelDataTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float rows = max(1.0, floor(_Rows + 0.5));
                float columns = max(1.0, floor(_Columns + 0.5));
                float2 gridSize = float2(columns, rows);

                float2 cellIndex = floor(i.uv * gridSize);
                float2 quantizedUv = (cellIndex + 0.5) / gridSize;

                float3 pixelDataColor = tex2D(_PixelDataTex, quantizedUv).rgb;
                float3 proceduralPixelColor = _Color.rgb;
                float3 gridColor = lerp(proceduralPixelColor, pixelDataColor, saturate(_UsePixelDataTex));

                float outputGate = saturate(_Intensity) * saturate(_StrobeGate);
                float3 finalRgb = gridColor * outputGate;
                float finalAlpha = outputGate;

                return fixed4(finalRgb, finalAlpha);
            }
            ENDCG
        }
    }
}
