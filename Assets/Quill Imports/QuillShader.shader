Shader "Unlit/Quill Shader"
{
	Properties
    {
        [Toggle] _EnableTransparency("Enable Transparency", Int) = 1
        [Enum(Yes,0,No,2)] _Cull("Double Sided", Int) = 2
        [Toggle] _EnableFog("Enable Fog", Int) = 0
    }

    SubShader
    {
        Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100
        Blend Off
        Cull[_Cull]
        ZWrite On
        ZTest On

        Pass
        {
            AlphaToMask[_EnableTransparency]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #pragma shader_feature _ENABLEFOG_ON
            #pragma shader_feature _ENABLETRANSPARENCY_ON
            #include "UnityCG.cginc"

            struct inVertex
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                #ifdef _ENABLEFOG_ON
                UNITY_FOG_COORDS(1)

                #endif

                #ifdef _ENABLETRANSPARENCY_ON

                float4 screenPos : TEXCOORD2;

                #endif
                float4 color : COLOR;
            };

            v2f vert(inVertex v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                #ifdef _ENABLEFOG_ON
                UNITY_TRANSFER_FOG(o, o.vertex);

                #endif

                #ifdef _ENABLETRANSPARENCY_ON

                o.screenPos = ComputeScreenPos(o.vertex);

                #endif
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = i.color;

                #ifdef _ENABLEFOG_ON
                UNITY_APPLY_FOG(i.fogCoord, col);
                #endif

                #ifdef _ENABLETRANSPARENCY_ON
                float2 pos = _ScreenParams.xy * i.screenPos.xy / i.screenPos.w;
                const int MSAASampleCount = 8;
                float ran = frac(52.9829189*frac(dot(pos, float2(0.06711056,0.00583715))));
                col.a = clamp(col.a + 0.99*(ran-0.5)/float(MSAASampleCount), 0.0, 1.0);
                #endif

                return col;
            }
            ENDCG
        }

        // extra pass that renders to depth buffer only
        Pass {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0
        }

    }
    //Fallback "VertexLit"
}
