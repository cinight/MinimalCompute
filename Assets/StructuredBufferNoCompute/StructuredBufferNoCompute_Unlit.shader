Shader "Unlit/StructuredBufferNoCompute Unlit"
{
    Properties
    {
		_EmissionDistance("_Emission Distance", Float) = 2.0
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		_Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float emissionPower : TEXCOORD1;
            };

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                float4 wvertex = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1));
                o.emissionPower = 0;
    
                #ifdef UNITY_COMPILER_HLSL
                    float dist , power;
                    
                    [unroll]
                    for(int i=0; i< 2; i++) //only 2 spheres in scene
                    {
                        dist = abs(distance(myObjectBuffer[i].objPosition, wvertex.xyz));
                        power = 1 - clamp(dist / _EmissionDistance, 0.0f, 1.0f);
                        o.emissionPower += power;
                    }
                #endif
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                col += _EmissionColor*i.emissionPower;
                return col;
            }
            ENDCG
        }
    }
}
