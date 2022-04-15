using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using HexMap;
using UnityEditor;
using System;
using System.IO;
using Newtonsoft.Json;

namespace WorldMapEditor
{
    public enum PropertySettings
    {
        Property,
        Texture2D,
        Chunk,
        Grid,
        Button,

    }

    public partial class HexMapEditor
    {
        AssetJson AssetJson;


        [Title("地图属性")]
        [LabelText("地图id"), MinValue(0), PropertyOrder((int)PropertySettings.Property), PropertySpace(SpaceBefore = 10), LabelWidth(50), VerticalGroup("ID"), ShowIf("IsSettings")]
        public int id;
        [LabelText("地图名称"), PropertyOrder((int)PropertySettings.Property), PropertySpace(SpaceBefore = 3), LabelWidth(50), VerticalGroup("ID"), ShowIf("IsSettings")]
        public new string name;
        [LabelText("版本号"), PropertyOrder((int)PropertySettings.Property), PropertySpace(SpaceBefore = 3), LabelWidth(50), VerticalGroup("ID"), ShowIf("IsSettings")]
        public int version = 0;

        [LabelText("噪点图"), PropertyOrder((int)PropertySettings.Property), PropertySpace(SpaceBefore = 10), OnValueChanged("OnNoiseChanged"), LabelWidth(50), VerticalGroup("ID"), ShowIf("IsSettings")]
        public Texture2D Noise;

        [InlineButton("CreateTextures", "创建纹理数组")]
        [LabelText("纹理数组"), LabelWidth(50), PropertySpace(SpaceBefore = 30), PropertyOrder((int)PropertySettings.Texture2D), HorizontalGroup("Texture2D"), ShowIf("IsSettings")]
        public List<Texture2D> textures;

        bool hasTextureArray = false;


        [LabelText("战争迷雾"), ReadOnly, ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertySettings.Property), LabelWidth(50), HorizontalGroup("FOW ", 10), ShowIf("IsSettings")]
        public bool isFOW = false;
        [LabelText("战争黑雾"), ReadOnly, ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertySettings.Property), LabelWidth(50), HorizontalGroup("FOW ", 10), ShowIf("IsSettings")]
        public bool isExplorer = false;
        [LabelText("视野阻挡"), ReadOnly, ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertySettings.Property), LabelWidth(50), HorizontalGroup("FOW ", 10), OnValueChanged("OnVisionBlockChanged"), ShowIf("IsSettings")]
        public bool IsVisionBlock = false;



        [Title("地图块")]
        [LabelText("宽"), PropertySpace(SpaceBefore = 30), PropertyOrder((int)PropertySettings.Chunk), LabelWidth(30), ProgressBar(0, 10), OnValueChanged("OnValueChanged"), ShowIf("IsSettings")]
        public int chunkCountX;
        [LabelText("高"), PropertySpace(SpaceBefore = 3), PropertyOrder((int)PropertySettings.Chunk), LabelWidth(30), ProgressBar(0, 10), OnValueChanged("OnValueChanged"), ShowIf("IsSettings")]
        public int chunkCountZ;


        [LabelText("单元格数量"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 10), LabelWidth(60), ReadOnly, HorizontalGroup("Cell "), ShowIf("IsSettings")]
        public int cellCount;
        [LabelText("宽(x)"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 10), LabelWidth(30), ReadOnly, HorizontalGroup("Cell "), ShowIf("IsSettings")]
        public int cellCountX;
        [LabelText("高(z)"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 10), LabelWidth(30), ReadOnly, HorizontalGroup("Cell "), ShowIf("IsSettings")]
        public int cellCountZ;


        [LabelText("随机数种子"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 30), LabelWidth(60), HorizontalGroup("Random"), OnValueChanged("OnValueChanged"), ShowIf("IsSettings")]
        [InlineButton("Random")]
        public int seed;


        public void Random()
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
            seed ^= (int)System.DateTime.Now.Ticks;
            seed ^= (int)Time.unscaledTime;
            seed &= int.MaxValue;

            changed = true;
            UType = UpdateType.ReCreate;
        }


        [Button("创建地形", ButtonSizes.Medium), PropertySpace(SpaceBefore = 40), PropertyOrder((int)Property.Button), HorizontalGroup("DataGroup"), ShowIf("IsSettings")]
        partial void Create();



        [Button("导入场景", ButtonSizes.Medium), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Button), HorizontalGroup("Load"), ShowIf("IsSettings")]
        partial void Import();


        //[Button("导入场景", ButtonSizes.Medium), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Button), HorizontalGroup("Load"), ShowIf("IsSettings")]
        //partial void ImportScene();

        [Button("导出场景", ButtonSizes.Medium), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Button), HorizontalGroup("Export"), ShowIf("IsSettings")]
        partial void Export();

        //[Button("导出场景", ButtonSizes.Medium), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Button), HorizontalGroup("Export"), ShowIf("IsSettings")]
        //partial void ExportScene();


        public void CreateTextures()
        {
            if (name == "")
            {
                EditorUtility.DisplayDialog("Error", "缺少地图名称", "Yes");
                return;
            }
            if (textures.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "缺少纹理", "Yes");
                return;
            }
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i] == null)
                {
                    EditorUtility.DisplayDialog("Error", "纹理数组空缺", "Yes");
                    return;
                }
            }

            string path = Application.dataPath + MapsPath + MapName;
            if (Directory.Exists(path))
            {
                if (EditorUtility.DisplayDialog("Error", "文件目录已存在，是否覆盖？", "是", "否"))
                {
                    DeleteFiles(path);
                    Directory.CreateDirectory(path);
                }
                else
                {
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(path);
            }
            if (!Directory.Exists(MaterialPath))
            {
                Directory.CreateDirectory(MaterialPath);
            }
            if (!Directory.Exists(MapJsonPath))
            {
                DirectoryInfo info = Directory.CreateDirectory(MapJsonPath);
                info.Attributes = FileAttributes.Normal & FileAttributes.Directory;
            }

            AssetJson = new AssetJson();
            AssetJson.textures = new string[textures.Count];

            TerrainList.Clear();
            Texture2D t = textures[0];
            Texture2DArray textureArray = new Texture2DArray(t.width, t.height, textures.Count, t.format, t.mipmapCount > 1);
            textureArray.anisoLevel = t.anisoLevel;
            textureArray.filterMode = t.filterMode;
            textureArray.wrapMode = t.wrapMode;

            for (int i = 0; i < textures.Count; i++)
            {
                for (int m = 0; m < t.mipmapCount; m++)
                {
                    try
                    {
                        Graphics.CopyTexture(textures[i], 0, m, textureArray, i, m);

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }

                }
                AssetJson.textures[i] = AssetDatabase.GetAssetPath(textures[i]);
                TerrainList.Add(new TerrainType() { use = false, name = textures[i].name, id = i });
            }

            AssetDatabase.CreateAsset(textureArray, MaterialPath + "TerrainTextureArray.asset");
            AssetDatabase.Refresh();
            //材质
            Shader shader = Shader.Find("Custom/Lit/Terrain");
            Material mat = new Material(shader);
            mat.SetTexture("_MainTex", textureArray);
            mat.SetTexture("_GridTex", HexMapMgr.Instance.GridTex);
            mat.enableInstancing = true;
            mat.SetColor("_Specular", new Color(60 / 255f, 60 / 255f, 60 / 255f));
            mat.SetFloat("_Smoothness", 0.5f);
            AssetDatabase.CreateAsset(mat, MaterialPath + "Terrain.mat");
            AssetDatabase.Refresh();

            HexMapMgr.Instance.SetMaterial(mat);
            hasTextureArray = true;

            //ISerializer serializer = new SerializerBuilder().Build();
            //string yaml = serializer.Serialize(Arrays);


        }

        [LabelText("地图id"), PropertyOrder((int)Property.CellList), Searchable, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true), ShowIf("IsSettings")]
        List<HexCell> cells;


        public void OnNoiseChanged()
        {
            HexMetrics.noiseSource = Noise;
            //更新所有与扰动相关的内容
        }

        public bool DeleteFiles(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles();
            try
            {
                foreach (var item in files)
                {
                    File.Delete(item.FullName);
                }
                if (dir.GetDirectories().Length != 0)
                {
                    foreach (var item in dir.GetDirectories())
                    {
                        if (!item.ToString().Contains("$") && (!item.ToString().Contains("Boot")))
                        {
                            DeleteFiles(item.ToString());
                        }
                    }
                }
                Directory.Delete(path);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}