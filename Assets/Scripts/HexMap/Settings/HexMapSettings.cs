using HexMap;
using Sirenix.OdinInspector;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HexMap
{
    [CreateAssetMenu(fileName = "HexMapSettings", menuName = "HexMapSettings", order = 1000)]
    public class HexMapSettings : ScriptableObject
    {
        public HexMapSetting Settings;
    }

    [Serializable]
    public class HexMapSetting
    {
        [LabelText("编辑器场景文件")]
        public string ScenePath = "Assets/Plugins/HexMap/Editor/Scenes/";


        [LabelText("原始数据目录"), InfoBox("用于存储原始的二进制地图数据，可导入编辑器"), PropertySpace(SpaceBefore = 10)]
        public string MapOriginalDataPath = "Assets/Main/Maps/";

        [LabelText("地图数据目录"), InfoBox("用于游戏运行时的二进制地图数据，不可导入编辑器"), PropertySpace(SpaceBefore = 10)]
        public string MapDataPath = "Assets/Main/Maps/";

        //[LabelText("地图目录"), InfoBox("用于美术使用的Mesh，不可导入编辑器"), PropertySpace(SpaceBefore = 10)]
        //public string MapPath = "/Main/Maps/";

        [LabelText("地图块文件"), PropertySpace(SpaceBefore = 10)]
        public HexChunk chunk;

        [LabelText("地图材质文件"), PropertySpace(SpaceBefore = 10)]
        public Material TerrainMaterial;

        [LabelText("单元格标签文件"), PropertySpace(SpaceBefore = 10)]
        public Text CellLabelPrefab;

        //[LabelText("噪点文件"), PropertySpace(SpaceBefore = 10)]
        //public Texture2D Noise;

        [LabelText("网格文件"), PropertySpace(SpaceBefore = 10)]
        public Texture2D GridTex;
    }

}
