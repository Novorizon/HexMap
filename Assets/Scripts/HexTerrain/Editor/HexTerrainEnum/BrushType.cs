using Sirenix.OdinInspector;



public enum BrushType
{
    None = 0,
    Terrain = 1 << 1,
    Elevation = 1 << 2,
    River = 1 << 3,
    Road = 1 << 4,
    Water = 1 << 5,
    Feature = 1 << 6,
    Special = 1 << 9,
}
