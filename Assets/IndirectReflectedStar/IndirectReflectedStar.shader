Shader "IndirectReflectedStar"
{
 	Properties 
 	{
		 _MainTex ("Texture", 2D) = "white" {}
    }

	SubShader 
	{
		Pass 
		{
			ZWrite Off ZTest Always Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			
			struct Particle
			{
				uint2 uv;
				float intensity;
			};
			StructuredBuffer<Particle> particleBuffer;  //has to be same name with compute shader

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

			v2f vert (appdata v, uint inst : SV_InstanceID)
			{
				v2f o;

				float2 resolution = _ScreenParams.xy;
				float2 spos = (float2)particleBuffer[inst].uv / resolution;
				float intensity = particleBuffer[inst].intensity;

				float4 npos = v.vertex;
				npos.xy += spos;
				o.position = UnityObjectToClipPos (npos);
				o.color = 1;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv) * i.color;
			}
			
			ENDCG
		
		}
	}
}
