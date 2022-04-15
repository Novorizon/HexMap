//using HexMap;
//using System.Collections.Generic;
//using UnityEngine;

//[System.Serializable]
//public static class HexMapUtils
//{

//    public HexCell GetNeighbor(List<HexCell> Cells, HexCell cell,HexDirection direction)
//    {
//        if (cell.neighbors[(int)direction] < 0)
//            return null;
//        return Cells[cell.neighbors[(int)direction]];
//    }

//    public void SetNeighbor(List<HexCell> Cells, HexDirection direction, int i)
//    {
//        neighbors[(int)direction] = i;
//        Cells[i].neighbors[(int)direction.Opposite()] = id;
//    }

//    public HexEdgeType GetEdgeType(List<HexCell> Cells, HexDirection direction)
//    {
//        return HexMetrics.GetEdgeType(elevation, Cells[neighbors[(int)direction]].elevation);
//    }

//    void SetRoad(List<HexCell> Cells, int index, bool state)
//    {
//        roads[index] = state;
//        HexCell neighbor = Cells[neighbors[index]];
//        neighbor.roads[(int)((HexDirection)index).Opposite()] = state;
//        neighbor.RefreshSelfOnly();
//        RefreshSelfOnly();
//    }
//}