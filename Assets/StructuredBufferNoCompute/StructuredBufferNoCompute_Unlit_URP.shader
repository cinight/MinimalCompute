Shader "Unlit/StructuredBufferNoCompute Unlit URP"
{
	Properties
	{
		_EmissionDistance("_Emission Distance", Float) = 2.0
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags
		{
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Opaque"
			"Queue"="Geometry"
		}
		Pass
		{
			Name "Pass"

			HLSLPROGRAM

			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			struct appdata
			{
				float3 positionOS : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float4 uv : TEXCOORD0;
                float emissionPower : TEXCOORD1;
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _EmissionColor;
            float _EmissionDistance;
			CBUFFER_END

            struct myObjectStruct
            {
                float3 objPosition;
            };
            StructuredBuffer<myObjectStruct> myObjectBuffer;

			v2f vert(appdata v)
			{
				v2f output;

				float3 positionWS = TransformObjectToWorld(v.positionOS);
				output.positionCS = TransformWorldToHClip(positionWS);
				output.uv = v.uv;

                output.emissionPower = 0;
    
                float dist , power;
                
                [unroll]
                for(int i=0; i< 2; i++) //only 2 spheres in scene
                {
                    dist = abs(distance(myObjectBuffer[i].objPosition, positionWS.xyz));
                    power = 1 - clamp(dist / _EmissionDistance, 0.0f, 1.0f);
                    output.emissionPower += power;
                }

				return output;
			}

			float4 frag(v2f i) : SV_TARGET 
			{    
                float4 col = _Color;
                col += _EmissionColor*i.emissionPower;
                return col;
			}

			ENDHLSL
		}
	}
}