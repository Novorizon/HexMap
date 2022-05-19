using HexMap;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

public partial class HexTerrain
{

#if UNITY_EDITOR
    private void OnSceneGUI(SceneView sceneView)
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));//为scene响应添加默认事件,用来屏蔽以前的点击选中物体
        Event currentEvent = Event.current;

        switch (mode)
        {
            case EditorMode.Brush:
                UpdateBrush();
                if (lastMode != mode && HexMapMgr.Instance.TerrainMaterial != null)
                    HexMapMgr.Instance.TerrainMaterial?.DisableKeyword("GRID_ON");
                break;

            case EditorMode.Feature:
                UpdateBrush();
                if (lastMode != mode && HexMapMgr.Instance.TerrainMaterial != null)
                    HexMapMgr.Instance.TerrainMaterial?.DisableKeyword("GRID_ON");
                break;

            case EditorMode.Pathfinding:
                UpdatePathfinding();
                if (lastMode != mode && HexMapMgr.Instance.TerrainMaterial != null)
                    HexMapMgr.Instance.TerrainMaterial?.EnableKeyword("GRID_ON");
                break;

            case EditorMode.FogOfWar:
                //UpdateFogOfWar();
                if (lastMode != mode && HexMapMgr.Instance.TerrainMaterial != null)
                    HexMapMgr.Instance.TerrainMaterial?.EnableKeyword("GRID_ON");
                break;
        }
        if (lastMode == mode)
            return;
        lastMode = mode;
        UpdateLabel();

        if (HexMapMgr.Instance.Root)
            EditorUtility.SetDirty(HexMapMgr.Instance.Root);
    }



    private void SelectionChanged()
    {
    }
#endif
}
