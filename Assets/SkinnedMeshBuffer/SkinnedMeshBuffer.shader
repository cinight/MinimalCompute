// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "SkinnedMeshBuffer"
{
	Properties
	{
		_Progress ("_Progress", Range(0,1)) = 0.5
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
			RWByteAddressBuffer bufVerticesA;
			RWByteAddressBuffer bufVerticesB;

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

			//from https://github.com/Unity-Technologies/MeshApiExamples/blob/master/Assets/ProceduralWaterMesh/WaterComputeShader.compute
			float3 GetVertexData_Position(RWByteAddressBuffer addBuffer, uint vid)
			{
				// We know that our vertex layout is 6 floats per vertex
				// (float3 position + float3 normal).
				int vidx = vid * 6;
				float3 position = asfloat(addBuffer.Load3(vidx<<2));
				return position;
			}
			float3 GetVertexData_Normal(RWByteAddressBuffer addBuffer, uint vid)
			{
				// We know that our vertex layout is 6 floats per vertex
				// (float3 position + float3 normal).
				int vidx = vid * 6;
				float3 normal = asfloat(addBuffer.Load3(vidx+3)<<2);
				return normal;
			}
			
			v2f vert (uint id : SV_VertexID)
			{
				v2f o;
				
				uint realid = id;

				//Blend A and B
				float3 pos = lerp( GetVertexData_Position(bufVerticesA,realid), GetVertexData_Position(bufVerticesB,realid) , _Progress );
				float3 nor = lerp( GetVertexData_Normal(bufVerticesA,realid), GetVertexData_Normal(bufVerticesB,realid) , _Progress );
				o.vertex = UnityObjectToClipPos(pos);
				o.normal = nor;

				//Rim
				float wpos = mul(unity_ObjectToWorld, o.vertex);
				float3 viewDir = normalize( _WorldSpaceCameraPos.xyz - wpos );
				float rim = pow( 1.0 - saturate( dot( viewDir, o.normal ) ), 3.0 );
				o.color = lerp( 0, _Color, rim );

				//o.uv = TRANSFORM_TEX(vertexBuffer[realid].uv, _MainTex);
				//o.color = vertexBuffer[realid].col;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{


				//fixed4 col = tex2D(_MainTex, i.uv);
				//col.rgb *= i.color.rgb;
				return i.color;//col*_Color;
			}
			ENDCG
		}
	}
}
