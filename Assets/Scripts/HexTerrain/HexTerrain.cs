using HexMap;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

[ExecuteInEditMode]
public partial class HexTerrain : MonoBehaviour
{
#if UNITY_EDITOR
    static public HexTerrain Instance;
    bool changed;

    private void Update()
    {
    }
#endif
    private void OnEnable()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            HexTerrainData terrain = GetComponent<HexTerrainData>();
            HexMapMgr.Instance.Load(terrain);

        }
        else
        {
            terrain = 0;
            Instance = this;
            name = MapName;
            SceneView.duringSceneGui += OnSceneGUI;
            Selection.selectionChanged += SelectionChanged;

            cellCount = cellCountX * cellCountZ;
            cellMetreX = cellCountX * radius * HexMetrics.outerToInner * 2;
            cellMetreZ = 1.5f * cellCountZ * radius + 0.5f * radius;
            HexMetrics.Radius = radius;
            Load();

        }
#else
            HexTerrainData terrain =GetComponent<HexTerrainData>();
            HexMapMgr.Instance.Load(terrain);
#endif
    }

#if UNITY_EDITOR
    private void OnDisable()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            return;
        CreateTexture();
        Instance = null;
        if (HexMapMgr.Instance.TerrainMaterial)
            HexMapMgr.Instance.TerrainMaterial.SetFloat("_EDITOR", 0);
    }



    [MenuItem("GameObject/HexTerrain", false, 10)]
    static void CreateHexTerrain(MenuCommand menuCommand)
    {
        Create(menuCommand);
    }

    private void OnDestroy()
    {
        Instance = null;
        UType = UpdateType.None;
        BrushType = BrushType.None;
    }

#endif
}
