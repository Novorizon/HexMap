// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Lit/Terrain"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Terrain  Texture Array", 2DArray) = "" { }
        [HideInInspector] _EditorTexture ("Editor Texture", 2D) = "white" { }
        [HideInInspector] _TerrainTypeTexture ("Terrain Type", 2D) = "white" { }
        [HideInInspector] _TerrainOpacityTexture ("Terrain Opacity", 2D) = "white" { }
        [HideInInspector] _RoadTexture ("RoadTexture", 2D) = "white" { }
        [HideInInspector] _NoiseTex ("Noise Texture", 2D) = "white" { }
        [HideInInspector] _GridTex ("Grid Texture", 2D) = "white" { }
        _Specular ("Specular", Color) = (0.2, 0.2, 0.2)
        _Smoothness ("Smoothness", float) = 10
        _Cutoff ("Cutoff", float) = 0.5
        [HideInInspector] _CellX ("CellX", float) = 1
        [HideInInspector] _Radius ("Radius", float) = 10
        [HideInInspector] _Test ("_Test", float) = 0.5
        [Toggle(TRIPLANAR)] _TriPlanar ("Triplanar", float) = 1
        [Toggle(EDITOR)] _EDITOR ("EDITOR", float) = 1
        _Blend ("Blend", Range(0.1, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 200
        
        Pass
        {
            Name "URPSimpleLit"
            Tags { "LightMode" = "UniversalForward" }
            
            // Render State
            Blend SrcAlpha OneMinusSrcAlpha
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
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ GRID_ON
            #pragma multi_compile _ TRIPLANAR
            #pragma multi_compile _ EDITOR
            
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
            #include "HexCellData.hlsl"
            #include "Noise.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float4 _GridTex_ST;
            half4 _Specular;
            float _Smoothness;
            float _Cutoff;
            int _CellX;
            float _Radius;
            float _Test;
            float _Blend;
            CBUFFER_END
            
            Texture2DArray _MainTex;
            SamplerState sampler_MainTex;
            
            SamplerState sampler_EditorTexture;
            TEXTURE2D(_EditorTexture);
            
            SamplerState sampler_TerrainTypeTexture;
            TEXTURE2D(_TerrainTypeTexture);

            SamplerState sampler_TerrainOpacityTexture;
            TEXTURE2D(_TerrainOpacityTexture);

            SamplerState sampler_RoadTexture;
            TEXTURE2D(_RoadTexture);
            SamplerState sampler_NoiseTex;
            TEXTURE2D(_NoiseTex);

            TEXTURE2D(_GridTex);
            SAMPLER(sampler_GridTex);

            float4 GetOpacity(Varyings IN, float2 uv)
            {
                return _TerrainOpacityTexture.Sample(sampler_TerrainOpacityTexture, uv);
            }
            

            float2 GetUV(Varyings IN, int index)
            {
                float2 uv;
                uv.x = (IN.terrain[index] + 0.5) * _HexCellData_TexelSize.x;
                float row = floor(uv.x);
                uv.x -= row;
                uv.y = (row + 0.5) * _HexCellData_TexelSize.y;
                return uv;
            }

            void GetCellData(float2 uv, out float4 data)
            {
                data = _TerrainTypeTexture.Sample(sampler_TerrainTypeTexture, uv);
                data.r *= 255;
                data.g *= 255;
                data.b *= 255;
                data.w *= 255;
            }

            float4 GetRoadData(float2 uv)
            {
                float4 data = _RoadTexture.Sample(sampler_RoadTexture, uv);
                data.r *= 255;
                return data;
            }


            half4 GetRoadColor(Varyings IN, int b)
            {
                half4 color = 0;
                
                for (int i = 0; i < 4; i++)
                {
                    //float3 uvw = float3(IN.positionWS.xz * 0.052, i);
                    
                    float3 uvw = float3(IN.positionWS.xz * 1 / _Radius, i);
                    if (b > 0)
                    {
                        color = _MainTex.Sample(sampler_MainTex, uvw) ;
                    }
                    b = b >>1;
                }
                return color;
            }
            

            half GetRoadAlpha(half width, half noise, half noiseIF, half d)
            {
                half blend = 1 - width;// PerlinNoise(half2(x, z)) ;
                blend *= noise + noiseIF;// -noiseIF*d+1;
                blend = smoothstep(0.4, 0.7, blend);
                return saturate(blend * (1 - d * d));
            }
            
            half GetRoadAlpha(half width, half noise, half noiseIF)
            {
                half blend = 1 - width;// PerlinNoise(half2(x, z)) ;
                blend *= noise + noiseIF;// -noiseIF*d+1;
                blend = smoothstep(0.4, 0.7, blend);
                return blend;
            }
            half4 GetTerrainColor(Varyings IN, int index, float4 cell, float4 opacity, float2 uv)
            {
                half4 color = 0;
                int w = cell.w;
                //if(w==0)


                #ifdef TRIPLANAR
                    float3 blend = pow(abs(normalize(IN.normalWS)), _Blend);
                    blend /= dot(blend, 1.0);
                    for (int i = 0; i < 4; i++)
                    {
                        float3 uvx = float3(IN.positionWS.zy * 0.2 / _Radius, i);
                        float3 uvy = float3(IN.positionWS.xz * 0.2 / _Radius, i);
                        float3 uvz = float3(IN.positionWS.xy * 0.2 / _Radius, i);
                        half4 colorx = (w & 1) * _MainTex.Sample(sampler_MainTex, uvx);
                        half4 colory = (w & 1) * _MainTex.Sample(sampler_MainTex, uvy);
                        half4 colorz = (w & 1) * _MainTex.Sample(sampler_MainTex, uvz);
                        color = color + (w & 1) * opacity[i] * (blend.x * colorx + blend.y * colory + blend.z * colorz);

                        w = w >>1;
                    }
                #else
                    for (int i = 0; i < 4; i++)
                    {
                        float3 uvy = float3(IN.positionWS.xz * 0.052 / _Radius, i);
                        half4 colory = (w & 1) * _MainTex.Sample(sampler_MainTex, uvy);
                        color = color + (w & 1) * opacity[i] * colory;

                        w = w >>1;
                    }
                #endif

                if (cell.b > 0)
                {
                    int id = cell.r * 256 + cell.g;
                    int texType = cell.b;
                    

                    float4 road = GetRoadData(uv);
                    //int noiseType = ((int)road.r >>7) & 1;
                    int dir = (int)road.r & 63;
                    float width = road.g * _Radius;
                    float noiseIF = road.a;
                    
                    int rE = (dir & 1) | ((dir >>3) & 1);
                    int rNE = ((dir >>1) & 1) | ((dir >>4) & 1);
                    int rSE = ((dir >>2)) | ((dir >>5) & 1);


                    float x = IN.positionWS.x;
                    float z = IN.positionWS.z;
                    float r = 0.866 * _Radius;

                    int p = (id / _CellX) % 2;
                    float x0 = 2 * (id % _CellX) * r + (p * r);
                    float y0 = 1.5 * (id / _CellX) * _Radius ;
                    
                    half4 noise = _NoiseTex.Sample(sampler_NoiseTex, IN.positionWS.xz * 0.025) ;
                    width = width * (1 + noise.x * noise.y) ;
                    width = width * 0.5;
                    half radius = width;
                    
                    half rr = (x - x0) * (x - x0) + (z - y0) * (z - y0)  ;
                    bool isCircle = rr <= radius * radius + noise.x *noise.y* radius;
                    isCircle = isCircle && ((rE & rNE) > 0 || (rE & rSE) > 0 || (rNE & rSE) > 0);
                    bool isNE = ((dir >>0) & 1) ;
                    bool isSW = ((dir >>3) & 1) ;
                    
                    bool isE = ((dir >>1) & 1);
                    bool isW = ((dir >>4) & 1);
                    bool isSE = ((dir >>2) & 1);
                    bool isNW = ((dir >>5) & 1);
                    
                    half roadalpha = road.g * 0.5;

                    if (isCircle && (isE || isW || isNE || isSW || isSE || isNW))
                    {
                        half4 roadColor = GetRoadColor(IN, texType);
                        half blend = GetRoadAlpha(roadalpha, noise.x, noiseIF);
                        roadColor.a = saturate(blend + 0.6);
                        return roadColor * IN.color[index] * blend + color * IN.color[index] * (1 - blend);
                    }
                    
                    float E = y0;
                    half d = abs(z - E) ;
                    if (isE && x > x0 && d < width)
                    {
                        half4 roadColor = GetRoadColor(IN, texType);
                        if (isCircle)
                            return roadColor * IN.color[index] * 1 ;

                        half dd = d / width;
                        half blend = GetRoadAlpha(roadalpha, noise.x, noiseIF, dd);
                        roadColor.a = saturate(blend + 0.6);
                        return roadColor * IN.color[index] * blend + color * IN.color[index] * (1 - blend);
                    }
                    if (isW && x < x0 && d < width)
                    {
                        half dd = d / width;
                        half4 roadColor = GetRoadColor(IN, texType);
                        half blend = GetRoadAlpha(roadalpha, noise.x, noiseIF, dd);
                        roadColor.a = saturate(blend + 0.6);
                        return roadColor * IN.color[index] * blend + color * IN.color[index] * (1 - blend);
                    }
                    
                    float NE = 1.732 * (x - x0) + y0;
                    half a = abs(x - (z - y0) / 1.732 - x0);
                    half b = abs(z - NE);
                    half c = sqrt(a * a + b * b);
                    d = a * b / c;
                    
                    float h = -0.5774 * (x - x0) + y0;
                    if (isNE && z > h && d < width)
                    {
                        half dd = d / width;
                        half4 roadColor = GetRoadColor(IN, texType);
                        half blend = GetRoadAlpha(roadalpha, noise.x, noiseIF, dd);
                        roadColor.a = saturate(blend + 0.6);

                        return roadColor * IN.color[index] * blend + color * IN.color[index] * (1 - blend);
                    }
                    if (isSW && z < h && d < width)
                    {
                        half dd = d / width;
                        half4 roadColor = GetRoadColor(IN, texType);
                        half blend = GetRoadAlpha(roadalpha, noise.x, noiseIF, dd);
                        roadColor.a = saturate(blend + 0.6);

                        return roadColor * IN.color[index] * blend + color * IN.color[index] * (1 - blend);
                    }
                    
                    float SE = -1.732 * (x - x0) + y0;
                    a = abs(x - (y0 - z) / 1.732 - x0);
                    b = abs(z - SE);
                    c = sqrt(dot(a, a) + dot(b, b));
                    d = a * b / c;
                    h = 0.866 * (x - x0) + y0;
                    if (isSE && z < h && d < width)
                    {
                        
                        half dd = d / width;
                        half4 roadColor = GetRoadColor(IN, texType);
                        half blend = GetRoadAlpha(roadalpha, noise.x, noiseIF, dd);
                        roadColor.a = saturate(blend + 0.6);
                        return roadColor * IN.color[index] * blend + color * IN.color[index] * (1 - blend);
                    }
                    if (isNW && z > h && d < width)
                    {
                        half dd = d / width;

                        half4 roadColor = GetRoadColor(IN, texType);
                        half blend = GetRoadAlpha(roadalpha, noise.x, noiseIF, dd);
                        roadColor.a = saturate(blend + 0.6);
                        return roadColor * IN.color[index] * blend + color * IN.color[index] * (1 - blend);
                    }
                }
                
                return saturate(color * (IN.color[index])) ;//* a + (1 - a) *  half4(1, 1, 1, 1);
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
                
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);

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

                float2 uv0 = GetUV(IN, 0);
                float2 uv1 = GetUV(IN, 1);
                float2 uv2 = GetUV(IN, 2);

                GetCellData(uv0, cell0);
                GetCellData(uv1, cell1);
                GetCellData(uv2, cell2);

                float4 opacity0 = GetOpacity(IN, uv0);
                float4 opacity1 = GetOpacity(IN, uv1);
                float4 opacity2 = GetOpacity(IN, uv2);


                IN.terrain.x = cell0.w;
                IN.terrain.y = cell1.w;
                IN.terrain.z = cell2.w;
                half4 color = 0;
                
                #ifdef EDITOR
                    if (cell0.w == 0 && cell1.w == 0 && cell2.w == 0)
                    {
                        color = _EditorTexture.Sample(sampler_EditorTexture, IN.positionWS.xz * 0.2 / _Radius);
                    }
                    else
                    {
                        half shadow = MainLightRealtimeShadow(IN.shadowCoord);

                        color = GetTerrainColor(IN, 0, cell0, opacity0, uv0) + GetTerrainColor(IN, 1, cell1, opacity1, uv1) + GetTerrainColor(IN, 2, cell2, opacity2, uv2);
                        color.rgb = diffuse * color.rgb * grid.xyz * _Color.rgb + specular;
                        color.rgb *= shadow;

                        float3 blend = pow(abs(IN.normalWS), _Blend);
                    }
                    return color;

                #else
                    half shadow = MainLightRealtimeShadow(IN.shadowCoord);

                    color = GetTerrainColor(IN, 0, cell0, opacity0, uv0) + GetTerrainColor(IN, 1, cell1, opacity1, uv1) + GetTerrainColor(IN, 2, cell2, opacity2, uv2);
                    color.rgb = diffuse * color.rgb * grid.xyz * _Color.rgb + specular;
                    color.rgb *= shadow;

                    float3 blend = pow(abs(IN.normalWS), _Blend);
                    return color;
                #endif
            }
            ENDHLSL

        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}