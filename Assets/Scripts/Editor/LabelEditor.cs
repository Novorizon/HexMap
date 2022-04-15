using UnityEngine;

namespace WorldMapEditor
{
    public partial class HexMapEditor
    {
        EditorMode lastMode = EditorMode.Brush;


        public void UpdateLabel()
        {
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

                case EditorMode.Pathfinding:
                    ClearLabel();
                    DisableHighlight();
                    if (!showDistance)
                        break;

                    break;
            }

            lastMode = mode;

        }


        void EnableHighlight()
        {
            if (cells == null)
                return;
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].EnableHighlight(Color.white);
            }
        }

        void DisableHighlight()
        {
            if (cells == null)
                return;
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].DisableHighlight();
            }
        }

        void SetCoordinateLabel()
        {
            if (cells == null)
                return;
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].EnableHighlight(Color.white);
                cells[i].SetCoordinateLabel();
            }
        }

        void ClearLabel()
        {
            if (cells == null)
                return;
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].ClearLabel();
            }
        }
    }
}