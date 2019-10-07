Shader "ComputeParticlesDirect"
{
 	Properties 
 	{
		_Color1("_Color1", color) = (1,1,1,1)
		_Color2("_Color2", color) = (1,1,1,1)
		_pSize("Particle Size",Range(1,5)) = 2 //seems only work for Metal :(
    }

	SubShader 
	{
		Pass 
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct Particle
			{
				float3 position;
			};
			StructuredBuffer<Particle> particleBuffer;  //has to be same name with compute shader

			struct v2f
			{
				float4 color : COLOR;
				float4 position : SV_POSITION;
				float pSize : PSIZE;
			};

			float4 _Color1;
			float4 _Color2;
			float _pSize;
			
			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float4 pos = float4(particleBuffer[inst].position, 1);

				float f = inst / 320000.0f;
				
				o.position = UnityObjectToClipPos (pos);
				o.color.rgb = lerp( _Color1 , _Color2 , f );
				o.color.a = 1;
				o.pSize = _pSize;

				return o;
			}
			
			float4 frag (v2f IN) : SV_Target
			{
				return IN.color;
			}
			
			ENDCG
		
		}
	}
}
