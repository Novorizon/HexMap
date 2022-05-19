using Sirenix.OdinInspector;
using UnityEngine;

public enum HexEdgeType
{
    Flat,
    Slope, 
    Cliff
}
public enum RoadNoiseType
{
    [LabelText("Perlin噪声")]
    Perlin,
    [LabelText("分形布尔噪声")]
    FBM,
}
