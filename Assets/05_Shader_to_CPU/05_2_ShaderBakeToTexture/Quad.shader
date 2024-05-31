Shader "05_2_ShaderBakeToTexture/Quad"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5 //UAV needs 4.5+

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

            RWTexture2D<float4> Result : register(u6); //match with C# script "targetID"
            uint _Size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 id = float2(i.uv * _Size);
                float4 col = Result[id.xy];
                return col;
            }
            ENDCG
        }
    }
}
