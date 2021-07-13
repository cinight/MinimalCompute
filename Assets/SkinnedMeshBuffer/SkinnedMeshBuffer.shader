Shader "SkinnedMeshBuffer"
{
	Properties
	{
		_MainTex ("_MainTex (RGBA)", 2D) = "white" {}
		_Color ("_Color", Color) = (1,1,1,1)
		_ByteAddressOffset_Pos ("_ByteAddressOffset_Pos", Vector) = (40,0,0,0)
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
			//Index buffers
			ByteAddressBuffer bufVerticesA_index;
			ByteAddressBuffer bufVerticesB_index;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				//float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float3 _HipLocalPositionA;
			float3 _HipLocalPositionB;
			uint4 _ByteAddressOffset_Pos;

			//ref: https://github.com/Unity-Technologies/MeshApiExamples/blob/master/Assets/ProceduralWaterMesh/WaterComputeShader.compute
			float3 GetVertexData_Position(ByteAddressBuffer vBuffer, ByteAddressBuffer iBuffer, uint vid)
			{
				int vidx = vid * _ByteAddressOffset_Pos.x;
				float3 position = asfloat(vBuffer.Load3(vidx)<<_ByteAddressOffset_Pos.y);
				return position;
			}
			float3 GetVertexData_Normal(ByteAddressBuffer vBuffer, ByteAddressBuffer iBuffer, uint vid)
			{
				uint id = asuint(iBuffer.Load(vid)<<0);
				int vidx = id * 6;
				float3 normal = asfloat(vBuffer.Load3(vidx+3)<<2);
				return normal;
			}
			
			v2f vert (uint id : SV_VertexID)
			{
				v2f o;

				//Blend position
				float3 posA = GetVertexData_Position(bufVerticesA,bufVerticesA_index,id) + _HipLocalPositionA;
				float3 posB = GetVertexData_Position(bufVerticesB,bufVerticesB_index,id) + _HipLocalPositionB;
				float3 pos = lerp( posA, posB, _Progress );
				o.vertex = UnityObjectToClipPos(pos);

				//float3 nor = lerp( GetVertexData_Normal(bufVerticesA,realid), GetVertexData_Normal(bufVerticesB,realid) , _Progress );
				//float3 wpos = mul(unity_ObjectToWorld, pos);
				float3 nor = GetVertexData_Normal(bufVerticesA,bufVerticesA_index,id);
				o.normal = nor;

				o.color = o.vertex;

				//Rim
				//float3 wpos = mul(unity_ObjectToWorld, o.vertex);
				//float3 viewDir = normalize( _WorldSpaceCameraPos.xyz - wpos );
				//float rim = pow( 1.0 - saturate( dot( viewDir, o.normal ) ), 3.0 );
				//o.color = lerp( 0, _Color, rim );

				//o.uv = TRANSFORM_TEX(vertexBuffer[realid].uv, _MainTex);
				//o.color = vertexBuffer[realid].col;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{


				//fixed4 col = tex2D(_MainTex, i.uv);
				//col.rgb *= i.color.rgb;
				return frac(i.color);//col*_Color;
			}
			ENDCG
		}
	}
}
