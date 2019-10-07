Shader "UAVTest/VertFrag"
{
	Properties
	{
		_MainTex("_MainTex (RGBA)", 2D) = "white" {}
		_Speed("_Speed",Range(0,0.5)) = 0.5
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

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

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _Speed;

			#ifdef UNITY_COMPILER_HLSL
				RWStructuredBuffer<float> Field : register(u6); //match with C# script "targetID"
			#endif

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//Rainbow color
				float3 c;
				c.r = frac(sin(_Time.x*_Speed));
				c.g = frac(sin(_Time.z*_Speed));
				c.b = frac(sin(_Time.w*_Speed));

				#ifdef UNITY_COMPILER_HLSL
					Field[0] = c.r;
					Field[1] = c.g;
					Field[2] = c.b;
				#endif

				float4 col = tex2D(_MainTex, i.uv);
				col.rgb *= c;
				return col;
			}
			ENDCG
		}
	}
}
