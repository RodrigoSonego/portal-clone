Shader "CustomRenderTexture/PortalClip"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex("InputTex", 2D) = "white" {}
     }

     SubShader
     {
        Blend One Zero
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "IgnoreProjector"="True" }

        Pass
        {
            Name "PortalClip"

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            float4      _Color;
            sampler2D   _MainTex;

            struct appdata 
            {
                float4 vertex: POSITION;
            };

            struct v2f
            {
                float4 screenPos: TEXCOORD0;
                float4 vertex: SV_POSITION;
            };

            v2f vert (appdata v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                return tex2D(_MainTex, uv) * _Color;
            }
            ENDCG
        }
    }
}
