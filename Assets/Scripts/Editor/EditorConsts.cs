using HexMap;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WorldMapEditor
{
    public partial class HexMapEditor
    {

        public static readonly string HexMapSettingsPath = "Assets/Plugins/HexMap/Settings/HexMapSettings.asset";

        public static readonly string HexMapFeaturePath = "Assets/Plugins/HexMap/Settings/HexMapFeature.asset";
        public static readonly string HexMapAssetsPath = "Assets/Plugins/HexMap/Settings/HexMapAssets.asset";

        public static string ScenePath = "Assets/Plugins/HexMap/Editor/Scenes/";
        public static string MapPath = "Assets/Main/Maps/";
        public static string MapsPath = "/Main/Maps/";
        public static string EditorPath = "Assets/Editor/HexMap/Maps/";
        public static HexChunk HexChunk;
        public static Text CellLabelPrefab;
        //public static Texture2D Noise;
        public static Texture2D GridTex;


        public static Material TerrainMaterial;
        static void LoadSettings()
        {

            HexMapSettings assets = AssetDatabase.LoadAssetAtPath<HexMapSettings>(HexMapSettingsPath);

            if (assets == null)
            {
                Debug.LogError("资源已经不存在");
                return;
            }
            ScenePath = assets.Settings.ScenePath;
            MapPath = assets.Settings.MapDataPath;
            MapsPath = MapPath.Substring(6, MapPath.Length - 6);// assets.Settings.MapMeshPath;
            HexChunk = assets.Settings.chunk;
            TerrainMaterial = assets.Settings.TerrainMaterial;
            CellLabelPrefab = assets.Settings.CellLabelPrefab;
            //Noise = assets.Settings.Noise;
            GridTex = assets.Settings.GridTex;


        }
    }
}
