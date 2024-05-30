Shader "VRPaint/BaseLayerShader"
{

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

// Pass to render object as a shadow caster, required to write to depth texture
		Pass 
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			Fog {Mode Off}
			ZWrite On ZTest LEqual Cull Off
			Offset 1, 1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f 
			{ 
				V2F_SHADOW_CASTER;
			};

			v2f vert( appdata_base v )
			{
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			float4 frag( v2f i ) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG

		}

        Pass
        {
            ZTest LEqual
            //ColorMask 0
            LOD 100
            Blend Off
            Cull Off
            ZWrite On

            Stencil
            {
                Ref [_GroupID]
                WriteMask 63
                Comp Always
                Pass Replace
                Fail Keep
            }    

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"

            uint _GroupID;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                //float4 objPos : TEXCOORD0;
                //V2F_SHADOW_CASTER;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.objPos = v.vertex;
                o.color = v.color;
                //TRANSFER_SHADOW_CASTER(o)

                return o;
            }



            fixed4 frag (v2f i) : SV_Target
            {
                //SHADOW_CASTER_FRAGMENT(i)
                return i.color;
            }
            ENDCG
        }
    }
    // Pass to render object as a shadow caster, required to write to depth texture
    // https://forum.unity.com/threads/why-isnt-an-object-with-my-custom-material-applied-drawn-on-camera-depth-texture.1390741/#post-8835097
    //FallBack "Legacy Shaders/VertexLit"
}
