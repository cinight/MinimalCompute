Shader "Custom/ComputeSketch" 
{
	Properties 
	{
		[HideInInspector]_MainTex ("Screen", 2D) = "white" {}

		_Color ("Color",Color) = (1,1,1,1)
		_Size ("Size",Range(0,1)) = 0.1
		_MaxSize ("MaxSize",Range(0,1)) = 0.1
		_BrushTex ("BrushTex", 2D) = "white" {}
	}
	SubShader 
	{
		Pass 
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
			ZWrite Off ZTest Always Cull Off Fog { Mode Off }
			Blend SrcAlpha One

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct Particle
			{
				float2 position;
				float direction;
				float intensity;
			};
			StructuredBuffer<Particle> particleBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

			struct v2f
			{
				float4 position : SV_POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD;
			};

            sampler2D _BrushTex;
            float4 _BrushTex_ST;
			float4 _Color;
			float _Size;
			float _MaxSize;

			v2f vert (appdata v, uint inst : SV_InstanceID)
			{
				v2f o;

				float4 npos = v.vertex;

				//size according to intensity
				float size = _Size;
				size *= particleBuffer[inst].intensity;
				size = clamp(size,0,_MaxSize);
				npos.x *= size;
				npos.y *= size;// * _ScreenParams.x / _ScreenParams.y; //respect screen ratio

				//center position of star
				float4 pos = float4(particleBuffer[inst].position * 2.0 - 1.0, 0, 1);
				//pos.y *= -1;
				npos.xy += pos;
				npos.z += 0.1;

				//rotation
				//npos.x += cos(particleBuffer[inst].direction);
				//npos.y += sin(particleBuffer[inst].direction);

				o.position = npos;
				o.color = _Color;
				o.uv = TRANSFORM_TEX(v.uv, _BrushTex);

				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				return tex2D(_BrushTex, i.uv) * i.color;
			}
			
			ENDCG
		}
	}
	Fallback Off
}
