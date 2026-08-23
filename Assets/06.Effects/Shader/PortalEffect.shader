Shader "Custom/PortalEffect"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Portal Color", Color) = (0.05, 0.5, 1.0, 1.0)

        _BottomRadius ("Bottom Radius", Range(0.01, 0.5)) = 0.12
        _TopRadius ("Top Radius", Range(0.01, 0.5)) = 0.42

        _BodyAlpha ("Body Alpha", Range(0.0, 1.0)) = 0.12
        _BodyGlowSpeed ("Body Glow Speed", Range(0.0, 5.0)) = 1.0

        _WaveSpeed ("Wave Speed", Range(0.0, 2.0)) = 0.3
        _WaveCount ("Wave Count", Range(1, 6)) = 3
        _WaveThickness ("Wave Thickness", Range(0.005, 0.2)) = 0.025
        _WaveBrightness ("Wave Brightness", Range(0.0, 10.0)) = 4.0
        _WaveVerticalScale ("Wave Vertical Scale", Range(0.05, 1.0)) = 0.25

        _RingGlow ("Ring Glow", Range(0.0, 5.0)) = 0.5
        _RingGlowSoftness ("Ring Glow Softness", Range(0.01, 1.0)) = 0.25
        
        _ColumnCount ("Column Count", Range(1, 20)) = 8
        _ColumnWidth ("Column Width", Range(0.001, 0.05)) = 0.012
        _ColumnLength ("Column Length", Range(0.1, 2.0)) = 0.8
        _ColumnBrightness ("Column Brightness", Range(0.0, 20.0)) = 8.0
        _ColumnAngleRandomness ("Column Angle Randomness", Range(0.0, 1.0)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "PortalEffect"

            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)

                float4 _Color;

                float _BottomRadius;
                float _TopRadius;

                float _BodyAlpha;
                float _BodyGlowSpeed;

                float _WaveSpeed;
                float _WaveCount;
                float _WaveThickness;
                float _WaveBrightness;
                float _WaveVerticalScale;

                float _RingGlow;
                float _RingGlowSoftness;
            
                float _ColumnCount;
                float _ColumnWidth;
                float _ColumnLength;
                float _ColumnBrightness;
                float _ColumnAngleRandomness;   

            CBUFFER_END

            float GetConeRadius(float height)
            {
                return lerp(_BottomRadius, _TopRadius, height);
            }
            
            float Hash(float n)
            {
                return frac(sin(n * 12.9898) * 43758.5453);
            }
            
            float GetLightColumns(float2 uv)
            {
                float result = 0.0;
                const int MAX_COLUMNS = 20;
                float saturatedBottomRadius = _BottomRadius * 2.0 / 3.0;

                for (int i = 0; i < MAX_COLUMNS; i++)
                {
                    float index = (float)i;
                    float enabled = step(index, _ColumnCount - 1.0);

                    float lifetime = lerp(1.5, 3.0, Hash(index * 101.73f));
                    float phase = Hash(index * 121.37f);

                    float cycleTime = _Time.y / lifetime + phase;
                    float cycle = floor(cycleTime);
                    float time = frac(cycleTime);

                    float randomRadius = sqrt(Hash(index * 17.31f + cycle * 43.17f));
                    float randomAngle = Hash(index * 41.72f + cycle * 71.23f) * 6.2831853;

                    float startX = 0.5 + cos(randomAngle) * saturatedBottomRadius * randomRadius;
                    float startY = sin(randomAngle) * saturatedBottomRadius * randomRadius * _WaveVerticalScale * 0.5;
                    float2 start = float2(startX, startY);

                    float horizontalDirection = sign(startX - 0.5);
                    float distanceFromCenter = abs(startX - 0.5) / max(saturatedBottomRadius, 0.0001);
                    float baseAngle = horizontalDirection * lerp(0.03, 0.22, distanceFromCenter);

                    float angleOffset = (Hash(index * 53.21f + cycle * 17.43f) - 0.5) * _ColumnAngleRandomness;
                    float angle = baseAngle + horizontalDirection * angleOffset;
                    float2 direction = normalize(float2(sin(angle), cos(angle)));

                    float2 position = uv - start;
                    float along = dot(position, direction);
                    float perpendicularPosition = dot(position, float2(-direction.y, direction.x));
                    float perpendicular = abs(perpendicularPosition);

                    float maxLength = _ColumnLength * lerp(0.8, 1.2, Hash(index * 71.43f + cycle * 31.71f));
                    float progress = saturate(along / maxLength);

                    float widthFade = 1.0 - smoothstep(0.0, 1.0, progress);
                    float width = _ColumnWidth * lerp(0.7, 1.0, widthFade);

                    float column = 1.0 - smoothstep(width * 0.1, width, perpendicular);

                    float startFade = smoothstep(0.0, 0.08, along);
                    float endFade = 1.0 - smoothstep(maxLength * 0.35, maxLength, along);

                    column *= startFade * endFade;

                    float appear = smoothstep(0.0, 0.12, time);
                    float disappear = 1.0 - smoothstep(0.78, 1.0, time);
                    float lifeAlpha = appear * disappear;

                    float flickerSpeed = lerp(4.0, 8.0, Hash(index * 91.37f + cycle * 27.51f));
                    float flickerPhase = Hash(index * 31.17f + cycle * 83.21f);

                    float flicker = sin(_Time.y * flickerSpeed + flickerPhase * 6.2831853);
                    flicker = lerp(0.5, 1.0, flicker * 0.5 + 0.5);

                    column *= lifeAlpha * flicker;

                    float height = saturate(uv.y);
                    float radius = GetConeRadius(height);
                    float coneMask = 1.0 - step(radius, abs(uv.x - 0.5));

                    column *= coneMask;
                    column *= enabled;

                    result += column;
                }

                return saturate(result);
            }
            
            float GetBodyColumn(float2 uv)
            {
                float height = saturate(uv.y);
                float radius = GetConeRadius(height);

                float horizontal = abs(uv.x - 0.5f);

                float coneMask = 1.0f - smoothstep(radius * 0.8f, radius, horizontal);

                float normalizedX = horizontal / max(radius, 0.0001f);

                float endHeight = 0.95f - normalizedX * normalizedX * 0.3f;

                float endFade = 1.0f - smoothstep(endHeight - 0.12f, endHeight, height);

                float startFade = smoothstep(0.0f, 0.08f, height);

                float body = coneMask * startFade * endFade;

                float flicker1 = sin(_Time.y * _BodyGlowSpeed);
                float flicker2 = sin(_Time.y * _BodyGlowSpeed * 1.73f + 2.4f);
                float flicker = 0.75f + flicker1 * 0.1f + flicker2 * 0.1f;

                body *= flicker;

                return body;
            }
            
            float GetBodyGlow(float2 uv)
            {
                float height = saturate(uv.y);
                float radius = GetConeRadius(height);

                float horizontal = 1.0 - smoothstep(radius * 0.2, radius, abs(uv.x - 0.5));

                float bottomFade = smoothstep(0.0, 0.08, height);
                float topFade = 1.0 - smoothstep(0.8, 1.0, height);

                float bodyMask = horizontal * bottomFade * topFade;

                float flicker1 = sin(_Time.y * _BodyGlowSpeed);
                float flicker2 = sin(_Time.y * _BodyGlowSpeed * 1.73 + 2.4);

                float flicker = 0.7 + flicker1 * 0.15 + flicker2 * 0.1;

                return bodyMask * flicker;
            }

            float GetPortalGlow(float2 uv)
            {
                float2 p = uv - 0.5;
                float radius = GetConeRadius(uv.y);

                float horizontal = 1.0 - smoothstep(0.0, radius, abs(p.x));

                float vertical = smoothstep(0.0, 0.15, uv.y);
                vertical *= 1.0 - smoothstep(0.75, 1.0, uv.y);

                float glow = horizontal * vertical;
                glow = pow(glow, 2.5);

                return glow;
            }

            float GetRing(float2 uv, float ringHeight)
            {
                float2 center = float2(0.5, ringHeight);
                float2 p = uv - center;

                float radius = GetConeRadius(ringHeight);
                float verticalRadius = radius * _WaveVerticalScale;

                float2 ellipse = float2(
                    p.x / max(radius, 0.0001),
                    p.y / max(verticalRadius, 0.0001)
                );

                float distance = length(ellipse);

                float ring =
                    step(1.0 - _WaveThickness, distance) *
                    step(distance, 1.0);

                float startFade = smoothstep(0.0, 0.08, ringHeight);

                float edgeFade = 1.0 - smoothstep(
                    0.72,
                    0.95,
                    ringHeight + verticalRadius
                );

                float heightFade = startFade * edgeFade;

                float brightness = lerp(0.7, 1.0, ringHeight);

                return ring * heightFade * brightness;
            }

            float GetRingGlow(float2 uv, float ringHeight)
            {
                float2 center = float2(0.5, ringHeight);
                float2 p = uv - center;

                float radius = GetConeRadius(ringHeight);
                float verticalRadius = radius * _WaveVerticalScale;

                float2 ellipse = float2(
                    p.x / max(radius, 0.0001),
                    p.y / max(verticalRadius, 0.0001)
                );

                float distance = length(ellipse);

                float glow = 1.0 - smoothstep(
                    0.8,
                    1.0 + _RingGlowSoftness,
                    distance
                );

                float startFade = smoothstep(0.0, 0.08, ringHeight);
                float endFade = 1.0 - smoothstep(0.75, 1.0, ringHeight);
                float heightFade = startFade * endFade;

                return glow * heightFade;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float height = saturate(uv.y);

                // uv.x += GetDistortion(uv);
                
                float bodyColumn = GetBodyColumn(uv);
                float lightColumns = GetLightColumns(uv);

                float waves = 0.0;
                float ringGlow = 0.0;

                const int MAX_WAVES = 6;

                for (int i = 0; i < MAX_WAVES; i++)
                {
                    float index = (float)i;
                    float progress = frac(_Time.y * _WaveSpeed + index / _WaveCount);
                    float enabled = step(index, _WaveCount - 1.0);

                    float ring = GetRing(uv, progress);
                    float glow = GetRingGlow(uv, progress);

                    waves += ring * enabled;
                    ringGlow += glow * enabled;
                }

                waves = saturate(waves);
                ringGlow = saturate(ringGlow);

                float3 portalColor = _Color.rgb;

                float3 bodyColor = portalColor * bodyColumn * _BodyAlpha;
                float3 ringGlowColor = portalColor * ringGlow * _RingGlow;
                float3 waveColor = portalColor * waves * _WaveBrightness;
                float3 columnColor = portalColor * lightColumns * _ColumnBrightness;

                float3 finalColor = bodyColor + ringGlowColor + waveColor + columnColor;

                float finalAlpha = 0;
                finalAlpha = max(finalAlpha, ringGlow * 0.15);
                finalAlpha = max(finalAlpha, waves);
                finalAlpha = max(finalAlpha, lightColumns);
                finalAlpha = max(finalAlpha, bodyColumn);
                finalAlpha *= input.color.a;

                return half4(finalColor, saturate(finalAlpha));
            }

            ENDHLSL
        }
    }
}