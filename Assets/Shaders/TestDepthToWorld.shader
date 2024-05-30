Shader "Hidden/DepthToWorldPos"
{
    Properties
    {
        _MainTex ("-", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D_float _CameraDepthTexture;
    float4x4 _InverseView;
    half _Intensity;

    fixed4 frag (v2f_img i) : SV_Target
    {
        //return float4(i.uv.x, i.uv.y, 0, 1);
        const float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
        const float2 p13_31 = float2(unity_CameraProjection._13, unity_CameraProjection._23);
        const float isOrtho = unity_OrthoParams.w;
        const float near = _ProjectionParams.y;
        const float far = _ProjectionParams.z;

        float4x4 inverseViewMat = unity_CameraToWorld;

        float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
#if defined(UNITY_REVERSED_Z)
        d = 1 - d;
        inverseViewMat._13 *= -1;
		inverseViewMat._23 *= -1;
		inverseViewMat._33 *= -1;
#endif

        float zOrtho = lerp(near, far, d);
        float zPers = near * far / lerp(far, near, d);
        float vz = lerp(zPers, zOrtho, isOrtho);

        float3 vpos = float3((i.uv * 2 - 1 + p13_31) / p11_22 * vz, -vz);
        //float3 vpos = float3((i.uv * 2 - 1 - p13_31) / p11_22 * 1, -1);
        float4 wpos = mul(inverseViewMat, float4(vpos, 1));
        //float4 wpos = mul(_InverseView, float4(vpos, 1));

        half4 source = tex2D(_MainTex, i.uv);
        half3 color = pow(abs(cos(wpos.xyz * UNITY_PI * 4)), 20);
        return half4(lerp(source.rgb, color, 0.5), source.a);
    }

    //fixed4 frag (v2f_img i) : SV_Target
    //{
    //    float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);  non-linear Z
 
    //    float2 uv = i.uv;
 
    //    float4x4 proj, eyeToWorld;
 
    //    proj = unity_CameraProjection;
    //    eyeToWorld = unity_CameraToWorld;

    //    eyeToWorld._22  *= -1;
 
    //    float2 uvClip = uv * 2.0 - 1.0;
    //    float4 clipPos = float4(uvClip, d, 1.0);
    //    float4 viewPos = mul(proj, clipPos);  inverse projection by clip position
    //    viewPos /= viewPos.w;  perspective division
    //    float3 worldPos = mul(eyeToWorld, viewPos).xyz;
 
    //    fixed3 color = pow(abs(cos(worldPos * UNITY_PI * 4)), 20);  visualize grid
    //    return fixed4(color, 1);
    //}

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}