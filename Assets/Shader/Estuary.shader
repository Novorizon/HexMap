Shader "Custom/Estuary"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" { }
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 200
        
        Pass
        {
            Name "Pass"
            Tags {  }
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
            

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
            #include "HexCellData.hlsl"
            #include "Water.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _Glossiness;
            half _Metallic;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            float4 _MainTex_ST;
            
            SamplerState sampler_MainTex;
            
            

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
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
                float shore = IN.uv.y;
                float foam = Foam(shore, IN.positionWS.xz, _MainTex, sampler_MainTex, _Time.y);
                float waves = Waves(IN.positionWS.xz, _MainTex, sampler_MainTex, _Time.y);
                waves *= 1 - shore;
                float shoreWater = max(foam, waves);
                float river = River(IN.uv, _MainTex, sampler_MainTex, _Time.y);
                float water = lerp(shoreWater, river, IN.uv.x);
                
                half4 color = saturate(_Color + water);
                return color;
            }
            ENDHLSL

        }
    }

    FallBack "Hidden/Shader Graph/FallbackError"
}