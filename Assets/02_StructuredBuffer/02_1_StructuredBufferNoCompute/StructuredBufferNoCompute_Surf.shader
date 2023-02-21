Shader "StructuredBufferNoCompute Surf"
{
	Properties 
	{
		_EmissionDistance("_Emission Distance", Float) = 2.0
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		_Color ("Color", Color) = (1,1,1,1)
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 4.5

		sampler2D _MainTex;

		struct Input 
		{
			//float2 uv_MainTex;
			float emissionPower;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		#ifdef UNITY_COMPILER_HLSL
		struct myObjectStruct
		{
			float3 objPosition;
		};
		StructuredBuffer<myObjectStruct> myObjectBuffer;
		#endif

		CBUFFER_START(MyRarelyUpdatedVariables)
			float _EmissionDistance;
			float4 _EmissionColor;
		CBUFFER_END

		//===========vert
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			UNITY_SETUP_INSTANCE_ID(v);

			float4 wvertex = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1));
			o.emissionPower = 0;

			#ifdef UNITY_COMPILER_HLSL
				float dist = abs(distance(myObjectBuffer[0].objPosition, wvertex.xyz));
				float power = 1 - clamp(dist / _EmissionDistance, 0.0f, 1.0f);
				o.emissionPower += power;

				dist = abs(distance(myObjectBuffer[1].objPosition, wvertex.xyz));
				power = 1 - clamp(dist / _EmissionDistance, 0.0f, 1.0f);
				o.emissionPower += power;
			#endif
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 c = _Color;

			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			o.Emission = _EmissionColor*IN.emissionPower;
		}
		ENDCG
	}
}
