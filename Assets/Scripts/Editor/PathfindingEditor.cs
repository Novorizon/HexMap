using UnityEngine;
using UnityEngine.InputSystem;
using HexMap;

namespace WorldMapEditor
{
    public partial class HexMapEditor
    {
        HexCellPriorityQueue searchFrontier;//寻路
        HexCell searchFromCell;//寻路
        HexCell searchToCell;//寻路

        int searchFrontierPhase;
        HexCell currentPathFrom, currentPathTo;
        bool currentPathExists;

        public void OnShowDistanceChanged()
        {
            ClearLabel();
            ShowDistance();
        }

        private void UpdatePathfinding()
        {
            if (Mouse.current.leftButton.isPressed)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 1000))
                {
                    HexCell currentCell = HexMapMgr.Instance.GetCell(hit.point);
                    if (currentCell != null)
                    {
                        if (Keyboard.current.shiftKey.ReadValue() > 0 && searchToCell != currentCell)
                        {
                            if (searchFromCell != currentCell)
                            {
                                if (searchFromCell != null)
                                {
                                    searchFromCell.DisableHighlight();
                                }
                                searchFromCell = currentCell;
                                searchFromCell.EnableHighlight(Color.blue);

                                if (searchToCell != null)
                                {
                                    UpdatePath(searchFromCell, searchToCell);
                                }

                            }
                        }
                        else if (searchFromCell != null && searchFromCell != currentCell)
                        {
                            if (searchToCell != currentCell)
                            {
                                searchToCell = currentCell;
                                UpdatePath(searchFromCell, searchToCell);
                            }
                        }
                    }
                    previousCell = currentCell;
                }
            }
        }


        public bool IsValidDestination(HexCell cell)
        {
            if (isExplorer)
                return cell.IsExplored && !cell.IsUnderwater;
            else
                return !cell.IsUnderwater;
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
                moveCost = 1;
            }
            else
            {
                moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
                //moveCost += toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
            }
            return moveCost;
        }



        public void UpdatePath(HexCell fromCell, HexCell toCell)
        {
            InitDistance();
            ClearPath();

            currentPathFrom = fromCell;
            currentPathTo = toCell;
            if (isTurnBased)
                currentPathExists = FindPathTurnBased(fromCell, toCell);
            else
                currentPathExists = FindPath(fromCell, toCell);
            ShowPath();
            ShowDistance();
        }


        bool FindPath(HexCell fromCell, HexCell toCell)
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
                        neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
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

        bool FindPathTurnBased(HexCell fromCell, HexCell toCell, int speed = 10000)
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

                int currentTurn = current.Distance / speed;

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
                    int turn = distance / speed;
                    if (turn > currentTurn)
                    {
                        distance = turn * speed + moveCost;
                    }

                    if (neighbor.SearchPhase < searchFrontierPhase)
                    {
                        neighbor.SearchPhase = searchFrontierPhase;
                        neighbor.Distance = distance;
                        neighbor.PathFrom = current;
                        neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
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

        void ClearPath()
        {
            if (currentPathExists)
            {
                HexCell current = currentPathTo;
                while (current != currentPathFrom)
                {
                    current.SetLabel(null);
                    current.DisableHighlight();
                    current = current.PathFrom;
                }
                current.DisableHighlight();
                currentPathExists = false;
            }
            else if (currentPathFrom != null)
            {
                currentPathFrom.DisableHighlight();
                currentPathTo.DisableHighlight();
            }
            currentPathFrom = currentPathTo = null;
        }

        void ShowPath()
        {
            if (currentPathExists)
            {
                HexCell current = currentPathTo;
                while (current != currentPathFrom)
                {
                    if (isTurnBased)
                    {
                        int turn = current.Distance / speed;
                        current.SetLabel(turn.ToString());
                    }
                    current.EnableHighlight(Color.white);
                    current = current.PathFrom;
                }
            }
            currentPathFrom.EnableHighlight(Color.blue);
            currentPathTo.EnableHighlight(Color.red);
        }

        //重置距离 仅用在编辑器，游戏中不用此步骤
        void InitDistance()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Distance = int.MaxValue;
            }
        }

        void ShowDistance()
        {
            if (showDistance)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    cells[i].SetDistanceLabel();
                }
            }
        }
    }
}