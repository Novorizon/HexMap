
using UnityEngine;
using HexMap;
using System.Collections.Generic;
using Unity.Mathematics;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

public partial class HexTerrain
{
#if UNITY_EDITOR

    [InfoBox("按下Shift，点击左键选择起始点。松开Shift，点击左键选择终点。")]
    [LabelText("显示距离"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Settings), HorizontalGroup("Distance"), OnValueChanged("OnShowDistanceChanged"), ShowIf("IsPathfinding")]
    public bool showDistance = false;


    [Title("寻路代价")]
    [HideInInspector, LabelText("道路"), LabelWidth(40), PropertySpace(SpaceBefore = 30), PropertyOrder((int)PropertyOrder.Brush), OnValueChanged("OnRoadCostChanged"), ShowIf("IsPathfinding")]
    public int RoadCost = 1;
    [HideInInspector, LabelText("平地"), LabelWidth(40), PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), OnValueChanged("OnFlatCostChanged"), ShowIf("IsPathfinding")]
    public int FlatCost = 2;
    [HideInInspector, LabelText("斜坡"), LabelWidth(40), PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), OnValueChanged("OnSlopeCostChanged"), ShowIf("IsPathfinding")]
    public int SlopeCost = 3;

    [Button("Ring"), ShowIf("IsPathfinding")]
    public void GetRing()
    {

        List<HexCell> cells = HexMapMgr.Instance.cells;
        if (cells == null)
            return;

        List<HexCell> results = HexMapMgr.Instance.GetRing(cells[45], 5);
        for (int i = 0; i < results.Count; i++)
        {
            results[i].EnableHighlight(Color.red);
        }
    }
    public void OnRoadCostChanged() => HexMapMgr.Instance.Data.RoadCost = RoadCost;
    public void OnFlatCostChanged() => HexMapMgr.Instance.Data.FlatCost = FlatCost;
    public void OnSlopeCostChanged() => HexMapMgr.Instance.Data.SlopeCost = SlopeCost;
#endif
}
