Shader "ComputeParticlesIndirect"
{
 	Properties 
 	{
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
				uint idx;
				float4 color;
			};
			StructuredBuffer<Particle> particleBuffer;  //has to be same name with compute shader
			StructuredBuffer<uint> particleResult;  //has to be same name with compute shader

			struct v2f
			{
				float4 color : COLOR;
				float4 position : SV_POSITION;
				float pSize : PSIZE;
			};

			float _pSize;
			
			v2f vert (uint inst : SV_InstanceID)
			{
				v2f o;

				uint id = particleResult[inst];

				float4 pos = float4(particleBuffer[id].position, 1);
				o.position = UnityObjectToClipPos (pos);
				o.color.rgb = particleBuffer[id].color.rgb;
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
