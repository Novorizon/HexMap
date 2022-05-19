using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using HexMap;
using System.Reflection;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif


public partial class HexTerrain
{
#if UNITY_EDITOR
    UpdateType UType = UpdateType.None;
    BrushType BrushType= BrushType.None;
    HexDirection dragDirection;
    HexCell previousCell;
    bool isDrag;
    int terrainCost = 0;
    int terrain = 0;
    int road = 0;

    int opacity = 1;//透明度

    [Title("笔刷")]
    [LabelText("笔刷尺寸"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"), OnValueChanged("OnBrushSizeChanged"), ProgressBar(0, 10, Segmented = true, DrawValueLabel = true)]
    public int brushSize = 1;

    [LabelText("橡皮擦"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), /*Brush("Erase"), */ShowIf("IsBrush"), OnValueChanged("OnEraseChanged"), HorizontalGroup("isErase")]
    public bool isErase;

    [LabelText("地形"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"),] [Brush("Base"), LabelWidth(1), OnValueChanged("OnTerrainChanged"), HorizontalGroup("Terrain")] public bool IsTerrain = false;

    [LabelText("海拔"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"),]
    [Brush("Base"), LabelWidth(3), OnValueChanged("OnElevationChanged"), HorizontalGroup("Elevation")]
    public bool IsElevation = false;
    [PropertySpace(SpaceBefore = 10, SpaceAfter = 3), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"),]
    [ProgressBar(0, 10), HideLabel, HorizontalGroup("Elevation")]
    public int Elevation;

    [LabelText("水位"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"),]
    [Brush("Water"), LabelWidth(3), OnValueChanged("OnWaterChanged"), HorizontalGroup("Water")]
    public bool IsWater = false;

    [PropertySpace(SpaceBefore = 10, SpaceAfter = 3), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"),]
    [ProgressBar(0, 10), HideLabel, HorizontalGroup("Water")]
    public int WaterLevel;

    [LabelText("水体材质"), PropertySpace(SpaceBefore = 3), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsWater"), OnValueChanged("OnWaterMatChanged"),]
    public Material WaterMat;
    [LabelText("水边材质"), PropertySpace(SpaceBefore = 3), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsWater"), OnValueChanged("OnWaterShoreMatChanged"),]
    public Material WaterShoreMat;
    [LabelText("入海口材质"), PropertySpace(SpaceBefore = 3), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsWater"), OnValueChanged("OnEstuaryMatChanged"),]
    public Material EstuaryMat;

    [LabelText("河流"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"), Brush("River"), LabelWidth(3), OnValueChanged("OnRiverChanged"), HorizontalGroup("River")]
    public bool IsRiver = false;

    [HideLabel, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"), OnValueChanged("OnRiverMatChanged"), ShowIf("IsBrush"), HorizontalGroup("River")]
    public Material RiverMat;

    [LabelText("道路"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"), Brush("Road "), LabelWidth(3), OnValueChanged("OnRoadChanged"), VerticalGroup("Road")]
    public bool IsRoad = false;

    [LabelText("宽度系数"), ProgressBar(0, 1), LabelWidth(85), PropertySpace(SpaceBefore = 3), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsRoad"), HorizontalGroup("roadWidth", marginLeft: 23)]
    public float roadWidth;

    [LabelText("噪声系数"), ProgressBar(0, 1), LabelWidth(85), PropertySpace(SpaceBefore = 3), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsRoad"), HorizontalGroup("roadNoise", marginLeft: 23)]
    public float roadNoiseIF;



    [LabelText("地形数组"), PropertySpace(SpaceBefore = 30, SpaceAfter = 3), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsBrush"),]// OnValueChanged("CreateTextures"),]
    public List<TerrainTexture> textures;

#endif
}
