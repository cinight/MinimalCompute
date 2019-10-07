Shader "ComputeVertex"
{
	Properties
	{
		_MainTex ("_MainTex (RGBA)", 2D) = "white" {}
		_Color ("_Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// Same with the one with compute shader & C# script
			struct vertexData
			{
				float4 pos;
				float3 nor;
				float2 uv;
				float4 col;

				float4 opos;
				float3 velocity;
			};
			StructuredBuffer<vertexData> vertexBuffer;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			
			v2f vert (uint id : SV_VertexID)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(vertexBuffer[id].pos);
				o.uv = TRANSFORM_TEX(vertexBuffer[id].uv, _MainTex);
				o.color = vertexBuffer[id].col;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb *= i.color.rgb;
				return col*_Color;
			}
			ENDCG
		}
	}
}
