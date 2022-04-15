// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Lit/Terrain"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Splat Array", 2DArray) = "" { }
        _TerrainTypeTexture ("Terrain Type", 2D) = "white" { }
        _TerrainOpacityTexture ("Terrain Opacity", 2D) = "white" { }
        _GridTex ("Grid Texture", 2D) = "white" { }
        _Specular ("Specular", Color) = (0.2, 0.2, 0.2)
        _Smoothness ("Smoothness", float) = 10
        _Cutoff ("Cutoff", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        LOD 200
        
        Pass
        {
            Name "URPSimpleLit"
            Tags { "LightMode" = "UniversalForward" }
            
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
            half4 _Specular;
            float _Smoothness;
            float _Cutoff;
            //half3 _BackgroundColor;
            CBUFFER_END
            
            Texture2DArray _MainTex;
            SamplerState sampler_MainTex;
            
            SamplerState sampler_TerrainTypeTexture;
            TEXTURE2D(_TerrainTypeTexture);

            SamplerState sampler_TerrainOpacityTexture;
            TEXTURE2D(_TerrainOpacityTexture);

            TEXTURE2D(_GridTex);
            SAMPLER(sampler_GridTex);

            
            float4 GetOpacity(Varyings IN, float2 uv)
            {
                return _TerrainOpacityTexture.Sample(sampler_TerrainOpacityTexture, uv);
            }
            

            void GetCellData(Varyings IN, int index, out float4 data, out float2 uv)
            {
                uv.x = (IN.terrain[index] + 0.5) * _HexCellData_TexelSize.x;
                float row = floor(uv.x);
                uv.x -= row;
                uv.y = (row + 0.5) * _HexCellData_TexelSize.y;
                
                data = _TerrainTypeTexture.Sample(sampler_TerrainTypeTexture, uv);
                data.w *= 255;
            }
            

            half4 GetTerrainColor(Varyings IN, int index, float4 opacity)
            {
                half4 color = 0;
                int w = IN.terrain[index];
                for (int i = 0; i < 4; i++)
                {
                    float3 uvw = float3(IN.positionWS.xz * 0.02, i);// IN.terrain[index]);
                    half4 cc = (w & 1) * _MainTex.Sample(sampler_MainTex, uvw) * opacity[i];
                    w = w >>1;
                    color = color + cc;
                }
                
                return color * (IN.color[index]);
            }
            
            
            

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
                half3 specular = LightingSpecular(light.color, light.direction, normalize(IN.normalWS), normalize(IN.viewDirWS), _Specular, _Smoothness);
                //附加光照
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, IN.positionWS);
                    diffuse += LightingLambert(light.color, light.direction, IN.normalWS);
                    specular += LightingSpecular(light.color, light.direction, normalize(IN.normalWS), normalize(IN.viewDirWS), _Specular, _Smoothness);
                }
                
                //网格线框
                half4 grid = 1;
                #if defined(GRID_ON)
                    float2 gridUV = IN.positionWS.xz;
                    gridUV.x *= 1 / (4 * 8.66025404);
                    gridUV.y *= 1 / (2 * 15.0);
                    grid = SAMPLE_TEXTURE2D(_GridTex, sampler_GridTex, gridUV);
                #endif
                
                float4 cell0;
                float4 cell1;
                float4 cell2;

                float2 uv;
                GetCellData(IN, 0, cell0, uv);
                float4 opacity0 = GetOpacity(IN, uv);

                GetCellData(IN, 1, cell1, uv);
                float4 opacity1 = GetOpacity(IN, uv);

                GetCellData(IN, 2, cell2, uv);
                float4 opacity2 = GetOpacity(IN, uv);

                //float4 cell0 = GetCellData(IN, 0);
                //float4 cell1 = GetCellData(IN, 1);
                //float4 cell2 = GetCellData(IN, 2);

                IN.terrain.x = cell0.w;
                IN.terrain.y = cell1.w;
                IN.terrain.z = cell2.w;
                


                half4 color = GetTerrainColor(IN, 0, opacity0) + GetTerrainColor(IN, 1, opacity1) + GetTerrainColor(IN, 2, opacity2);
                color.rgb = diffuse * color.rgb * grid.xyz * _Color.rgb + specular;
                return color;
                

                //SurfaceData surfaceData = (SurfaceData)0;
                //surfaceData.alpha = color.a ;
                //surfaceData.albedo = color.rgb ;
                //surfaceData.smoothness = _Smoothness;
                //surfaceData.emission = _BackgroundColor * (1 - explored);
                //surfaceData.occlusion = explored;
                
                ////surfaceData.normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
                //InputData inputData = InitializeInputData(IN, surfaceData.normalTS);
                

                //half4 UniversalFragmentPBR(InputData inputData, half3 albedo, half metallic, half3 specular,
                //half smoothness, half occlusion, half3 emission, half alpha)
                //half4 color = UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL

        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}