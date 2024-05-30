Shader "VRPaint/Highlight"
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
			Blend SrcAlpha OneMinusSrcAlpha


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#include "BlendModes.cginc"


			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;

			};
	
			sampler2D _LayerContentTex;
			float4 _LayerContentTex_TexelSize;

			// Opacity
			float _LayerOpacity;

			
			v2f vert (appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				o.uv.xy = v.texcoord.xy;

				return o;
			}
	
			half4 frag (v2f i) : COLOR {

				// sample the base color texture
				//float2 baseColorUV = i.uv;
				//float4 baseColor = tex2D (_BaseLayerColorTex, i.uv);

				float4 blendColor = tex2D(_LayerContentTex, i.uv);
				float intensity = sin(_Time.w * 10);
				return float4(1.0, 1.0, 1.0, intensity * 0.5 * blendColor.a);
				//blendColor.a *= _LayerOpacity;

				//return Blend(baseColor, blendColor, _ColorMixMode);
			}



			ENDCG
		}
	}

	Fallback off
}
