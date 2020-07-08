Shader "DX11/DrawIndirect" 
{
	Properties 
	{
		[HideInInspector]_MainTex ("Screen", 2D) = "white" {}

		[Header(Filter settings)]
		_SampleDistance ("SampleDistance",Range(0,0.1)) = 0.02
		_Threshold ("Threshold",Range(0,5)) = 1

		[Header(Drawing settings)]
		_Size ("Size",Range(0,1)) = 1
		_Color ("Color", Color) = (1,1,1,1)
		_Star ("Star", 2D) = "white" {}
		_Rotation ("Rotation", Range(-10,10)) = 0
	}
	SubShader 
	{
		//================= For defining star position
		Pass
		{
			ZWrite Off ZTest Always Cull Off Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"

			struct appdata 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			sampler2D _MainTex;
			AppendStructuredBuffer<float2> pointBufferOutput : register(u1); //make sure you have same id in C# script
			float _Threshold;
			float _SampleDistance;

			float4 frag (v2f i) : COLOR0
			{
				float2 uv = i.uv;

				float4 c = tex2D (_MainTex, uv);
				float4 c1 = tex2D (_MainTex, uv + saturate(float2(1,0) * _SampleDistance) );
				float4 c2 = tex2D (_MainTex, uv + saturate(float2(0,1) * _SampleDistance) );
				float4 c3 = tex2D (_MainTex, uv + saturate(float2(-1,0) * _SampleDistance) );
				float4 c4 = tex2D (_MainTex, uv + saturate(float2(0,-1) * _SampleDistance) );

				float lumc = Luminance(c) + Luminance(c1) + Luminance(c2) + Luminance(c3) + Luminance(c4);
				lumc /= 5.0f;

				[branch]
				if (lumc > _Threshold )// && lumc > lumc1)
				{
					pointBufferOutput.Append (i.uv);
				}

				return c;
			}
			ENDCG
		}

		//================= Drawing the stars
		Pass 
		{
			ZWrite Off ZTest Always Cull Off Fog { Mode Off }
			Blend SrcAlpha One

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			// struct Particle
			// {
			// 	uint2 uv;
			// 	float intensity;
			// };
			StructuredBuffer<float2> pointBuffer;

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

            sampler2D _Star;
            float4 _Star_ST;
			float4 _Color;
			float _Size;
			float _Rotation;

			float4 ApplyRotation (float4 v, float3 rotation)
			{
				// Create LookAt matrix
				float3 up = float3(0,0,1);
				
				float3 zaxis = rotation+0.001f;
				float3 xaxis = normalize(cross(up, zaxis));
				float3 yaxis = cross(zaxis, xaxis);

				float4x4 lookatMatrix = {
					xaxis.x, yaxis.x, zaxis.x, 0,
					xaxis.y, yaxis.y, zaxis.y, 0,
					xaxis.z, yaxis.z, zaxis.z, 0,
					0, 0, 0, 1
				};
				
				return mul(lookatMatrix,v);
			}

			v2f vert (appdata v, uint inst : SV_InstanceID)
			{
				v2f o;

				//center position of star
				float4 pos = float4(pointBuffer[inst] * 2.0 - 1.0, 0, 1);
				pos.y *= -1;
				
				//Rotation
				float rot = _Rotation;
				//rot *= 4.0f;
				//rot = radians(rot);

				float4 npos = v.vertex;
				npos.xy = ApplyRotation(npos,float3(rot,1,1)).xy;
				npos.x *= _Size;
				npos.y *= _Size * _ScreenParams.x / _ScreenParams.y; //respect screen ratio
				npos.xy += pos;
				npos.z += 0.1;

				o.position = npos;
				o.color = _Color;
				o.uv = TRANSFORM_TEX(v.uv, _Star);

				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				return tex2D(_Star, i.uv) * i.color;
			}
			
			ENDCG
		}
	}
	Fallback Off
}
