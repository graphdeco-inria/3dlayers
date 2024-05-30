// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VRPaint/ClippedLayerShader"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        LOD 100

        // Pass that clears out the stencil
        Pass {
            ZTest Always
            ZWrite Off
            Cull Off
            ColorMask 0

            Stencil
            {
                WriteMask 192
                Ref 64
                //WriteMask 224
                Comp Always
                Pass Replace
                Fail Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 0.0;
            }
            ENDCG
        }

        Pass {
            ZTest GEqual
            ZWrite Off
            Cull Front
            ColorMask 0

            // Write to stencil buffer only if the z test succeeds (ie, fragment is behind scene)
            Stencil
            {
                //Ref 128
                //WriteMask 128 // Write to the 1st highest bit
                Ref 192
                WriteMask 192 // Write to the 1st and 2nd highest bits
                Comp Always
                Pass Replace
                ZFail Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            uint _StencilOp;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 0.0;
            }
            ENDCG
        }

        // Intersection pass
        Pass
        {
            ZTest LEqual
            //ZTest [_ZTest]
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            BlendOp Add

            Stencil
            {
                // Ref 192 + [_GroupIDIntersect] (but we do this addition in C#, so _GroupIDIntersect = 192 + ACTUAL_GROUP_UID)
                Ref[_GroupIDIntersect]
                ReadMask 255
                WriteMask 64 // write to 2nd highest bit (forbid further drawing on this fragment for this stroke)
                Comp[_StencilComparison]
                //Pass Keep
                Pass Invert // This prevents redrawing fragments for the same stroke => prevents intra-stroke alpha blending
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
            };


            // Stencil comparison op
            uint _StencilComparison;

            // Layer type parameters
            uint _ZTest;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                return i.color;

            }
            ENDCG
        }

    }
        
}
