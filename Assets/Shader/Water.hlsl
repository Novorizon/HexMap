
            
float Waves(float2 worldXZ, Texture2D noiseTex, SamplerState sampler_noiseTex, float _Time)
{
    float2 uv1 = worldXZ;
    uv1.y += _Time;
    
    float4 noise1 = noiseTex.Sample(sampler_noiseTex, uv1 * 0.025);

    float2 uv2 = worldXZ;
    uv2.x += _Time;
    float4 noise2 = noiseTex.Sample(sampler_noiseTex, uv2 * 0.025);

    float blendWave = sin((worldXZ.x + worldXZ.y) * 0.1 + (noise1.y + noise2.z) + _Time);
    blendWave *= blendWave;

    float waves = lerp(noise1.z, noise1.w, blendWave) + lerp(noise2.x, noise2.y, blendWave);
    noise1.z + noise2.x;
    return smoothstep(0.75, 2, waves);
}

float Foam(float shore, float2 worldXZ, Texture2D noiseTex, SamplerState sampler_noiseTex, float _Time)
{
    shore = sqrt(shore) * 0.9;

    float2 noiseUV = worldXZ + _Time * 0.25;
    float4 noise = noiseTex.Sample(sampler_noiseTex, noiseUV * 0.015);

    float distortion1 = noise.x * (1 - shore);
    float foam1 = sin((shore + distortion1) * 10 - _Time);
    foam1 *= foam1;

    float distortion2 = noise.y * (1 - shore);
    float foam2 = sin((shore + distortion2) * 10 + _Time + 2);
    foam2 *= foam2 * 0.7;

    return max(foam1, foam2) * shore;
}

float River(float2 riverUV, Texture2D noiseTex, SamplerState sampler_noiseTex, float _Time)
{
    float2 uv = riverUV;
    uv.x = uv.x * 0.0625 + _Time * 0.005;
    uv.y -= _Time * 0.25;
    float4 noise = noiseTex.Sample(sampler_noiseTex, uv);

    float2 uv2 = riverUV;
    uv2.x = uv2.x * 0.0625 - _Time * 0.0052;
    uv2.y -= _Time * 0.23;
    float4 noise2 = noiseTex.Sample(sampler_noiseTex, uv2);

    return noise.r * noise2.w;
}

float3 TangentToWorldDir(float3 normalTS, float3 tangentWS, float3 bitangentWS, float3 normalWS)
{
    float3x3 T2WMatrix = float3x3(tangentWS, bitangentWS, normalWS);
    float3 normal = mul(normalTS, T2WMatrix);
    return normal;
}

float3 SampleNormal(float2 uv, float time, Texture2D normalTex, SamplerState sampler_NormalTex, float normalScale, float4 waterFlow)
{
    float3 normalMap1 = UnpackNormal(SAMPLE_TEXTURE2D(normalTex, sampler_NormalTex, uv + time * waterFlow.xy));
    float3 normalMap2 = UnpackNormal(SAMPLE_TEXTURE2D(normalTex, sampler_NormalTex, uv + time * waterFlow.zw));

    float3 normal = BlendNormal(normalMap1, normalMap2);
    normal = lerp(half3(0, 0, 1), normal, normalScale);
    return normal;
}