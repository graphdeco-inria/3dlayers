Shader "VRPaint/BlendingTexture"
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
	
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			// Depth Texture
            //sampler2D _CameraDepthTexture; 
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

			// Base Color texture
			sampler2D _BaseLayerColorTex;
			float4 _BaseLayerColorTex_TexelSize;

			// Opacity
			float _LayerOpacity;

			// Color mix modes
            int _ColorMixMode;

			// Canvas transform
			float4x4 _CanvasObjectToWorldMatrix;
			float4x4 _CanvasWorldToObjectMatrix;
			float _CanvasScale;

			// Texture type

			// Texture scale
			float3 _TextureSamplingScale;
			float _MinOpacity;
			float _MaxOpacity;

			
			v2f vert (appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				o.uv.xy = v.texcoord.xy;

				return o;
			}


			#include "DepthToWorldPos.cginc"
			#include "Assets/Shaders/jp.keijiro.noiseshader/PerlinNoise3D.hlsl"

			//#define NOISE_FUNC(coord, period) PeriodicNoise(coord, period)
			#define NOISE_FUNC(coord, period) ClassicNoise(coord)

			float map(float value, float min1, float max1, float min2, float max2) {
				float perc = (value - min1) / (max1 - min1);
				float smoothValue = perc * (max2 - min2) + min2;
				return smoothValue;
			}

			// Value is in [-1, 1]
			float threshold(float value, float threshold, float lowValue, float highValue) {
				if (value < threshold)
					return lowValue;
				else
					return highValue;
			}

	
			half4 frag (v2f i) : COLOR {

				// sample the base color texture
				float2 baseColorUV = i.uv;
				float4 baseColor = tex2D (_BaseLayerColorTex, i.uv);

				float4 blendColor = tex2D(_MainTex, i.uv);

				float4 worldPos = SampleWorldPos(i.uv);
				float4 canvasSpacePos = mul(_CanvasWorldToObjectMatrix, worldPos);

				//half3 color = pow(abs(cos(worldPos.xyz * UNITY_PI * 30)), 20);
				//return float4(color, 1.0);

				// sample 3D texture to determine blend color alpha
				float s = 100;
				//float3 period = float3(s, s, s) * 2.0;
				float p = 1.0;
				float3 period = float3(p, p, p) * 2.0;
				float noiseSample = NOISE_FUNC(canvasSpacePos * _TextureSamplingScale, period);
				//float alphaSample = map(noiseSample, -1.0, 1.0, _MinOpacity, _MaxOpacity);
				float alphaSample = threshold(noiseSample, -0.2, _MinOpacity, _MaxOpacity);
				//return float4(alphaSample, 0.0, 0.0, 1.0);

				//float gradientValue = sceneZ;

				blendColor.a *= _LayerOpacity;
				blendColor.a *= alphaSample;

				return Blend(baseColor, blendColor, _ColorMixMode);
			}



			ENDCG
		}
	}

	Fallback off
}
