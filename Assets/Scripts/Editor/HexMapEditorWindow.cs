using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine.SceneManagement;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using HexMap;
using System;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using System.Reflection;

namespace WorldMapEditor
{
    [Serializable]
    public partial class HexMapEditor : OdinEditorWindow
    {
        public enum State
        {
            None,
            Open,
            Create,
            ReCreate,
            Import,
            Export,
            Settings,
        }
        State state = State.Open;

        float rollSensitivity = 0.1f;
        public static HexMapEditor Instance;


        [HideInInspector,]
        bool changed;


        string MapName { get { return id + "_" + name; } }
        string MapDataName { get { return MapName + "_data.bytes"; } }

        string MapTerrainTypeTexturePath { get { return MaterialPath + "TerrainTypeTexture.png"; } }
        string MapTerrainTypeTextureApplicationPath { get { return Application.dataPath + MaterialApplicationPath + "TerrainTypeTexture.png"; } }

        string MapTerrainOpacityTexturePath { get { return MaterialPath + "TerrainOpacityTexture.png"; } }
        string MapDataPath { get { return MapPath + MapName + "/" + MapDataName; } }
        string MapJsonName { get { return MapName + ".json"; } }
        string MapJsonPath { get { return EditorPath + MapName + "/"; } }

        string MapScenePath { get { return MapPath + MapName + "/" + MapName + ".unity"; } }
        string MaterialPath { get { return MapPath + MapName + "/Material/"; } }
        string MaterialApplicationPath { get { return MapsPath + MapName + "/Material/"; } }

        public static readonly GUIContent newContent = new GUIContent(_new_ico, "new.");

        [MenuItem("Tools/HexMapEditor")]
        public static void ShowWindow()
        {
            GetWindow<HexMapEditor>("HexMap编辑器");
        }


        protected override void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(30);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            //if (GUILayout.Button(new_ico, GUILayout.Width(50), GUILayout.Height(30)))
            //{
            //    mode = EditorMode.Brush;//update
            //    UpdateMode();
            //}
            if (GUILayout.Button(EditorGUIUtility.IconContent("Terrain Icon", "Terrain."), GUILayout.Width(50), GUILayout.Height(30)))
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
            base.OnGUI();

        }

        private new void OnEnable()
        {
            if (Instance != null)
            {
                Instance.Close();
                Instance = null;
            }
            Instance = this;

            hasUnsavedChanges = false;
            autoRepaintOnSceneChange = true;
            saveChangesMessage = "Click \"Cancel\" and export data to Json. \nOr click \"Save\" or \"Discard\" to close Level Editor.";

            _new_ico = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.guanjinbiao.hexmap/Assets/Texture/Icons/Advanced.png", typeof(Texture2D));

            HexMapMgr.Instance.Data = new WorldMapData();

            EditorApplication.update += Update;
            SceneView.duringSceneGui += OnSceneGUI;
            EditorSceneManager.sceneClosing += (scene, removeing) =>
            {
                OnSceneChanged();
            };
            chunkCountX = 4;
            chunkCountZ = 3;

            previousCell = null;
            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.unity.images-library/Example/Images/image.png", typeof(Texture2D));
            _new_ico = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/compass.png");
            new_ico = new GUIContent(_new_ico, "地形");
        }



        private void OnDisable()
        {
            previousCell = null;
            previousVisionCell = null;
            HexMapMgr.Instance.UnloadMap();

            EditorApplication.update -= Update;

            autoRepaintOnSceneChange = false;
            hasUnsavedChanges = false;
            state = State.None;
        }

        private void OnSceneChanged()
        {
            if (!EditorApplication.isPlaying)
            {
                if (state == State.Import)
                {
                    if (File.Exists(MapTerrainTypeTextureApplicationPath))
                    {
                        Texture2D TerrainTypeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(MapTerrainTypeTexturePath);
                        HexMapMgr.Instance.TerrainMaterial.SetTexture("_TerrainTypeTexture", TerrainTypeTexture);
                    }
                    if (File.Exists(Application.dataPath + MaterialApplicationPath + "TerrainOpacityTexture.png"))
                    {
                        Texture2D TerrainOpacityTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(MaterialPath + "TerrainOpacityTexture.png");
                        HexMapMgr.Instance.TerrainMaterial.SetTexture("_TerrainOpacityTexture", TerrainOpacityTexture);
                    }
                }
            }
            state = State.Open;
        }
        private void OnSceneGUI(SceneView sceneView)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));//为scene响应添加默认事件,用来屏蔽以前的点击选中物体
            Event currentEvent = Event.current;

            switch (mode)
            {
                case EditorMode.Brush:
                    UpdateBrush();
                    if (HexMapMgr.Instance.TerrainMaterial != null)
                        HexMapMgr.Instance.TerrainMaterial?.DisableKeyword("GRID_ON");
                    break;

                case EditorMode.Feature:
                    UpdateBrush();
                    if (HexMapMgr.Instance.TerrainMaterial != null)
                        HexMapMgr.Instance.TerrainMaterial?.DisableKeyword("GRID_ON");
                    break;

                case EditorMode.Pathfinding:
                    if (HexMapMgr.Instance.TerrainMaterial != null)
                        HexMapMgr.Instance.TerrainMaterial?.EnableKeyword("GRID_ON");
                    UpdatePathfinding();
                    break;

                case EditorMode.FogOfWar:
                    if (HexMapMgr.Instance.TerrainMaterial != null)
                        HexMapMgr.Instance.TerrainMaterial?.EnableKeyword("GRID_ON");
                    UpdateFogOfWar();
                    break;
            }
            if (lastMode == mode)
                return;
            lastMode = mode;
            UpdateLabel();

            if (HexMapMgr.Instance.Root)
                EditorUtility.SetDirty(HexMapMgr.Instance.Root);
        }

        private void Update()
        {
            if (Instance == null)
                return;
            //OnMouse();        
        }

        private void OnMouse()
        {
            if (Mouse.current.middleButton.isPressed)
            {
                float x = Mouse.current.delta.ReadValue().x;
                float y = Mouse.current.delta.ReadValue().y;
                Vector3 position = Camera.main.transform.position;
                position = Vector3.Lerp(position, new Vector3(position.x - x, position.y, position.z - y), 0.1f);
                Camera.main.transform.position = position;
            }
            Vector2 scroll = Mouse.current.scroll.ReadValue();
            if (scroll != Vector2.zero)
            {
                float x = scroll.x;
                float y = scroll.y;
                Vector3 position = Camera.main.transform.position;
                position = Vector3.Lerp(position, new Vector3(position.x, position.y - y, position.z), 0.05f);
                Camera.main.transform.position = position;
            }
            if (Mouse.current.rightButton.isPressed)
            {
                var roll = Mouse.current.delta.ReadValue() * rollSensitivity;

                quaternion rotation = Camera.main.transform.rotation;

                quaternion rollY = quaternion.RotateY(math.radians(roll.x));
                quaternion rollX = quaternion.Euler(math.radians(new float3(-roll.y, 0, 0)));

                // 绕世界坐标系Y轴旋转
                rotation = math.mul(rotation, math.mul(math.mul(math.inverse(rotation), rollY), rotation));
                // 绕自身坐标系X轴旋转
                Camera.main.transform.rotation = math.mul(rotation, rollX);
            }
            Type gameViewType = GetGameViewType();
            EditorWindow gameViewWindow = GetGameViewWindow(gameViewType);

            if (gameViewWindow == null)
            {
                Debug.LogError("GameView is null!");
                return;
            }
            var defScaleField = gameViewType.GetField("m_defaultScale", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //whatever scale you want when you click on play
            float defaultScale = 1;
            var areaField = gameViewType.GetField("m_ZoomArea", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var areaObj = areaField.GetValue(gameViewWindow);
            var scaleField = areaObj.GetType().GetField("m_Scale", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            scaleField.SetValue(areaObj, new Vector2(defaultScale, defaultScale));
        }
        private static Type GetGameViewType()
        {
            Assembly unityEditorAssembly = typeof(EditorWindow).Assembly;
            Type gameViewType = unityEditorAssembly.GetType("UnityEditor.GameView");
            return gameViewType;
        }

        private static EditorWindow GetGameViewWindow(Type gameViewType)
        {
            UnityEngine.Object[] obj = Resources.FindObjectsOfTypeAll(gameViewType);
            if (obj.Length > 0)
            {
                return obj[0] as EditorWindow;
            }
            return null;
        }
    }
}