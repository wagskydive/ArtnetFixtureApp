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
            int _PatternType;
            float _Speed;
            float _Size;
            float _StrobeGate;
            float _BeamRadius;
            float _BeamSoftness;
            float _BeamOffsetX;
            float _BeamOffsetY;
            float _BeamRotation;

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
                float size01 = saturate((safeSize - 0.5) / 7.5);
                float time = _Time.y * _Speed;
                float2 centeredUv = (i.uv - 0.5) * 2.0;

                float sineRot = sin(_BeamRotation);
                float cosineRot = cos(_BeamRotation);
                float2 rotatedUv = float2(
                    (centeredUv.x * cosineRot) - (centeredUv.y * sineRot),
                    (centeredUv.x * sineRot) + (centeredUv.y * cosineRot)
                );

                float2 beamCenterOffset = float2(_BeamOffsetX, _BeamOffsetY);
                float2 maskUv = rotatedUv - beamCenterOffset;
                float radialDistance = length(maskUv);

                float pattern = (float)_PatternType;
                float solidMask = 1.0 - step(0.5, abs(pattern - 0.0));
                float radialMask = 1.0 - step(0.5, abs(pattern - 1.0));
                float pulseMask = 1.0 - step(0.5, abs(pattern - 2.0));
                float barsMask = 1.0 - step(0.5, abs(pattern - 3.0));
                float beamMask = 1.0 - step(0.5, abs(pattern - 4.0));
                float horizontalStripesMask = 1.0 - step(0.5, abs(pattern - 5.0));
                float checkerMask = 1.0 - step(0.5, abs(pattern - 6.0));
                float diagonalWaveMask = 1.0 - step(0.5, abs(pattern - 7.0));
                float outlineMask = 1.0 - step(0.5, abs(pattern - 8.0));
                float verticalWaveMask = 1.0 - step(0.5, abs(pattern - 9.0));
                float ringBandsMask = 1.0 - step(0.5, abs(pattern - 10.0));
                float spiralMask = 1.0 - step(0.5, abs(pattern - 11.0));
                float diamondGridMask = 1.0 - step(0.5, abs(pattern - 12.0));
                float sparkleMask = 1.0 - step(0.5, abs(pattern - 13.0));
                float pinwheelMask = 1.0 - step(0.5, abs(pattern - 14.0));
                float sweepMask = 1.0 - step(0.5, abs(pattern - 15.0));
                float rippleMask = 1.0 - step(0.5, abs(pattern - 16.0));
                float plasmaMask = 1.0 - step(0.5, abs(pattern - 17.0));
                float crossPulseMask = 1.0 - step(0.5, abs(pattern - 18.0));
                float coneMaskPattern = 1.0 - step(0.5, abs(pattern - 19.0));

                float radialBrightness = saturate(1.0 - radialDistance * safeSize);
                float pulseBrightness = 0.5 + 0.5 * sin(time);
                float barsBrightness = step(0.5, frac((rotatedUv.x * safeSize * 4.0) + time));
                float beamBrightness = smoothstep(0.0, 1.0, 1.0 - abs(rotatedUv.x) * safeSize);

                float stripesFrequency = safeSize * 8.0;
                float horizontalStripesBrightness = step(0.5, frac(i.uv.y * stripesFrequency + (time * 0.5)));

                float checkerX = step(0.5, frac(i.uv.x * stripesFrequency + time * 0.25));
                float checkerY = step(0.5, frac(i.uv.y * stripesFrequency));
                float checkerBrightness = abs(checkerX - checkerY);

                float diagonalWaveBrightness = 0.5 + 0.5 * sin(((i.uv.x + i.uv.y) * safeSize * 12.0) + time);

                float edgeDistance = min(min(i.uv.x, 1.0 - i.uv.x), min(i.uv.y, 1.0 - i.uv.y));
                float outlineCoreWidth = 0.02;
                float outlineBlur = lerp(0.01, 0.35, size01);
                float outlineBrightness = 1.0 - smoothstep(outlineCoreWidth, outlineCoreWidth + outlineBlur, edgeDistance);

                float verticalWaveBrightness = 0.5 + 0.5 * sin(i.uv.x * safeSize * 18.0 + time);
                float ringBandsBrightness = 0.5 + 0.5 * sin((radialDistance * safeSize * 22.0) - time * 1.5);

                float spiralAngle = atan2(maskUv.y, maskUv.x);
                float spiralBrightness = 0.5 + 0.5 * sin((spiralAngle * (safeSize * 4.0)) + (radialDistance * 16.0) - time * 2.0);

                float diamondPattern = abs(rotatedUv.x) + abs(rotatedUv.y);
                float diamondGridBrightness = step(0.5, frac((diamondPattern * safeSize * 6.0) + time * 0.25));

                float sparkleHash = frac(sin(dot(i.uv * (safeSize * 64.0 + 8.0), float2(12.9898, 78.233)) + time * 0.5) * 43758.5453);
                float sparkleBrightness = step(0.93, sparkleHash);

                float pinwheelBrightness = step(0.5, frac((spiralAngle / 6.2831853) * safeSize * 16.0 + time * 0.5));
                float sweepBrightness = smoothstep(0.0, 0.35, 1.0 - abs(i.uv.y - frac(time * 0.2)) * safeSize * 2.0);
                float rippleBrightness = 0.5 + 0.5 * sin((radialDistance * safeSize * 36.0) - time * 3.0);
                float plasmaBrightness = 0.5 + 0.25 * sin((i.uv.x * safeSize * 10.0) + time) + 0.25 * cos((i.uv.y * safeSize * 13.0) - time * 1.3);

                float crossPulse = min(abs(maskUv.x), abs(maskUv.y));
                float crossPulseBrightness = smoothstep(0.35, 0.0, crossPulse * safeSize + 0.2 * sin(time * 2.0));

                float coneFalloff = smoothstep(1.0, 0.0, radialDistance * safeSize);
                float coneNoise = 0.5 + 0.5 * sin((radialDistance * 30.0) - time * 2.0);
                float coneMaskBrightness = coneFalloff * coneNoise;

                float brightness =
                    solidMask +
                    (radialMask * radialBrightness) +
                    (pulseMask * pulseBrightness) +
                    (barsMask * barsBrightness) +
                    (beamMask * beamBrightness) +
                    (horizontalStripesMask * horizontalStripesBrightness) +
                    (checkerMask * checkerBrightness) +
                    (diagonalWaveMask * diagonalWaveBrightness) +
                    (outlineMask * outlineBrightness) +
                    (verticalWaveMask * verticalWaveBrightness) +
                    (ringBandsMask * ringBandsBrightness) +
                    (spiralMask * spiralBrightness) +
                    (diamondGridMask * diamondGridBrightness) +
                    (sparkleMask * sparkleBrightness) +
                    (pinwheelMask * pinwheelBrightness) +
                    (sweepMask * sweepBrightness) +
                    (rippleMask * rippleBrightness) +
                    (plasmaMask * plasmaBrightness) +
                    (crossPulseMask * crossPulseBrightness) +
                    (coneMaskPattern * coneMaskBrightness);

                float beamRadius = saturate(_BeamRadius);
                float beamSoftness = max(_BeamSoftness, 0.001);
                float beamCircleMask = 1.0 - smoothstep(beamRadius, beamRadius + beamSoftness, radialDistance);

                float3 finalColor = _Color.rgb * brightness;
                float alpha = saturate(brightness) * beamCircleMask;
                float outputGate = _Intensity * _StrobeGate;

                return fixed4(finalColor * outputGate, alpha * outputGate);
            }
            ENDCG
        }
    }
}
