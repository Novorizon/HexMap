Shader "Custom/Water Shore"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" { }
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        _NormalTex ("NormalTex", 2D) = "white" {}
        _NormalScale ("NormalScale", range(0, 1)) = 1
        _WaterColor ("WaterColor", Color) = (1, 1, 1, 1)
        _WaterFlow ("WaterFlow", Vector) = (1,1,1,1)
        _RimPower ("RimPower", float) = 1
        _SSSScale ("SSSScale", float) = 1
        _FoamScale ("FoamScale", float) = 1
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
            float4 _WaterColor;
            float4 _WaterFlow;
            float _NormalScale;
            float _RimPower;
            float _SSSScale;
            float _FoamScale;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            float4 _MainTex_ST;
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalTex);
            SAMPLER(sampler_NormalTex);            
           
            VaryingsTangent vert(Attributes IN)
            {
                VaryingsTangent OUT = (VaryingsTangent)0;
                OUT.color = IN.color;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz, IN.tangentOS);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.viewDirWS = GetCameraPositionWS() - positionInputs.positionWS;
                OUT.uv = IN.uv;

                OUT.tangentWS = normalInputs.tangentWS;
                OUT.bitangentWS = normalInputs.bitangentWS;
                OUT.normalWS = normalInputs.normalWS;
                
                OUT.terrain = IN.texcoord2.xyz;
                return OUT;
            }

            half4 frag(VaryingsTangent IN): SV_TARGET
            {

                //float waves = Waves(IN.positionWS.xz, _MainTex, sampler_MainTex, _Time.y);
                //color = saturate(_Color + waves);

                //return IN.uv.x;
                //return color * visibility;

                float3 positionWS = IN.positionWS.xyz;
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float3 viewDir = normalize(IN.viewDirWS);
                float3 halfDir = normalize(lightDir + viewDir);

                float2 uv = IN.positionWS.xz * _MainTex_ST.xy;
                float3 normal = SampleNormal(uv, _Time.x, _NormalTex, sampler_NormalTex, _NormalScale, _WaterFlow);
                float3 normalWS = TangentToWorldDir(normal, IN.tangentWS.xyz, IN.bitangentWS.xyz, IN.normalWS.xyz);

                float NdotL = saturate(dot(normalWS, lightDir));
                float HdotN = saturate(dot(normalWS, halfDir));
                float NdotV = saturate(dot(normalWS, viewDir));

                float3 diffuse = _WaterColor.rgb * NdotL;
                float3 specular = _MainLightColor.rgb * pow(HdotN, _Glossiness * 512) * 0.9;
                float3 rim = pow(1 - saturate(NdotV), _RimPower) * _MainLightColor.rgb;

                half3 directLighting = dot(lightDir, half3(0, 1, 0)) * _MainLightColor.rgb;
                directLighting += saturate(pow(dot(viewDir, -lightDir), 3)) * 5 * _MainLightColor.rgb;
                half3 sss = directLighting * _SSSScale;

                float shore = IN.uv.y;
                float foam = Foam(shore, IN.positionWS.xz, _MainTex, sampler_MainTex, _Time.y);
                float waves = Waves(IN.positionWS.xz, _MainTex, sampler_MainTex, _Time.y);
                waves *= 1 - shore;
                float3 foamColor = max(foam, waves) * _FoamScale * shore;

                return float4(specular + diffuse + rim + sss + foamColor, 1);
            }
            ENDHLSL

        }
    }

    FallBack "Hidden/Shader Graph/FallbackError"
}