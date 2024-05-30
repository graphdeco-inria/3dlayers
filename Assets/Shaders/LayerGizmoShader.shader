Shader "VRPaint/LayerGizmoShader"
{

    Properties
    {
        _BaseColor("Base Color", Color) = (0.0, 0.2, 0.8, 0.5)
        _HighlightColor("Highlight Color", Color) = (0.5, 0.2, 0.0, 0.9)
    }

        SubShader
    {
        Tags { "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            ZTest Off
            LOD 100
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"

            float4 _BaseColor;


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
                return _BaseColor;
            }
            ENDCG
        }
    }
}
