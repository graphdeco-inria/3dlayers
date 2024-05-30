Shader "VRPaint/GridHighlight"
{
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        // extra pass that renders to depth buffer only
        Pass {
            ZWrite On
            ColorMask 0
        }

        Pass
        {
            ZTest LEqual
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Highlight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                //float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 objPos : TEXCOORD0;
            };

            float _TimeFreq;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;

                float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                    );

                o.objPos = v.vertex * float4(scale, 1.0);

                return o;
            }



            fixed4 frag(v2f i) : SV_Target
            {

                float4 gridColor = HighlightGrid(i.objPos, _Time.w, ContrastingGreyscaleColor(i.color), _TimeFreq);
                //float4 gridColor = HighlightGrid(i.objPos, _Time.w, float4(1.0, 1.0, 0.0, 1.0), _TimeFreq);

                float4 baseColor = i.color;
                baseColor.a = 0.3 * (1.0 - gridColor.a);
                
                //gridColor.a = gridOpacity;
                return gridColor + baseColor;
            }
            ENDCG
        }
    }
}
