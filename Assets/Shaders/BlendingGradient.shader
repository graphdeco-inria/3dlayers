Shader "VRPaint/BlendingGradient"
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
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			#include "BlendModes.cginc"


			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				//float4 screenPos : TEXCOORD1;
				//float3 camRelativeWorldPos : TEXCOORD2;
				//float3 ray : TEXCOORD3;
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

			// Gradient info
			float _GradientType; // 0 = linear, 1 = sphere
			float3 _GradientA; // black (0)
			float _ValueA;
			float3 _GradientB; // white (1)
			float _ValueB;

			// Canvas transform
			float4x4 _CanvasObjectToWorldMatrix;
			float4x4 _CanvasWorldToObjectMatrix;
			float _CanvasScale;

			float LinearGradient (float3 A, float3 B, float3 position) 
			{
				// Gradient direction
				float3 AB = B - A;
				float dist = length(AB);
				float3 dir = AB / dist;

				// Project position along gradient direction
				float proj = dot(dir, position - A) / dist;
				float value = proj;

				if (_ValueB - _ValueA < 0)
				{
					value = 1.0 - value;
					return clamp(value, _ValueB, _ValueA);
				}
				
				return clamp(value, _ValueA, _ValueB);
				//return _ValueA;
			}

			float SphereGradient (float3 A, float3 B, float3 position)
			{
				//return B.x;
				// Radius
				float radius = length(A - B);
				//return radius;
				float distToCenter = length(A - position);
				//return distToCenter;
				float relDist = distToCenter / radius;
				//return relDist;

				float value = relDist;

				if (_ValueB - _ValueA < 0)
				{
					value = 1.0 - value;
					return clamp(value, _ValueB, _ValueA);
				}
				else
					return clamp(value, _ValueA, _ValueB);
				
			}
			
			v2f vert (appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
                //o.screenPos = ComputeScreenPos(o.pos);

				o.uv.xy = v.texcoord.xy;

				return o;
			}


			#include "DepthToWorldPos.cginc"

	
			half4 frag (v2f i) : COLOR {

				// sample the base color texture
				float2 baseColorUV = i.uv;
				float4 baseColor = tex2D (_BaseLayerColorTex, i.uv);

				float4 blendColor = tex2D(_MainTex, i.uv);

				float4 worldPos = SampleWorldPos(i.uv);

				//half3 color = pow(abs(cos(worldPos.xyz * UNITY_PI * 4)), 20);
				//return color;

				// compute gradient color based on position
				float3 A = mul(_CanvasObjectToWorldMatrix, float4(_GradientA, 1.0)).xyz;
				float3 B = mul(_CanvasObjectToWorldMatrix, float4(_GradientB, 1.0)).xyz;
				float gradientValue = (1.0 - _GradientType) * LinearGradient(A, B, worldPos.xyz) + _GradientType * SphereGradient(A, B, worldPos.xyz);
				//return float4(gradientValue, 0.0, 0.0, 1.0);

				//float gradientValue = sceneZ;

				blendColor.a *= _LayerOpacity;
				blendColor.a *= gradientValue;

				//return Blend(baseColor, blendColor, _ColorMixMode);

				float4 preAlphaColor = Blend(baseColor, blendColor, _ColorMixMode);
				float3 preMultRGB = preAlphaColor.a * preAlphaColor.rgb;

				return float4(preMultRGB, preAlphaColor.a);
			}



			ENDCG
		}
	}

	Fallback off
}
