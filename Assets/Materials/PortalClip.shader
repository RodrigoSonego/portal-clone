Shader "CustomRenderTexture/PortalClip"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex("InputTex", 2D) = "white" {}
        _HideView("HideView", Int) = 1
        
        _HRadius("HorizontalRadius", Range(0, 1.0)) = 1.0
        _VRadius("VerticalRadius", Range(0, 1.0)) = 1.0
        
        _EdgeColor ("Edge Color", Color) = (0.1, 0.7, 1.0, 1) // Portal-blue
        _EdgeWidth ("Edge Width", Float) = 0.1
        _Sharpness ("Edge Sharpness", Float) = 4.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Geometry"
        }
        //Cull Off

        Pass
        {
            Name "PortalClip"

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            float4 _Color;
            sampler2D _MainTex;
            int _HideView;
            float4 blackColor = (0, 0, 0, 1);

            float _HRadius;
            float _VRadius;

            float _EdgeWidth;
            float4 _EdgeColor;
            float _Sharpness;

            struct appdata
            {
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
            };

            struct v2f
            {
                float4 screenPos: TEXCOORD0;
                float4 vertex: SV_POSITION;
                float2 localUV: TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.localUV = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;

                // Transform UV from [0, 1] to [-1, 1]
                float2 centeredUV = i.localUV* 2.0 - 1.0;

                // Ellipse equation: x²/a² + y²/b² <= 1
                float ellipse = (centeredUV.x * centeredUV.x) / (_HRadius * _HRadius) +
                    (centeredUV.y * centeredUV.y) / (_VRadius * _VRadius);

                // If within ellipse, use default color/ camera view
                if (ellipse < 1.0)
                {
                    float4 col = _HideView ? blackColor : _Color;
                    return tex2D(_MainTex, uv) * col;
                }

                // Use edge Color within width
                if (ellipse < 1.0 + _EdgeWidth)
                {
                    return _EdgeColor;
                }
                
                // Discard if outside the ellipse and edge
                discard;

                return 0;
            }
            ENDCG
        }
    }
}