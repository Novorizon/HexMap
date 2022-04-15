// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Unlit/Terrain"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("SplatArray", 2DArray) = "" { }
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        
        Pass
        {
            Name "Pass"
            Tags {  }
            
            // Render State
            Blend One Zero, One Zero
            Cull Back
            ZTest LEqual
            ZWrite On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.5
            #pragma multi_compile_instancing
            
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            CBUFFER_END
            
            Texture2DArray _MainTex;
            SamplerState sampler_MainTex;


            struct Attributes
            {
                float4 positionOS: POSITION;
                float3 texcoord2: TEXCOORD2;
                float4 color: COLOR;
            };

            struct Varyings
            {
                float4 color: COLOR;
                float4 worldPos: SV_POSITION;
                float3 terrain: TEXCOORD0;
            };
            
            

            float4 GetTerrainColor(Varyings IN, int index)
            {
                float3 uvw = float3(IN.worldPos.xz * 0.02, IN.terrain[index]);
                float4 color = _MainTex.Sample(sampler_MainTex, uvw);
                return color * IN.color[index];
            }
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT ;
                OUT.worldPos = TransformObjectToHClip(IN.positionOS);
                OUT.color = IN.color;
                OUT.terrain = IN.texcoord2.xyz;
                return OUT;
            }

            half4 frag(Varyings IN): SV_TARGET
            {
                half4 color = GetTerrainColor(IN, 0) + GetTerrainColor(IN, 1) + GetTerrainColor(IN, 2);
                return color;
            }
            ENDHLSL

        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}