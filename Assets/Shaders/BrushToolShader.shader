// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "VRPaint/BrushToolShader"
{
    //Properties
    //{
    //    _BaseColor("Base color", Color) = (.25, .5, .5, 1)
    //}

    SubShader
    {
        // Render this after the clipped layers
        Tags { "RenderType"="Transparent" }
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

        // Default brush appearance: 
        // - Clipped layer: semi-transparent sphere
        // - Base Layer: opaque sphere
        Pass 
        {
            ZTest LEqual
            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            

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

            // Current paint color
            float4 _PaintColor;

            float _BaseOpacity;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 c = _PaintColor;
                c.a *= _BaseOpacity;
                return c;
            }
            ENDCG    
        }

        // Intersection brush appearance: occluded surface stencil
        Pass 
        {
            ZTest GEqual
            ZWrite Off
            Cull Front
            ColorMask 0

            // Write to stencil buffer only if the z test succeeds (ie, fragment is behind scene)
            Stencil
            {
                //Ref 128
                //WriteMask 128
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


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
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
                // Ref 192 + [_GroupIDIntersect] (but we do this addition in C#, so _GroupIDIntersect = 128 + ACTUAL_GROUP_UID)
                Ref[_GroupIDIntersect]
                ReadMask 255
                WriteMask 64 // write to 2nd highest bit
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
                //float4 color : COLOR0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                //float4 color : COLOR0;
            };


            // Stencil comparison op
            uint _StencilComparison;

            // Layer type parameters
            uint _ZTest;

            // Current paint color
            float4 _PaintColor;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                //o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                return _PaintColor;

            }
            ENDCG
        }

        // Permissive depth test pass
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            BlendOp Add

            Stencil
            {
                // Ref 64 + [_GroupIDPermissive] (but we do this addition in C#, so _GroupIDPermissive = 64 + ACTUAL_GROUP_UID)
                Ref[_GroupIDPermissive]
                ReadMask 127
                WriteMask 64 // write to 2nd highest bit
                //WriteMask 32 // write to 3rd highest bit
                Comp[_StencilComparison]
                //Pass Keep
                Pass Invert // This prevents redrawing fragments for the same stroke => prevents intra-stroke alpha blending
                //Fail Zero
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
                //float4 color : COLOR0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                //float4 uvgrab : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float depth : TEXCOORD2;
                float4 objPos : TEXCOORD4;
            };

            // Depth Texture
            sampler2D _CameraDepthTexture;

            // Depth threshold value
            float _DepthThreshold;

            // Stencil comparison op
            uint _StencilComparison;

            // Layer type parameters
            uint _ZTest;

            // Current paint color
            float4 _PaintColor;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);

                float depth;
                COMPUTE_EYEDEPTH(depth);
                o.depth = depth;

                //o.uvgrab.xy = (float2(o.vertex.x, -o.vertex.y) + o.vertex.w) * 0.5;
                //o.uvgrab.zw = o.vertex.zw;

                //o.color = v.color;

                o.objPos = v.vertex;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                float objectZ = i.depth;
                float2 uv = i.screenPos.xy / i.screenPos.w;
                float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                sceneZ = LinearEyeDepth(sceneZ);
                bool shouldRender = abs(sceneZ - objectZ) < _DepthThreshold;

                if (shouldRender)
                {
                    return _PaintColor;
                }
                else
                {
                    discard;
                    return 0.0;
                }

            }
            ENDCG
        }

        // Pass that draws just a small white dot
        Pass{
            ZTest LEqual
            ZWrite Off
            Cull Off
            //Blend SrcAlpha OneMinusSrcAlpha

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
                float scale = 0.1;
                o.vertex = UnityObjectToClipPos(float4(v.vertex.x * scale, v.vertex.y * scale, v.vertex.z * scale, v.vertex.w));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return float4(1.0, 1.0, 1.0, 1.0);
            }
            ENDCG
        }

        // Painting brush appearance:
        // - Intersection brush: color the part of the sphere that matches the occluded stencil mask & is on GroupID stencil
        // - Over brush: TODO
        //Pass 
        //{
        //    ZTest LEqual
        //    ZWrite Off
        //    Cull Back
        //    Blend SrcAlpha OneMinusSrcAlpha


        //    Stencil
        //    {
        //        // Ref 128 + [_GroupID] (but we do this addition in C#, so _GroupID = 128 + ACTUAL_GROUP_UID)
        //        Ref [_GroupID]
        //        ReadMask 191 // 10 11 11 11 (we don't need to write to the alpha mask bit)
        //        WriteMask 192 // 11 00 00 00 (we will clear the first 2 bits)
        //        Comp Equal
        //        Pass Zero
        //        Fail Zero
        //    }

        //    CGPROGRAM
        //    #pragma vertex vert
        //    #pragma fragment frag

        //    #include "UnityCG.cginc"

        //    struct appdata
        //    {
        //        float4 vertex : POSITION;
        //    };

        //    struct v2f
        //    {
        //        float4 vertex : SV_POSITION;
        //    };
        //    
        //    // Uniforms should be updated each time we change base layer or change painting color

        //    // Base Layer Group ID
        //    uint _GroupID;

        //    // Current paint color
        //    float4 _PaintColor;


        //    v2f vert (appdata v)
        //    {
        //        v2f o;
        //        o.vertex = UnityObjectToClipPos(v.vertex);
        //        return o;
        //    }

        //    fixed4 frag (v2f i) : SV_Target
        //    {
        //        return _PaintColor;
        //    }
        //    ENDCG    
        //}



        //// Base appearance: a sphere that fades in the center
        //Pass
        //{
        //    Cull Back
        //    Blend SrcAlpha OneMinusSrcAlpha

        //    CGPROGRAM
        //    #pragma vertex vert
        //    #pragma fragment frag

        //    #include "UnityCG.cginc"

        //    struct appdata
        //    {
        //        float4 vertex : POSITION;
        //    };

        //    struct v2f
        //    {
        //        float4 vertex : SV_POSITION;
        //        float4 screenPos : TEXCOORD0;
        //        float4 centre : TEXCOORD1;
        //        float4 screenSpaceRX : TEXCOORD2;
        //    };

        //    //float3 _SphereCentrePos;
        //    float4 _BaseColor;
        //    float _MinRadius;

        //    v2f vert (appdata v)
        //    {
        //        v2f o;
        //        o.vertex = UnityObjectToClipPos(v.vertex);
        //        o.screenPos = ComputeScreenPos(o.vertex);
        //        float4 objectOrigin = unity_ObjectToWorld[3];
        //        o.centre = ComputeScreenPos(UnityObjectToClipPos(objectOrigin));
        //        //o.distToCentre = length(o.vertex - projectedOrigin);

        //        // Object scale
        //        float scale = length(unity_ObjectToWorld._m00_m10_m20);

        //        // Screen space radius
        //        float4 rX = 0.5 * scale * float4(1.0, 0.0, 0.0, 0.0) + objectOrigin;
        //        o.screenSpaceRX = ComputeScreenPos(UnityObjectToClipPos(rX));

        //        return o;
        //    }

        //    fixed4 frag (v2f i) : SV_Target
        //    {
        //        float2 centre = i.centre.xy * _ScreenParams.xy / i.centre.w;
        //        float2 screenSpaceRX = i.screenSpaceRX.xy * _ScreenParams.xy / i.screenSpaceRX.w;
        //        float2 fragPos = i.screenPos.xy * _ScreenParams.xy / i.screenPos.w;
        //        float screenSpaceRadius = length(centre - screenSpaceRX);
        //        float distToCentre = length(fragPos - centre);

        //        if (distToCentre < 0.5 * screenSpaceRadius)
        //            return float4(1.0, 0.0, 0.0, 1.0);
        //        else
        //            return float4(1.0, 1.0, 0.0, 1.0);
                    
        //    }
        //    ENDCG
        //}
    }
}
