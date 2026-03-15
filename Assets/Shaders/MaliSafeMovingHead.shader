Shader "Custom/MaliSafeMovingHead"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,1)) = 1
        _PatternType ("Pattern Type", Int) = 0
        _Speed ("Speed", Float) = 1
        _Size ("Size", Float) = 1
        _StrobeGate ("Strobe Gate", Range(0,1)) = 1

        _BeamRadius ("Beam Radius", Range(0,1)) = 0.35
        _BeamSoftness ("Beam Softness", Range(0.001,0.5)) = 0.08

        _BeamOffsetX ("Beam Offset X", Range(-1,1)) = 0
        _BeamOffsetY ("Beam Offset Y", Range(-1,1)) = 0
        _BeamRotation ("Beam Rotation", Range(0,6.2831853)) = 0

        _GoboTex("Gobo Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _GoboTex;

            fixed4 _Color;
            float _Intensity;
            int _PatternType;
            float _Speed;
            float _Size;
            float _StrobeGate;

            float _BeamRadius;
            float _BeamSoftness;
            float _BeamOffsetX;
            float _BeamOffsetY;
            float _BeamRotation;

            //--------------------------------------------
            // Vertex
            //--------------------------------------------

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2x2 rotMatrix : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                // precompute rotation matrix
                float s = sin(_BeamRotation);
                float c = cos(_BeamRotation);
                o.rotMatrix = float2x2(c, -s, s, c);

                return o;
            }

            //--------------------------------------------
            // Beam Transform
            //--------------------------------------------

            float2 GetBeamSpaceUV(float2 uv, float2x2 rotMatrix)
            {
                float2 centered = (uv - 0.5) * 2.0;
                float2 offset = float2(_BeamOffsetX, _BeamOffsetY);
                float2 shifted = centered - offset;
                return mul(rotMatrix, shifted);
            }

            //--------------------------------------------
            // Beam Mask
            //--------------------------------------------

            float BeamMask(float2 beamUV)
            {
                float radius = saturate(_BeamRadius);
                float softness = max(_BeamSoftness, 0.001);
                float d = length(beamUV);
                return 1.0 - smoothstep(radius, radius + softness, d);
            }

            //--------------------------------------------
            // Pattern Generator
            //--------------------------------------------

            float PatternBrightness(float2 beamUV, float2 uv, float radialDist, float time, float size)
            {
                float pattern = (float)_PatternType;

                // Masks
                float solidMask = 1.0 - step(0.5, abs(pattern - 0.0));
                float goboMask = 1.0 - step(0.5, abs(pattern - 1.0));
                float radialMask = 1.0 - step(0.5, abs(pattern - 2.0));
                float pulseMask = 1.0 - step(0.5, abs(pattern - 3.0));
                float barsMask = 1.0 - step(0.5, abs(pattern - 4.0));
                float beamMaskPattern = 1.0 - step(0.5, abs(pattern - 5.0));
                float horizontalStripesMask = 1.0 - step(0.5, abs(pattern - 6.0));
                float checkerMask = 1.0 - step(0.5, abs(pattern - 7.0));
                float diagonalWaveMask = 1.0 - step(0.5, abs(pattern - 8.0));
                float outlineMask = 1.0 - step(0.5, abs(pattern - 9.0));
                float verticalWaveMask = 1.0 - step(0.5, abs(pattern - 10.0));
                float ringBandsMask = 1.0 - step(0.5, abs(pattern - 11.0));
                float spiralMask = 1.0 - step(0.5, abs(pattern - 12.0));
                float diamondGridMask = 1.0 - step(0.5, abs(pattern - 13.0));
                float sparkleMask = 1.0 - step(0.5, abs(pattern - 14.0));
                float pinwheelMask = 1.0 - step(0.5, abs(pattern - 15.0));
                float sweepMask = 1.0 - step(0.5, abs(pattern - 16.0));
                float rippleMask = 1.0 - step(0.5, abs(pattern - 17.0));
                float plasmaMask = 1.0 - step(0.5, abs(pattern - 18.0));
                float crossPulseMask = 1.0 - step(0.5, abs(pattern - 19.0));
                float coneMaskPattern = 1.0 - step(0.5, abs(pattern - 20.0));

                // Standard brightness calculations
                float radialBrightness = saturate(1.0 - radialDist * size);
                float pulseBrightness = 0.5 + 0.5 * sin(time);
                float barsBrightness = step(0.5, frac(beamUV.x * size * 4.0 + time));
                float beamBrightness = smoothstep(0.0, 1.0, 1.0 - abs(beamUV.x) * size);

                float stripesFrequency = size * 8.0;
                float horizontalStripesBrightness = step(0.5, frac(uv.y * stripesFrequency + time * 0.5));

                float checkerX = step(0.5, frac(uv.x * stripesFrequency + time * 0.25));
                float checkerY = step(0.5, frac(uv.y * stripesFrequency));
                float checkerBrightness = abs(checkerX - checkerY);

                float diagonalWaveBrightness = 0.5 + 0.5 * sin((uv.x + uv.y) * size * 12.0 + time);

                float edgeDistance = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float outlineCoreWidth = 0.02;
                float outlineBlur = lerp(0.01, 0.35, (size - 0.5) / 7.5);
                float outlineBrightness = 1.0 - smoothstep(outlineCoreWidth, outlineCoreWidth + outlineBlur, edgeDistance);

                float verticalWaveBrightness = 0.5 + 0.5 * sin(uv.x * size * 18.0 + time);
                float ringBandsBrightness = 0.5 + 0.5 * sin(radialDist * size * 22.0 - time * 1.5);

                float spiralAngle = atan2(beamUV.y, beamUV.x);
                float spiralBrightness = 0.5 + 0.5 * sin(spiralAngle * (size * 4.0) + radialDist * 16.0 - time * 2.0);

                float diamondPattern = abs(beamUV.x) + abs(beamUV.y);
                float diamondGridBrightness = step(0.5, frac(diamondPattern * size * 6.0 + time * 0.25));

                float sparkleHash = frac(sin(dot(uv * (size * 64.0 + 8.0), float2(12.9898, 78.233)) + time * 0.5) * 43758.5453);
                float sparkleBrightness = step(0.93, sparkleHash);

                float pinwheelBrightness = step(0.5, frac((spiralAngle / 6.2831853) * size * 16.0 + time * 0.5));
                float sweepBrightness = smoothstep(0.0, 0.35, 1.0 - abs(uv.y - frac(time * 0.2)) * size * 2.0);
                float rippleBrightness = 0.5 + 0.5 * sin(radialDist * size * 36.0 - time * 3.0);
                float plasmaBrightness = 0.5 + 0.25 * sin(uv.x * size * 10.0 + time) + 0.25 * cos(uv.y * size * 13.0 - time * 1.3);

                float crossPulse = min(abs(beamUV.x), abs(beamUV.y));
                float crossPulseBrightness = smoothstep(0.35, 0.0, crossPulse * size + 0.2 * sin(time * 2.0));

                float coneFalloff = smoothstep(1.0, 0.0, radialDist * size);
                float coneNoise = 0.5 + 0.5 * sin(radialDist * 30.0 - time * 2.0);
                float coneMaskBrightness = coneFalloff * coneNoise;

                // Gobo sampling scaled by _Size
                float2 goboUV = beamUV * (size * 0.5) + 0.5;
                float goboSample = tex2D(_GoboTex, goboUV).a;

                // Combine all patterns
                float brightness =
                    solidMask +
                    goboMask * goboSample +
                    radialMask * radialBrightness +
                    pulseMask * pulseBrightness +
                    barsMask * barsBrightness +
                    beamMaskPattern * beamBrightness +
                    horizontalStripesMask * horizontalStripesBrightness +
                    checkerMask * checkerBrightness +
                    diagonalWaveMask * diagonalWaveBrightness +
                    outlineMask * outlineBrightness +
                    verticalWaveMask * verticalWaveBrightness +
                    ringBandsMask * ringBandsBrightness +
                    spiralMask * spiralBrightness +
                    diamondGridMask * diamondGridBrightness +
                    sparkleMask * sparkleBrightness +
                    pinwheelMask * pinwheelBrightness +
                    sweepMask * sweepBrightness +
                    rippleMask * rippleBrightness +
                    plasmaMask * plasmaBrightness +
                    crossPulseMask * crossPulseBrightness +
                    coneMaskPattern * coneMaskBrightness;

                return brightness;
            }

            //--------------------------------------------
            // Fragment
            //--------------------------------------------

            fixed4 frag(v2f i) : SV_Target
            {
                float safeSize = max(_Size, 0.01);
                float time = _Time.y * _Speed;

                float2 beamUV = GetBeamSpaceUV(i.uv, i.rotMatrix);
                float radialDist = length(beamUV);

                float brightness = PatternBrightness(
                    beamUV,
                    i.uv,
                    radialDist,
                    time,
                    safeSize
                );

                float mask = BeamMask(beamUV);

                float3 color = _Color.rgb * brightness;
                float gate = _Intensity * _StrobeGate;

                return float4(color * gate, saturate(brightness) * mask * gate);
            }

            ENDCG
        }
    }
}