Shader "VRPaint/DebugHighlight"
{

    SubShader
    {
        Pass
        {
            ZTest GEqual
            LOD 100
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZWrite Off

            Stencil
            {
                Ref 0
                WriteMask 128 // Write to the 1st highest bit
                ReadMask 128
                Comp Equal
                //Pass Keep
                Pass Invert // This prevents redrawing fragments for the same stroke => prevents intra-stroke alpha blending
                //Fail Zero
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 objPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.objPos = v.vertex;
                o.color = v.color;
                return o;
            }



            fixed4 frag(v2f i) : SV_Target
            {
                float4 c = i.color;
                c.a *= 0.5f;
                return c;
            }
            ENDCG
        }

        Pass
        {
            ZTest LEqual
            LOD 100
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 objPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.objPos = v.vertex;
                o.color = v.color;
                return o;
            }



            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
