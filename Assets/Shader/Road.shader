// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Road"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" { }
        _SandTex ("SandTex", 2D) = "white" { }
        _SpecularColor ("SpecularColor", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", float) = 10
        _NoiseTex ("NoiseTex", 2D) = "white" { }
        _Cutoff ("Cutoff", float) = 0.
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        
        LOD 200
        Offset -1, -1
        Pass
        {
            Name "Pass"
            Tags {  }
            
            // Render State
            //Blend One Zero, One Zero
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
            
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
            #include "HexCellData.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float4 _SpecularColor;
            float _Smoothness;
            float _Cutoff;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            float4 _NoiseTex_ST;
            TEXTURE2D(_SandTex);
            SAMPLER(sampler_SandTex);
            

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
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

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
                

                float4 noise = _NoiseTex.Sample(sampler_NoiseTex, IN.positionWS.xz * 0.025);

                float4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.positionWS.xz * _MainTex_ST.zw);
                float4 sandColor = SAMPLE_TEXTURE2D(_SandTex, sampler_SandTex, IN.positionWS.xz * _MainTex_ST.zw);
                
                half4 color = _Color * (noise.y * 0.75 + 0.25);
                color.rgb = diffuse * color.rgb;//+ specular;
                
                float blend = IN.uv.x;
                blend *= noise.x + 0.5;
                blend = smoothstep(0.1, 0.3, blend);
                color.a = blend;

                mainColor.a = blend;

                float bs = IN.uv.x;
                bs *= noise.x + 0.5;
                bs = smoothstep(0.2, 1, bs);
                sandColor.a = bs;
                float4 finalColor = lerp(sandColor, mainColor, bs);
                return finalColor;

                return mainColor;
            }
            ENDHLSL

        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}