Shader "StandardEdited1"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup

        // Same with the one with compute shader & C# script
        struct MyVertexData
        {
            uint id;
            float4 pos;
            float3 nor;
            float4 tan;
            float4 uv;
        };
        StructuredBuffer<MyVertexData> vertexBuffer;
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300


        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _DETAIL_MULX2
            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertBaseEdited
            #pragma fragment fragBaseEdited
            #include "UnityStandardCoreForward.cginc"

            VertexOutputForwardBase vertBaseEdited (uint id : SV_VertexID)
            { 
                uint realid = vertexBuffer[id].id;

                VertexInput o;
                o.vertex = vertexBuffer[realid].pos;
                o.normal = vertexBuffer[realid].nor;
                o.uv0 = vertexBuffer[realid].uv;
                o.uv1 = vertexBuffer[realid].uv;
                #if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
                o.uv2 = vertexBuffer[realid].uv;
                #endif
                #ifdef _TANGENT_TO_WORLD
                o.tangent = vertexBuffer[realid].tan;
                #endif

                //This is for coloring the trail
                VertexOutputForwardBase vOut = vertForwardBase(o);
                vOut.tex.z = vertexBuffer[realid].uv.z;

                return vOut;
            }

             half4 fragBaseEdited (VertexOutputForwardBase i) : SV_Target 
             {
                 //This is for coloring the trail
                 float4 col = lerp(0,float4(1,0,0,1),i.tex.z);
                 return fragForwardBaseInternal(i) + col; 
             }

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAddEdited
            #pragma fragment fragAdd
            #include "UnityStandardCoreForward.cginc"

            VertexOutputForwardAdd vertAddEdited (uint id : SV_VertexID) 
            { 
                uint realid = vertexBuffer[id].id;

                VertexInput o;
                o.vertex = vertexBuffer[realid].pos;
                o.normal = vertexBuffer[realid].nor;
                o.uv0 = vertexBuffer[realid].uv;
                o.uv1 = vertexBuffer[realid].uv;
                #if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
                o.uv2 = vertexBuffer[realid].uv;
                #endif
                #ifdef _TANGENT_TO_WORLD
                o.tangent = vertexBuffer[realid].tan;
                #endif

                return vertForwardAdd(o);
            }

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCasterEdited
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"


            void vertShadowCasterEdited (uint id : SV_VertexID
                , out float4 opos : SV_POSITION
                #ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
                , out VertexOutputShadowCaster o
                #endif
                #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
                , out VertexOutputStereoShadowCaster os
                #endif
            )
            {
                //My own vertex input========
                uint realid = vertexBuffer[id].id;

                VertexInput v;
                v.vertex = vertexBuffer[realid].pos;
                v.normal = vertexBuffer[realid].nor;
                v.uv0 = vertexBuffer[realid].uv;
                //v.uv1 = vertexBuffer[realid].uv;
               // #if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
                //v.uv2 = vertexBuffer[realid].uv;
                //#endif
                #if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
                v.tangent = vertexBuffer[realid].tan;
                #endif
                //My own vertex input========

                UNITY_SETUP_INSTANCE_ID(v);
                #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(os);
                #endif
                TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
                #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
                    o.tex = TRANSFORM_TEX(v.uv0, _MainTex);

                    #ifdef _PARALLAXMAP
                        TANGENT_SPACE_ROTATION;
                        o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
                    #endif
                #endif
            }

            ENDCG
        }
        // ------------------------------------------------------------------
    }
    CustomEditor "StandardShaderGUI"
}