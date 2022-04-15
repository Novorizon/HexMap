using Sirenix.OdinInspector;
using UnityEngine;



public enum Property
{
    Editor,
    Settings,
    Brush,
    Grid,
    Button,
    CellList

}

public enum EditorMode
{
    Brush,
    Feature,
    Pathfinding,
    FogOfWar,
    Settings,
}


[System.Flags]
public enum FeatureLevel
{
    Erase = 0,
    Sub1 = 1 << 1,
    Sub2 = 1 << 2,
    Sub3 = 1 << 3,
}
public enum TextMode
{
    None,
    Coordinate,
    Distance,
    Pathfinding,
}


[System.Flags]
public enum UpdateType
{
    None,
    ReCreate,
    Brush,
    Grid,
}


public enum BrushType
{
    None=0,
    Terrain = 1 << 1,
    Elevation = 1 << 2,
    River = 1 << 3,
    Road = 1 << 4,
    Water = 1 << 5,
    Feature = 1 << 6,
    Special = 1 << 9,
}

public enum TerrainType
{
    None,
    Sand,
    Grass,
    Sand1,
    Grass1,
    Sand2,
}


public enum RiverType
{
    [LabelText("绘制")]
    Yes,
    [LabelText("擦除")]
    No,
}

public enum RoadType
{
    Yes,
    No,
}

public enum WaterLevel
{
    Yes,
    No,
}