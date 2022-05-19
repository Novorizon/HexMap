using System.Collections.Generic;
using UnityEngine;

namespace HexMap
{
    public partial class HexMapMgr
    {
        HexCellPriorityQueue searchFrontier;//寻路
        int searchFrontierPhase;




        public bool IsValidDestination(HexCell cell)
        {
            return !cell.IsUnderwater;
            //if (isExplorer)
            //    return cell.IsExplored && !cell.IsUnderwater;
            //else
            //    return !cell.IsUnderwater;
        }

        public int GetMoveCost(HexCell fromCell, HexCell toCell, HexDirection direction)
        {
            if (!IsValidDestination(toCell))
            {
                return -1;
            }
            HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
            if (edgeType == HexEdgeType.Cliff)
            {
                return -1;
            }
            int moveCost;
            if (fromCell.HasRoadThroughEdge(direction))
            {
                moveCost = data.RoadCost;
            }
            else
            {
                moveCost = edgeType == HexEdgeType.Flat ? data.FlatCost : data.SlopeCost;
                moveCost += toCell.terrainCost + toCell.featureCost;
            }
            return moveCost;
        }


        public List<Vector3> FindPath(HexCell fromCell, HexCell toCell)
        {
            List<Vector3> paths = new List<Vector3>();
            bool currentPathExists = Search(fromCell, toCell);
            if (currentPathExists)
            {
                HexCell current = toCell;
                paths.Add(current.Position);
                while (current != fromCell)
                {
                    current = current.PathFrom;
                    paths.Add(current.Position);
                }
            }
            if (paths.Count > 0)
                paths.Reverse();
            return paths;
        }
        public List<HexCell> FindPathCell(HexCell fromCell, HexCell toCell)
        {
            List<HexCell> paths = new List<HexCell>();
            bool currentPathExists = Search(fromCell, toCell);
            if (currentPathExists)
            {
                HexCell current = toCell;
                paths.Add(current);
                while (current != fromCell)
                {
                    current = current.PathFrom;
                    paths.Add(current);
                }
            }
            if (paths.Count > 0)
                paths.Reverse();
            return paths;
        }

        bool Search(HexCell fromCell, HexCell toCell)
        {
            searchFrontierPhase += 2;
            if (searchFrontier == null)
            {
                searchFrontier = new HexCellPriorityQueue();
            }
            else
            {
                searchFrontier.Clear();
            }

            fromCell.SearchPhase = searchFrontierPhase;
            fromCell.Distance = 0;
            searchFrontier.Enqueue(fromCell);
            while (searchFrontier.Count > 0)
            {
                HexCell current = searchFrontier.Dequeue();
                current.SearchPhase += 1;

                if (current == toCell)
                {
                    return true;
                }

                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    HexCell neighbor = current.GetNeighbor(d);
                    if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
                    {
                        continue;
                    }

                    if (!IsValidDestination(neighbor))
                    {
                        continue;
                    }
                    int moveCost = GetMoveCost(current, neighbor, d);
                    if (moveCost < 0)
                    {
                        continue;
                    }

                    int distance = current.Distance + moveCost;

                    if (neighbor.SearchPhase < searchFrontierPhase)
                    {
                        neighbor.SearchPhase = searchFrontierPhase;
                        neighbor.Distance = distance;
                        neighbor.PathFrom = current;
                        //neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
                        neighbor.SearchHeuristic = neighbor.hexagon.DistanceTo(toCell.hexagon);
                        searchFrontier.Enqueue(neighbor);
                    }
                    else if (distance < neighbor.Distance)
                    {
                        int oldPriority = neighbor.SearchPriority;
                        neighbor.Distance = distance;
                        neighbor.PathFrom = current;
                        searchFrontier.Change(neighbor, oldPriority);
                    }
                }
            }
            return false;
        }

    }
}