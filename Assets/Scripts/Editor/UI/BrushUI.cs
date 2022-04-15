using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using HexMap;
using UnityEditor;

namespace WorldMapEditor
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class BrushAttribute : Attribute
    {
        public string group;
        public BrushAttribute(string group)
        {
            this.group = group;
        }

        public BrushAttribute()
        {
        }
    }

    [Serializable]
    public class TerrainType
    {
        [ToggleLeft, HideLabel, LabelWidth(1), OnValueChanged("OnValueChanged"), HorizontalGroup("TerrainType")]
        public bool use;
        [HideLabel, ReadOnly, HorizontalGroup("TerrainType")]
        public string name;
        [HideLabel, ProgressBar(0, 1), HorizontalGroup("TerrainType"), OnValueChanged("OnOpacityChanged")]
        public float opacity=1;

        [HideInInspector]
        public int id;
        public void OnValueChanged()
        {
            HexMapEditor.Instance.OnTerrainListChanged(id);
        }
        public void OnOpacityChanged()
        {
            HexMapEditor.Instance.OnOpacityChanged(id);
        }
    }

    public partial class HexMapEditor
    {
        UpdateType UType = UpdateType.None;
        BrushType BrushType;
        HexDirection dragDirection;
        HexCell previousCell;
        bool isDrag;
        int terrain = -1;

        int opacity = 1;//透明度

        [Title("笔刷")]
        [LabelText("橡皮擦"), ToggleLeft, PropertyOrder((int)Property.Brush), /*Brush("Erase"), */ShowIf("IsBrush"), OnValueChanged("OnEraseChanged"), HorizontalGroup("isErase")]
        public bool isErase;
        [LabelText("笔刷尺寸"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"), OnValueChanged("OnBrushSizeChanged"), ProgressBar(0, 10, Segmented = true, DrawValueLabel = true)]
        public int brushSize = 1;
        [LabelText("海拔"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"),] [Brush("Base"), LabelWidth(3), OnValueChanged("OnElevationChanged"), HorizontalGroup("Elevation")] public bool IsElevation = false;
        [LabelText("水位"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"),] [Brush("Water"), LabelWidth(3), OnValueChanged("OnWaterChanged"), HorizontalGroup("Water")] public bool IsWater = false;
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 3), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"),] [ProgressBar(0, 10), HideLabel, HorizontalGroup("Water")] public int WaterLevel;

        [LabelText("水体材质"), PropertySpace(SpaceBefore = 3), PropertyOrder((int)Property.Brush), ShowIf("IsWater"), OnValueChanged("OnWaterMatChanged"),]
        public Material WaterMat;

        [LabelText("水边材质"), PropertySpace(SpaceBefore = 3), PropertyOrder((int)Property.Brush), ShowIf("IsWater"), OnValueChanged("OnWaterShoreMatChanged"),]
        public Material WaterShoreMat;

        [LabelText("河流"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"),] [Brush("River"), LabelWidth(3), OnValueChanged("OnRiverChanged"), HorizontalGroup("River")] public bool IsRiver = false;
        [LabelText("道路"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"),] [Brush("Road "), LabelWidth(3), OnValueChanged("OnRoadChanged"), HorizontalGroup("Road")] public bool IsRoad = false;




        [HideLabel, PropertySpace(SpaceBefore = 3), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"), OnValueChanged("OnRiverMatChanged"), ShowIf("IsBrush"), HorizontalGroup("River")] public Material RiverMat;
        [HideLabel, PropertySpace(SpaceBefore = 3), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"), OnValueChanged("OnRoadMatChanged"), ShowIf("IsBrush"), HorizontalGroup("Road")] public Material RoadMat;


        [PropertySpace(SpaceBefore = 3, SpaceAfter = 3), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"),] [ProgressBar(0, 10), HideLabel, HorizontalGroup("Elevation")] public int Elevation;


        [LabelText("地形"), ToggleLeft, PropertySpace(SpaceBefore = 30), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"),] [Brush("Base"), LabelWidth(1), OnValueChanged("OnTerrainChanged"), HorizontalGroup("Terrain")] public bool IsTerrain = false;
        //[LabelText("混合"), ToggleLeft, PropertySpace(SpaceBefore = 30), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"), LabelWidth(1),HorizontalGroup("Terrain")] 
        //public bool IsTerrainBlend = false;
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false), LabelText("地形数组"), PropertySpace(SpaceBefore = 3, SpaceAfter = 3), PropertyOrder((int)Property.Brush), ShowIf("IsBrush"), HideLabel,]
        public List<TerrainType> TerrainList;

    }
}