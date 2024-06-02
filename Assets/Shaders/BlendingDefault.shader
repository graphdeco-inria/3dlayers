Shader "VRPaint/BlendingDefault"
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	
	Subshader {
		Pass {
			ZTest Off 
			Cull Off 
			ZWrite Off
			//Blend SrcAlpha OneMinusSrcAlpha
			Blend One OneMinusSrcAlpha


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#include "BlendModes.cginc"


			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;

			};
	
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;


			// Base Color texture
			sampler2D _BaseLayerColorTex;
			float4 _BaseLayerColorTex_TexelSize;

			// Opacity
			float _LayerOpacity;

			// Color mix modes
            int _ColorMixMode;

			
			v2f vert (appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				o.uv.xy = v.texcoord.xy;

				return o;
			}
	
			half4 frag (v2f i) : COLOR {

				// sample the base color texture
				float2 baseColorUV = i.uv;
				float4 baseColor = tex2D (_BaseLayerColorTex, i.uv);

				float4 blendColor = tex2D(_MainTex, i.uv);

				blendColor *= _LayerOpacity;

				float4 finalColor = Blend(baseColor, blendColor, _ColorMixMode);

				return finalColor;

				// Colors are already alpha premultiplied (we do this upon rendering each brush stroke, see ClippedLayerShader)
				//float3 preMultRGB = preAlphaColor.a * preAlphaColor.rgb;

				//return float4(preMultRGB, preAlphaColor.a);
			}



			ENDCG
		}
	}

	Fallback off
}
