using UnityEngine;
using UnityEngine.InputSystem;
using HexMap;
using System.Collections.Generic;

namespace WorldMapEditor
{
    public partial class HexMapEditor
    {
        HexCell previousVisionCell;
        int lastVision;

        public void OnVisionChanged()
        {
            if (previousVisionCell == null)
                return;
            DecreaseVisibility(previousVisionCell, lastVision);
            IncreaseVisibility(previousVisionCell, Vision);
            lastVision = Vision;
        }

        public void OnVisionBlockChanged()
        {
            ResetVisibility();
            previousVisionCell = null;
        }

        private void UpdateFogOfWar()
        {
            if (Keyboard.current.altKey.ReadValue() > 0 && Mouse.current.leftButton.isPressed)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 1000))
                {
                    HexCell currentCell = HexMapMgr.Instance.GetCell(hit.point);
                    if (currentCell != null)
                    {
                        if (previousVisionCell != null)
                            DecreaseVisibility(previousVisionCell, lastVision);
                        IncreaseVisibility(currentCell, Vision);

                        previousVisionCell = currentCell;
                        lastVision = Vision;
                    }
                }
            }
        }


        public void IncreaseVisibility(HexCell fromCell, int range)
        {
            List<HexCell> cells = GetVisibleCells(fromCell, range);
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].IncreaseVisibility();
            }
            ListPool<HexCell>.Add(cells);
        }

        public void DecreaseVisibility(HexCell fromCell, int range)
        {
            List<HexCell> cells = GetVisibleCells(fromCell, range);
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].DecreaseVisibility();
            }
            ListPool<HexCell>.Add(cells);
        }

        List<HexCell> GetVisibleCells(HexCell fromCell, int range)
        {
            List<HexCell> visibleCells = ListPool<HexCell>.Get();

            searchFrontierPhase += 2;
            if (searchFrontier == null)
            {
                searchFrontier = new HexCellPriorityQueue();
            }
            else
            {
                searchFrontier.Clear();
            }

            //站得高看得远
            if (IsVisionBlock)
            {
                range += fromCell.ViewElevation;
            }

            fromCell.SearchPhase = searchFrontierPhase;
            fromCell.Distance = 0;
            searchFrontier.Enqueue(fromCell);

            HexCoordinates fromCoordinates = fromCell.coordinates;

            while (searchFrontier.Count > 0)
            {
                HexCell current = searchFrontier.Dequeue();
                current.SearchPhase += 1;
                visibleCells.Add(current);

                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    HexCell neighbor = current.GetNeighbor(d);
                    if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase || !neighbor.Explorable)
                    {
                        continue;
                    }

                    int distance = current.Distance + 1;
                    if (IsVisionBlock)
                    {
                        if (distance + neighbor.ViewElevation > range || distance > fromCoordinates.DistanceTo(neighbor.coordinates))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (distance > range)
                        {
                            continue;
                        }
                    }

                    if (neighbor.SearchPhase < searchFrontierPhase)
                    {
                        neighbor.SearchPhase = searchFrontierPhase;
                        neighbor.Distance = distance;
                        neighbor.SearchHeuristic = 0;
                        searchFrontier.Enqueue(neighbor);
                    }
                    else if (distance < neighbor.Distance)
                    {
                        int oldPriority = neighbor.SearchPriority;
                        neighbor.Distance = distance;
                        searchFrontier.Change(neighbor, oldPriority);
                    }
                }
            }
            return visibleCells;
        }

        public void ResetVisibility()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].ResetVisibility();
            }
        }

    }
}