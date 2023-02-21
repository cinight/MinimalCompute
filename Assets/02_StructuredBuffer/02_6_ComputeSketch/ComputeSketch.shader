Shader "Custom/ComputeSketch" 
{
	Properties 
	{
		[HideInInspector]_MainTex ("Screen", 2D) = "white" {}

		_Color ("Color",Color) = (1,1,1,1)
		_SizeX ("SizeX",Range(0.001,0.04)) = 0.03
		_SizeY ("SizeY",Range(0.001,1)) = 1
		_BrushTex ("BrushTex", 2D) = "white" {}
	}
	SubShader 
	{
		Pass 
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
			ZWrite Off ZTest Always Cull Off Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha

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
				float4 color;
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
			float _SizeX,_SizeY;

			//https://forum.unity.com/threads/rotation-of-texture-uvs-directly-from-a-shader.150482/#post-1031763
			float2 Rotate(float2 pos, float radian)
			{
				float rot = radian;
				float sinX = sin ( rot );
				float cosX = cos ( rot );
				float sinY = sin ( rot );
				float2x2 rotationMatrix = float2x2( cosX, -sinX, sinY, cosX);
				return mul ( pos, rotationMatrix );
			}

			v2f vert (appdata v, uint inst : SV_InstanceID)
			{
				v2f o;

				float4 npos = v.vertex;

				//size according to intensity
				float size = particleBuffer[inst].intensity;
				npos.x *= _SizeX * size;
				npos.y *= _SizeY;

				//rotation
				npos.xy = Rotate(npos.xy,particleBuffer[inst].direction);

				//center position of star
				float4 pos = float4(particleBuffer[inst].position * 2.0 - 1.0, 0, 1);
				pos.x /= _ScreenParams.x / _ScreenParams.y; //respect screen ratio
				npos.xy += pos;
				npos.z += 0.1;

				o.position = npos;
				o.color = particleBuffer[inst].color;
				o.uv = TRANSFORM_TEX(v.uv, _BrushTex);

				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				return tex2D(_BrushTex, i.uv) * i.color * _Color;
			}
			
			ENDCG
		}
	}
	Fallback Off
}
