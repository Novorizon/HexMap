// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Lit/Feature"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("MainTex", 2D) = "" { }
        _SpecularColor ("SpecularColor", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", float) = 10
        _Cutoff ("Cutoff", float) = 0.5
        [NoScaleOffset] _GridCoordinates ("Grid Coordinates", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        LOD 200
        
        Pass
        {
            Name "URPSimpleLit"
            Tags { "LightMode" = "UniversalForward" }
            LOD 200
            
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
            
            #pragma multi_compile _ GRID_ON
            
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
            #include "HexCellData.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float4 _GridTex_ST;
            float4 _SpecularColor;
            float _Smoothness;
            float _Cutoff;
            CBUFFER_END
            
            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            Texture2D _GridCoordinates;
            SamplerState sampler_GridCoordinates;


            Varyings vert(Attributes IN)
            {
                Varyings OUT ;

                OUT.color = IN.color;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.viewDirWS = GetCameraPositionWS() - positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = IN.uv;

                OUT.terrain = IN.texcoord2.xyz;
                
                return OUT;
            }

            half4 frag(Varyings IN): SV_TARGET
            {

                //主光
                Light light = GetMainLight();
                half3 diffuse = LightingLambert(light.color, light.direction, IN.normalWS);
                half3 specular = LightingSpecular(light.color, light.direction, normalize(IN.normalWS), normalize(IN.viewDirWS), _SpecularColor, _Smoothness);
                //附加光照
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, IN.positionWS);
                    diffuse += LightingLambert(light.color, light.direction, IN.normalWS);
                    specular += LightingSpecular(light.color, light.direction, normalize(IN.normalWS), normalize(IN.viewDirWS), _SpecularColor, _Smoothness);
                }
                
                
                float4 gridUV = float4(IN.positionWS.xz, 0, 0);
                gridUV.x *= 1 / (4 * 8.66025404);
                gridUV.y *= 1 / (2 * 15.0);
                float2 cellDataCoordinates = floor(gridUV.xy) + _GridCoordinates.Sample(sampler_GridCoordinates, gridUV).rg;
                cellDataCoordinates *= 2;
                
                half4 color = half4(_BackgroundColor, 1);
                color = _MainTex.Sample(sampler_MainTex, IN.uv) * _Color;
                color.rgb = diffuse * color.rgb  + specular;

                return color;
            }
            ENDHLSL

        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}