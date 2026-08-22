Shader "Custom/UIHighlightEffect"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (1, 1, 1, 1)
        _OutlineColor("OutlineColor", Color) = (1, 1, 1, 1)
        _OutlineThickness("OutlineThickness", Float) = 3
        _UIHighlight("UIHighlight", Float) = 0
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Pass
        {
            Name "UIHighlightEffect"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }

            ColorMask [_ColorMask]
            
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
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            //--------------------------------------
            // Properties
            //--------------------------------------

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)

                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _UIHighlight;

            CBUFFER_END

            //--------------------------------------
            // Vertex
            //--------------------------------------

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv =
                    TRANSFORM_TEX(input.uv, _MainTex);

                output.color =
                    input.color * _Color;

                return output;
            }

            //--------------------------------------
            // Fragment
            //--------------------------------------

            half4 frag(Varyings input) : SV_Target
            {
                half4 color =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    );

                color *= input.color;
                
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
                
                half4 outlineColor = _OutlineColor;

                outlineColor.a *= outline * _UIHighlight;

                color.rgb = lerp(
                    color.rgb,
                    outlineColor.rgb,
                    outlineColor.a
                );

                color.a = max(
                    color.a,
                    outlineColor.a
                );
                
                return color;
            }

            ENDHLSL
        }
    }
}
