

float4 _HexCellData_TexelSize;

half3 _BackgroundColor;

#pragma multi_compile _ HEX_MAP_VISION
struct Attributes
{
    float4 positionOS : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    float4 normalOS : NORMAL;
    float4 tangentOS : TANGENT;

    float3 texcoord2 : TEXCOORD2; //z:地形索引
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 viewDirWS : TEXCOORD2;
    float3 normalWS : TEXCOORD3;
                
    float3 terrain : TEXCOORD4;
    float4 opacity : TEXCOORD5;

    float4 shadowCoord : TEXCOORD6;
};

struct VaryingsTangent
{
    float4 positionCS : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 viewDirWS : TEXCOORD2;
    float3 normalWS : TEXCOORD3;
                
    float3 terrain : TEXCOORD4;

    float3 tangentWS : TEXCOORD5;
    float3 bitangentWS : TEXCOORD6;
};

//float4 FilterCellData(float4 data)
//{
//#if defined(HEX_MAP_VISION)
//    data.xy = 1;
//#endif
//    return data;
//}

//float4 GetCellData(Varyings IN, int index)
//{
//    float2 uv;
//    uv.x = (IN.terrain[index] + 0.5) * _HexCellData_TexelSize.x;
//    float row = floor(uv.x);
//    uv.x -= row;
//    uv.y = (row + 0.5) * _HexCellData_TexelSize.y;
//    float4 data = _HexCellData.Sample(sampler_HexCellData, uv);
//    data.w *= 255;
//    return FilterCellData(data);
//}

//float4 GetCellData(float2 cellDataCoordinates)
//{
//    float2 uv = cellDataCoordinates + 0.5;
//    uv.x *= _HexCellData_TexelSize.x;
//    uv.y *= _HexCellData_TexelSize.y;
    
//    return FilterCellData(_HexCellData.Sample(sampler_HexCellData, uv));
//}

//float4 GetCellData(VaryingsTangent IN, int index)
//{
//    float2 uv;
//    uv.x = (IN.terrain[index] + 0.5) * _HexCellData_TexelSize.x;
//    float row = floor(uv.x);
//    uv.x -= row;
//    uv.y = (row + 0.5) * _HexCellData_TexelSize.y;
//    float4 data = _HexCellData.Sample(sampler_HexCellData, uv);
//    data.w *= 255;
//    return FilterCellData(data);
//}

