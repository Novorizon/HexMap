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
    
    [OnInspectorGUI("OnInspectorGUI", append: false)]
    private void OnInspectorGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(30);
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(EditorGUIUtility.IconContent("Terrain Icon"), GUILayout.Width(50), GUILayout.Height(30)))
        {
            mode = EditorMode.Brush;//update
            UpdateMode();
        }
        if (GUILayout.Button(EditorGUIUtility.IconContent("Prefab Icon"), GUILayout.Width(50), GUILayout.Height(30)))
        {
            mode = EditorMode.Feature;

            UpdateMode();
        }
        if (GUILayout.Button(EditorGUIUtility.IconContent("BuildSettings.Web.Small"), GUILayout.Width(50), GUILayout.Height(30)))
        {
            mode = EditorMode.Pathfinding;

            UpdateMode();
        }
        //if (GUILayout.Button(EditorGUIUtility.IconContent("ViewToolOrbit"), GUILayout.Width(50), GUILayout.Height(30)))
        //{
        //    mode = EditorMode.FogOfWar;

        //    UpdateMode();
        //}
        if (GUILayout.Button(EditorGUIUtility.IconContent("SettingsIcon"), GUILayout.Width(50), GUILayout.Height(30)))
        {
            mode = EditorMode.Settings;

            UpdateMode();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (changed)
        {
            EditorApplication.delayCall += () => { ReCreate(); };
            changed = false;
        }
    }
#endif
}
