using HexMap;
using System.Collections.Generic;
using UnityEngine;


public partial class HexTerrain
{
#if UNITY_EDITOR
    EditorMode lastMode = EditorMode.Brush;


    public void UpdateLabel()
    {
        List<HexCell> cells = HexMapMgr.Instance.cells;
        if (cells == null)
            return;
        if (lastMode == mode)
            return;
        switch (mode)
        {
            case EditorMode.Brush:

                ClearLabel();
                DisableHighlight();
                break;

            case EditorMode.Feature:
                SetCoordinateLabel();
                break;
        }

        lastMode = mode;

    }


    void EnableHighlight()
    {
        List<HexCell> cells = HexMapMgr.Instance.cells;
        if (cells == null)
            return;
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].EnableHighlight(Color.white);
        }
    }

    void DisableHighlight()
    {
        List<HexCell> cells = HexMapMgr.Instance.cells;
        if (cells == null)
            return;
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].DisableHighlight();
        }
    }

    void SetCoordinateLabel()
    {
        List<HexCell> cells = HexMapMgr.Instance.cells;
        if (cells == null)
            return;
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].EnableHighlight(Color.white);
            cells[i].SetCoordinateLabel();
        }
    }

    void SetXZLabel()
    {
        List<HexCell> cells = HexMapMgr.Instance.cells;
        if (cells == null)
            return;
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].EnableHighlight(Color.white);
            cells[i].SetXZLabel();
        }
    }


    void SetIdLabel()
    {
        List<HexCell> cells = HexMapMgr.Instance.cells;
        if (cells == null)
            return;
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].EnableHighlight(Color.white);
            cells[i].SetIdLabel();
        }
    }

    void ClearLabel()
    {
        List<HexCell> cells = HexMapMgr.Instance.cells;
        if (cells == null)
            return;
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].ClearLabel();
        }
    }
#endif
}
