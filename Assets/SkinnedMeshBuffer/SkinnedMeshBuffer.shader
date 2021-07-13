Shader "SkinnedMeshBuffer"
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
			#pragma target 5.0

			#include "UnityCG.cginc"

			//Progress
			float _Progress;

			//Vertex buffers
			ByteAddressBuffer bufVerticesA;
			ByteAddressBuffer bufVerticesB;

            struct appdata
            {
                float2 uv : TEXCOORD0;
            };

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float3 _HipLocalPositionA;
			float3 _HipLocalPositionB;

			uniform float4 _LightColor0;

			//ref: https://github.com/Unity-Technologies/MeshApiExamples/blob/master/Assets/ProceduralWaterMesh/WaterComputeShader.compute
			float3 GetVertexData_Position(ByteAddressBuffer vBuffer, uint vid)
			{
				//layout for vertex buffer (observed by using RenderDoc):
				//float3 position
				//float3 normal
				//float4 tamgent
				//therefore total 10 floats and 4 bytes each = 10*4 = 40
				int vidx = vid * 40;
				float3 data = asfloat(vBuffer.Load3(vidx));
				return data;
			}
			float3 GetVertexData_Normal(ByteAddressBuffer vBuffer, uint vid)
			{
				int vidx = vid * 40;
				float3 data = asfloat(vBuffer.Load3(vidx+12)); //offset by float3 position in front, so 3*4bytes = 12
				return data;
			}
			
			v2f vert (appdata v, uint id : SV_VertexID)
			{
				v2f o;

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				//Blend position
				float3 posA = GetVertexData_Position(bufVerticesA,id) + _HipLocalPositionA;
				float3 posB = GetVertexData_Position(bufVerticesB,id) + _HipLocalPositionB;
				float3 pos = lerp( posA, posB, _Progress );
				o.vertex = UnityObjectToClipPos(pos);

				//Blend normal
				float3 norA = GetVertexData_Normal(bufVerticesA,id);
				float3 norB = GetVertexData_Normal(bufVerticesB,id);
				float3 nor = lerp( norA, norB , _Progress );
				o.normal = nor;

				//world position for rim
				o.color = mul(unity_ObjectToWorld, pos);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//Texture
				fixed4 col = tex2D(_MainTex, i.uv);

				//Basic lighting
				float3 wnor = normalize( mul( unity_WorldToObject, float4( i.normal, 0.0 ) ).xyz );
				float3 lightdir = normalize(_WorldSpaceLightPos0.xyz);
				float atten = 1.0;
				float3 diffuse = atten * _LightColor0.xyz * max(0.0,dot(wnor, lightdir));
				col *= float4(diffuse,1);

				//Rim
				float3 viewDir = normalize( _WorldSpaceCameraPos.xyz - i.color.rgb );
				float rim = pow( 1.0 - saturate( dot( viewDir, wnor ) ), 3.0 );
				float4 rimColor = lerp( 0, _Color, rim );
				col += rimColor;

				return col;
			}
			ENDCG
		}
	}
}
