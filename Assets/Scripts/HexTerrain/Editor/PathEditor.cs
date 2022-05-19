using UnityEngine;
using UnityEngine.InputSystem;
using HexMap;
using System.Collections.Generic;
using UnityEditor;

public partial class HexTerrain
{
#if UNITY_EDITOR
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
        Event currentEvent = Event.current;
        if (currentEvent != null && (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) && currentEvent.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
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





    public void UpdatePath(HexCell fromCell, HexCell toCell)
    {
        InitDistance();
        ClearPath();

        currentPathFrom = fromCell;
        currentPathTo = toCell;
        List<HexCell> paths = HexMapMgr.Instance.FindPathCell(fromCell, toCell);
        currentPathExists = paths.Count > 0;
        //currentPathExists = FindPath(fromCell, toCell);
        ShowPath();
        ShowDistance();
    }



    void ClearPath()
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            if (current == null)
                return;
            while (current != null && current != currentPathFrom)
            {
                current.SetLabel(null);
                current.DisableHighlight();
                current = current.PathFrom;
            }
            if (current == null)
                return;
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
        for (int i = 0; i < HexMapMgr.Instance.cells.Count; i++)
        {
            HexMapMgr.Instance.cells[i].Distance = int.MaxValue;
        }
    }

    void ShowDistance()
    {
        if (HexMapMgr.Instance.cells == null)
            return;
        if (showDistance)
        {
            for (int i = 0; i < HexMapMgr.Instance.cells.Count; i++)
            {
                HexMapMgr.Instance.cells[i].SetDistanceLabel();
            }
        }
    }

#endif
}
