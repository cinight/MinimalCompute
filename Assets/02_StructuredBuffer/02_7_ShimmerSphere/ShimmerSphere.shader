Shader "Unlit/ShimmerSphere URP"
{
	Properties
	{
		[HDR] _Color("Color", Color) = (0,0,0)
		_MainTex ("ShimmerPattern", 2D) = "white" {}
		_DepthDistance("_DepthDistance", Range(-0.05,0)) = 1.0
		_CoverPower("_CoverPower", Range(0,5)) = 1.0
		
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 8
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source", Int) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination", Int) = 1.0
	}
	SubShader
	{
		Tags
		{
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}
		ZWrite Off
		ZTest [_ZTest]
		Blend [_SrcBlend] [_DstBlend]
		
		Pass
		{
			Name "Pass"

			HLSLPROGRAM

			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

			struct appdata
			{
				float3 positionOS : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float4 uv : TEXCOORD0;
				float3 positionWS : TEXCOORD1;
				float4 positionScreen : TEXCOORD2;
			};

			sampler2D _MainTex;

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Color;
            float _DepthDistance;
			float3 objectCenter;
			float _CoverPower;
			CBUFFER_END

            struct myObjectStruct
            {
                float3 objPosition;
				float objSize;
            };
            StructuredBuffer<myObjectStruct> myObjectBuffer;

			v2f vert(appdata v)
			{
				v2f output;

				// billboard
				float3 vpos = mul((float3x3)unity_ObjectToWorld, v.positionOS.xyz);
				float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				output.positionCS = mul(UNITY_MATRIX_P, viewPos);

				// get positions
				output.positionWS = mul(UNITY_MATRIX_I_V, viewPos).xyz;
				output.positionScreen = ComputeScreenPos(output.positionCS);
				
				// uv
				output.uv = v.uv;

				return output;
			}

			//chatgpt
			bool IsPointAfterRayPassThroughSphere(float3 p, float3 p0, float3 spherePos, float sphereRadius)
			{
				float3 dir = normalize(p - p0);
			    float3 srcToSphere = p0 - spherePos;

			    // parameters for quadratic equation
			    float a = dot(dir, dir);
			    float b = 2 * dot(srcToSphere, dir);
			    float c = dot(srcToSphere, srcToSphere) - sphereRadius * sphereRadius;

			    // compute the discriminant
			    float disc = b * b - 4 * a * c;

			    // If discriminant is less than 0, no intersection with sphere
			    if (disc < 0)
			    {
			        return false;
			    }
			    else
			    {
			        // Find the nearest/first intersection point
			        float t = (-b - sqrt(disc)) / (2 * a);

			        // Check if the intersection point is behind the ray origin
			        if (t < 0)
			        {
			            return false;
			        }
			       
			        // if p is located after the sphere on the ray
			        float3 intersection = p0 + t * dir;
			        float3 rayToP = p - p0; // vector from p0 to p

			        // Check if the intersection point is closer to p0 than the point p
			        return length(rayToP) > length(intersection - p0);
			    }
			}

			#define MAXSPHERECOUNT 64

			float4 frag(v2f input) : SV_TARGET 
			{    
                float4 col = 1.0;
				float2 uv = input.uv.xy;

				// Basic sphere fade
				float distToCenter = 1 - distance(uv,0.5);
				distToCenter = smoothstep(0.5,1,distToCenter);
				col.a = distToCenter;

				// Shimmer pattern
				uv -= 0.5;
				uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;
			    float angle = (atan2(uv.y, uv.x) + PI) / (2.0 * PI);
			    float4 tex  = tex2D(_MainTex, float2(_Time.x * 1.0, angle));
				col.rgb *= tex.rgb;
				col.rgb = lerp(tex.rgb,(_Color.rgb*0.7),pow(col.a*0.7,2));

				// Depth difference //ref: ShaderGraph SceneDepthDifferenceNode.cs
				float2 uvScreen = input.positionScreen.xy / input.positionScreen.w;
				float sceneDpeth = SampleSceneDepth(uvScreen);
				float deviceDepth = ComputeNormalizedDeviceCoordinatesWithZ(input.positionWS, GetWorldToHClipMatrix()).z;
				float depthDiff = 0;
				#if defined(UNITY_REVERSED_Z)
					depthDiff = deviceDepth - sceneDpeth;
				#else
				    depthDiff = sceneDpeth - deviceDepth;
				#endif
				depthDiff = 1-saturate(smoothstep(0,_DepthDistance,depthDiff));
				col.a *= depthDiff;

                // Sphere distances
                float power = col.a;
				float3 p0 = objectCenter;
				float3 p = input.positionWS.xyz;
				float planeDistToCamera = distance(p0, _WorldSpaceCameraPos);
                for(int i=0; i< MAXSPHERECOUNT; i++)
                {
                	// Find projected sphere position on the plane
                	float3 spherePos = myObjectBuffer[i].objPosition - _WorldSpaceCameraPos;
                	spherePos = normalize(spherePos) * planeDistToCamera + _WorldSpaceCameraPos;

                	// Find projected sphere radius on the plane
                	float3 spherePosBorder = myObjectBuffer[i].objPosition;
                	spherePosBorder.x += myObjectBuffer[i].objSize * 0.5f;
                	spherePosBorder -= _WorldSpaceCameraPos;
                	spherePosBorder = normalize(spherePosBorder) * planeDistToCamera + _WorldSpaceCameraPos;
                	float sphereRadius = distance(spherePos, spherePosBorder);
                	
                	if(IsPointAfterRayPassThroughSphere(p, p0, spherePos, sphereRadius))
					{
                		float sphereDistToCamera = distance(spherePos, p0) / 8.0;
                		power *= 1-(saturate(sphereDistToCamera * _CoverPower)*(1-distToCenter));
                		break;
					}
                }
				col.a = power;
				
				// Result
				col.rgb *= _Color.rgb;
				col.a = saturate(col.a)*_Color.a;

                return col;
			}

			ENDHLSL
		}
	}
}