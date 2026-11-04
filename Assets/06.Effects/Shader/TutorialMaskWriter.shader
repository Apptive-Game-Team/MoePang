Shader "Hidden/TutorialMaskWriter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Opacity ("Opacity", Float) = 1
        _UseSDF ("Use SDF", Float) = 0
        _SDFThreshold ("SDF Threshold", Float) = 0.5
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        // 겹치는 영역의 밝은 값을 유지합니다.
        Blend One One
        BlendOp Max

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Opacity;
            float _UseSDF;
            float _SDFThreshold;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float alpha : TEXCOORD1;
            };

            v2f vert(appdata input)
            {
                v2f output;

                // 스크립트에서 전달받는 위치는 DIM 내부의 0~1 좌표입니다.
                output.vertex = float4(
                    input.vertex.xy * 2.0 - 1.0,
                    0.0,
                    1.0
                );

                #if UNITY_UV_STARTS_AT_TOP
                    output.vertex.y = -output.vertex.y;
                #endif

                output.uv = input.uv;
                output.alpha = input.color.a;

                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float coverage = tex2D(_MainTex, input.uv).a;
                if (_UseSDF > 0.5)
                {
                    float edge = max(fwidth(coverage), 0.0001);
                    coverage = smoothstep(
                        _SDFThreshold - edge, _SDFThreshold + edge, coverage);
                }
                float alpha = coverage * input.alpha * _Opacity;

                return float4(alpha, alpha, alpha, alpha);
            }

            ENDCG
        }
    }
}
