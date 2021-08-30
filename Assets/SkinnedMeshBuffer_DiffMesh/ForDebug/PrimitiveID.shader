Shader "Unlit/PrimitiveID"
{
    Properties
    {
        _tid ("TriangleID", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            uint _tid;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i, uint triangleID: SV_PrimitiveID) : SV_Target
            {
                fixed4 col = 1;

                if(triangleID == _tid)
                {
                    col = float4(1,0,0,1);
                }

                return col;
            }
            ENDCG
        }
    }
}
