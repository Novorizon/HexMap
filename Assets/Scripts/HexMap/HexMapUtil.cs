//using System.Collections.Generic;


//namespace HexMap
//{
//    public partial class HexMapMgr
//    {
//        public HexCell GetNeighbor(HexCell cell, HexDirection direction)
//        {
//            if (cell.Neighbors[(int)direction] < 0)
//                return null;
//            return cells[cell.Neighbors[(int)direction]];
//        }
//        public HexCell GetNeighbor(HexCell cell, int i)
//        {
//            return cells[cell.Neighbors[i]];
//        }

//        public void SetNeighbor(HexCell cell, HexDirection direction, int id)
//        {
//            if (cell.Neighbors[(int)direction] < 0)
//                return ;
//            cell.Neighbors[(int)direction] = id;
//        }

//        public HexEdgeType GetEdgeType(HexCell cell, HexDirection direction)
//        {
//            if (cell.Neighbors[(int)direction] < 0)
//                return  HexEdgeType.Flat;
//            HexCell neighbor = cells[cell.Neighbors[(int)direction]];
//            return cell.GetEdgeType(neighbor);
//        }


//        public void RemoveIncomingRiver(HexCell cell)
//        {
//            if (!cell.HasIncomingRiver)
//            {
//                return;
//            }
//            cell.HasIncomingRiver = false;
//            cell.RefreshSelfOnly();

//            HexCell neighbor = cells[(int)cell.IncomingRiver];
//            neighbor.HasOutgoingRiver = false;
//            neighbor.RefreshSelfOnly();
//        }

//        public void RemoveOutgoingRiver(HexCell cell)
//        {
//            if (!cell.HasOutgoingRiver)
//            {
//                return;
//            }
//            cell.HasOutgoingRiver = false;
//            cell.RefreshSelfOnly();

//            HexCell neighbor = cells[(int)cell.OutgoingRiver];
//            neighbor.HasIncomingRiver = false;
//            neighbor.RefreshSelfOnly();
//        }

//        public void RemoveRiver(HexCell cell)
//        {
//            RemoveOutgoingRiver(cell);
//            RemoveIncomingRiver(cell);
//        }

//        public void ValidateRivers(HexCell cell)
//        {
//            if (cell.Neighbors[(int)cell.OutgoingRiver] == -1)
//                return;
//            HexCell neighbor = cells[cell.Neighbors[(int)cell.OutgoingRiver]];
//            if (cell.HasOutgoingRiver && !cell.IsValidRiverDestination(neighbor))
//            {
//                RemoveOutgoingRiver(cell);
//            }
//            neighbor = cells[cell.Neighbors[(int)cell.IncomingRiver]];
//            if (cell.HasIncomingRiver && !neighbor.IsValidRiverDestination(cell))
//            {
//                RemoveIncomingRiver(cell);
//            }
//        }


//        public void SetOutgoingRiver(HexCell cell, HexDirection direction)
//        {
//            if (cell.HasOutgoingRiver && cell.OutgoingRiver == direction)
//            {
//                return;
//            }

//            HexCell neighbor = cells[cell.Neighbors[(int)direction]];
//            if (!cell.IsValidRiverDestination(neighbor))
//            {
//                return;
//            }

//            RemoveOutgoingRiver(cell);
//            if (cell.HasIncomingRiver && cell.IncomingRiver == direction)
//            {
//                RemoveIncomingRiver(cell);
//            }
//            cell.HasOutgoingRiver = true;
//            cell.OutgoingRiver = direction;
//            cell.SpecialIndex = 0;


//            RemoveIncomingRiver(neighbor);
//            neighbor.HasIncomingRiver = true;
//            neighbor.IncomingRiver = direction.Opposite();
//            neighbor.SpecialIndex = 0;


//            SetRoad(cell, (int)direction, false);
//        }

//        public void AddRoad(HexCell cell, HexDirection direction)
//        {
//            HexCell neighbor = cells[cell.Neighbors[(int)direction]];

//            if (!cell.roads[(int)direction]
//                && !cell.HasRiverThroughEdge(direction)
//                && !cell.IsSpecial
//                && !neighbor.IsSpecial
//                && GetElevationDifference(cell, direction) <= 1)
//            {
//                SetRoad(cell, (int)direction, true);
//            }
//        }

//        public int GetElevationDifference(HexCell cell, HexDirection direction)
//        {
//            HexCell neighbor = cells[cell.Neighbors[(int)direction]];
//            int difference = cell.Elevation - neighbor.Elevation;
//            return difference >= 0 ? difference : -difference;
//        }

//        public void SetRoad(HexCell cell, int index, bool state)
//        {
//            HexCell neighbor = cells[cell.Neighbors[index]];
//            cell.roads[index] = state;
//            neighbor.roads[(int)((HexDirection)index).Opposite()] = state;
//            neighbor.RefreshSelfOnly();
//            cell.RefreshSelfOnly();
//        }

//        public void RemoveRoads(HexCell cell)
//        {
//            for (int i = 0; i < cell.Neighbors.Length; i++)
//            {
//                if (cell.roads[i])
//                {
//                    SetRoad(cell, i, false);
//                }
//            }
//        }

//        public void Refresh(HexCell cell)
//        {
//            if (cell.ChunkMgr != null)
//            {
//                cell.ChunkMgr.Refresh();
//                for (int i = 0; i < cell.Neighbors.Length; i++)
//                {
//                    HexCell neighbor = cells[cell.Neighbors[i]];
//                    if (neighbor != null && neighbor.ChunkMgr != cell.ChunkMgr)
//                    {
//                        neighbor.ChunkMgr.Refresh();
//                    }
//                }
//            }
//        }
//    }
//}