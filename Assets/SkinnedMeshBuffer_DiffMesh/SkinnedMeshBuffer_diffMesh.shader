Shader "Unlit/Graphics_DrawMeshInstancedIndirect"
{
    Properties
    {
		[Enum(Off,0,Front,1,Back,2)] _CullMode ("Culling Mode", int) = 0
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("_Color", Color) = (1,1,1,1)
		_Displacement ("_Displacement", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		Cull [_CullMode]

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

            struct appdata
            {
                float4 vertex : POSITION;
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
			float _Displacement;

            uniform float4 _LightColor0;

			//ref: https://github.com/Unity-Technologies/MeshApiExamples/blob/master/Assets/ProceduralWaterMesh/WaterComputeShader.compute
			float3 GetVertexData_Position(ByteAddressBuffer vBuffer, ByteAddressBuffer iBuffer, uint vid, uint iid)
			{
				//layout for index buffer
				//uint32 = 4 bytes for each id
                //3 ids for each triangle
				uint tid = (iid * 3 + vid)*4 ;
				uint id = asuint(iBuffer.Load( tid ));

				//layout for vertex buffer (observed by using RenderDoc):
				//float3 position
				//float3 normal
				//float4 tangent
				//therefore total 10 floats and 4 bytes each = 10*4 = 40
				int vidx = id * 40;
				float3 data = asfloat(vBuffer.Load3(vidx));
				return data;
			}
			float3 GetVertexData_Normal(ByteAddressBuffer vBuffer, ByteAddressBuffer iBuffer, uint vid, uint iid)
			{
				uint tid = (iid * 3 + vid)*4 ;
				uint id = asuint(iBuffer.Load( tid ));

				int vidx = id * 40;
				float3 data = asfloat(vBuffer.Load3(vidx+12)); //offset by float3 (position) in front, so 3*4bytes = 12
				return data;
			}
			//returns a-mid-b, when k is 0-0.5-1
			#define pi 3.14159265358979
			float3 MidLerp(float3 a, float3 b, float3 mid, float k)
			{
				float m = sin( k * pi ); //when k is 0-0.5-1, returns 0-1-0
				float l = saturate( ( (1.0-k) * 2.0)-1.0 ); //when k is 0-0.5-1, returns 1-0-0
				float r = saturate( (k * 2.0)-1.0 ); //when k is 0-0.5-1, returns 0-0-1

				float3 L = lerp( 0 , a , l );
				float3 R = lerp( 0 , b , r );
				float3 M = lerp( 0 , mid , m );

				return L+R+M;
			}

            v2f vert (appdata v, uint vid : SV_VertexID, uint instanceID : SV_InstanceID)
            {
				v2f o;

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				//Blend normal
				float3 norA = GetVertexData_Normal(bufVerticesA,bufVerticesA_index,vid,instanceID);
				float3 norB = GetVertexData_Normal(bufVerticesB,bufVerticesB_index,vid,instanceID);
				float3 nor = lerp( norA, norB , _Progress );
				o.normal = nor;

				//Blend position
				float3 posA = GetVertexData_Position(bufVerticesA,bufVerticesA_index,vid,instanceID) + _HipLocalPositionA;
				float3 posB = GetVertexData_Position(bufVerticesB,bufVerticesB_index,vid,instanceID) + _HipLocalPositionB;
				float3 pos = lerp( posA, posB, _Progress );
				pos = MidLerp(posA, posB, pos+nor*_Displacement, _Progress); //this makes the triangle goes outward at the middle of progress
				o.vertex = UnityObjectToClipPos(pos);

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

				//Debug Color
				//col *= i.color;

				return col;
			}
			ENDCG
        }
    }
}
