Shader "Custom/GlowEffect_ChangingHabitat"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineThickness("OutlineThickness", Float) = 3
        _EdgeThickness("EdgeThickness", Float) = 3
        _EdgeHighlight("EdgeHighlight", Float) = 0
        _Highlight("Highlight", Float) = 0
        [HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask("ColorMask", Float) = 15
        [HideInInspector]_UIMaskSoftnessX("UIMaskSoftnessX", Float) = 1
        [HideInInspector]_UIMaskSoftnessY("UIMaskSoftnessY", Float) = 1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }

        Stencil
        {
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            Ref [_Stencil]
            CompFront [_StencilComp]
            PassFront [_StencilOp]
            CompBack [_StencilComp]
            PassBack [_StencilOp]
        }
        
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        
        Pass
        {
            Name "UI"

            
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            //--------------------------------------
            // Structs
            //--------------------------------------

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
            };

            //--------------------------------------
            // Properties
            //--------------------------------------

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _OutlineThickness;
            float _EdgeThickness;
            float _Highlight;
            float _EdgeHighlight;
            float _Stencil;
            float _StencilComp;
            float _StencilOp;
            float _StencilWriteMask;
            float _StencilReadMask;
            float _ColorMask;

            CBUFFER_START(UnityPerMaterial)

                float4 _Color;

            CBUFFER_END

            //--------------------------------------
            // Vertex
            //--------------------------------------

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;
                output.color = input.color * _Color;

                return output;
            }

            //--------------------------------------
            // Fragment
            //--------------------------------------

            float3 HSVToRGB(float3 hsv)
            {
                float3 rgb = saturate(abs(
                    frac(hsv.x + float3(0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0
                ) - 1.0);

                rgb = rgb * rgb * (3.0 - 2.0 * rgb);

                return hsv.z * lerp(
                    float3(1, 1, 1),
                    rgb,
                    hsv.y
                );
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 color =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    );

                color *= input.color;

                // 아웃라인 형성
                float surroundingAlpha = 0.0;

                for (int i = 1; i <= _OutlineThickness; i++)
                {
                    float2 offset = _MainTex_TexelSize.xy * i;

                    surroundingAlpha = max(surroundingAlpha,
                        SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,
                            input.uv + float2( offset.x,  0)).a);

                    surroundingAlpha = max(surroundingAlpha,
                        SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,
                            input.uv + float2(-offset.x,  0)).a);

                    surroundingAlpha = max(surroundingAlpha,
                        SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,
                            input.uv + float2( 0,  offset.y)).a);

                    surroundingAlpha = max(surroundingAlpha,
                        SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,
                            input.uv + float2( 0, -offset.y)).a);

                    float2 diagonalOffset = normalize(float2(1, 1)) * offset.x;

                    surroundingAlpha = max(surroundingAlpha,
                        SAMPLE_TEXTURE2D(
                            _MainTex,
                            sampler_MainTex,
                            input.uv + diagonalOffset
                        ).a);

                    surroundingAlpha = max(surroundingAlpha,
                        SAMPLE_TEXTURE2D(
                            _MainTex,
                            sampler_MainTex,
                            input.uv + float2(-diagonalOffset.x, diagonalOffset.y)
                        ).a);

                    surroundingAlpha = max(surroundingAlpha,
                        SAMPLE_TEXTURE2D(
                            _MainTex,
                            sampler_MainTex,
                            input.uv + float2(diagonalOffset.x, -diagonalOffset.y)
                        ).a);

                    surroundingAlpha = max(surroundingAlpha,
                        SAMPLE_TEXTURE2D(
                            _MainTex,
                            sampler_MainTex,
                            input.uv - diagonalOffset
                        ).a);
                }
                float outline = surroundingAlpha * (1.0 - color.a);

                // 외곽 아웃라인 형성
                float edgeDistanceX = min(
                    input.uv.x,
                    1.0 - input.uv.x
                );

                float edgeDistanceY = min(
                    input.uv.y,
                    1.0 - input.uv.y
                );

                float edgeOutlineX =
                    1.0 - step(
                        _EdgeThickness * _MainTex_TexelSize.x,
                        edgeDistanceX
                    );

                float edgeOutlineY =
                    1.0 - step(
                        _EdgeThickness * _MainTex_TexelSize.y,
                        edgeDistanceY
                    );

                float edgeOutline =
                    max(edgeOutlineX, edgeOutlineY);

                edgeOutline *= (1.0 - color.a);

                // 중심으로부터의 각도에 따른 무지개색 구현
                float2 direction = input.uv - float2(0.5, 0.5);
                float angle = atan2(direction.y, direction.x);
                float hue = frac(angle / (2.0 * PI));

                float3 rainbowColor = HSVToRGB(float3(hue, 1.0, 1.0));

                // uv 기준 사각형 외곽선 방향에 따른 이동하는 무지개색 구현
                float speed = 0.2;
                float rainbowPosition;

                if (edgeOutlineY > 0.0)
                {
                    if (input.uv.y > 0.5)
                    {
                        // 위쪽: 왼쪽 → 오른쪽
                        rainbowPosition = input.uv.x * 0.5;
                    }
                    else
                    {
                        // 아래쪽: 오른쪽 → 왼쪽
                        rainbowPosition = (1.0 - input.uv.x) * 0.5;
                    }
                }
                else
                {
                    if (input.uv.x > 0.5)
                    {
                        // 오른쪽: 위쪽 → 아래쪽
                        rainbowPosition = 0.5 + (1.0 - input.uv.y) * 0.5;
                    }
                    else
                    {
                        // 왼쪽: 아래쪽 → 위쪽
                        rainbowPosition = 0.5 + input.uv.y * 0.5;
                    } 
                }

                rainbowPosition = frac(rainbowPosition + _Time.y * speed);
                float3 rotatingRainbowColor = HSVToRGB(float3(rainbowPosition, 1.0, 1.0));

                float edgeOutlineStrength = edgeOutline * _EdgeHighlight;
                float outlineStrength = outline * _Highlight;

                if (color.a == 0)
                {
                    color = (0, 0, 0, 0);
                }
                
                color += float4(rotatingRainbowColor, 1) * edgeOutlineStrength;
                color += float4(rainbowColor, 1) * outlineStrength;

                return color;
            }

            ENDHLSL
        }
    }
}