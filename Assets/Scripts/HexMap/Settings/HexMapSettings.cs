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
        [LabelText("�༭�������ļ�")]
        public string ScenePath = "Assets/Plugins/HexMap/Editor/Scenes/";


        [LabelText("ԭʼ����Ŀ¼"), InfoBox("���ڴ洢ԭʼ�Ķ����Ƶ�ͼ���ݣ��ɵ���༭��"), PropertySpace(SpaceBefore = 10)]
        public string MapOriginalDataPath = "Assets/Main/Maps/";

        [LabelText("��ͼ����Ŀ¼"), InfoBox("������Ϸ����ʱ�Ķ����Ƶ�ͼ���ݣ����ɵ���༭��"), PropertySpace(SpaceBefore = 10)]
        public string MapDataPath = "Assets/Main/Maps/";

        //[LabelText("��ͼĿ¼"), InfoBox("��������ʹ�õ�Mesh�����ɵ���༭��"), PropertySpace(SpaceBefore = 10)]
        //public string MapPath = "/Main/Maps/";

        [LabelText("��ͼ���ļ�"), PropertySpace(SpaceBefore = 10)]
        public HexChunk chunk;

        [LabelText("��ͼ�����ļ�"), PropertySpace(SpaceBefore = 10)]
        public Material TerrainMaterial;

        [LabelText("��Ԫ���ǩ�ļ�"), PropertySpace(SpaceBefore = 10)]
        public Text CellLabelPrefab;

        //[LabelText("����ļ�"), PropertySpace(SpaceBefore = 10)]
        //public Texture2D Noise;

        [LabelText("�����ļ�"), PropertySpace(SpaceBefore = 10)]
        public Texture2D GridTex;
    }

}
