using HexMap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WorldMapEditor
{

    public partial class HexMapEditor
    {

        void ReCreate() => Create();

        partial void Create()
        {
            if (!hasTextureArray)
            {
                EditorUtility.DisplayDialog("Error", "缺少纹理数组", "Yes");
                return;
            }


            WorldMapData data = new WorldMapData();
            data.version = version;
            data.id = id;
            data.seed = seed;
            data.chunkCountX = chunkCountX;
            data.chunkCountZ = chunkCountZ;

            HexMapMgr.Instance.UnloadMap();
            HexMapMgr.Instance.Create(data);
            cells = HexMapMgr.Instance.cells;

            //SetLabel();
            InitBrush();
            OnRiverMatChanged();
            OnRoadMatChanged();
            OnWaterMatChanged();
            OnWaterShoreMatChanged();
            OnNoiseChanged();
            state = State.Create;
        }


        partial void Export()
        {
            string path = Application.dataPath + MapsPath + MapName;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }



            cells = HexMapMgr.Instance.cells;

            List<int> features = new List<int>();
            for (int i = 0; i < cells.Count; i++)
            {
                for (int j = 0; j < cells[i].Features.Count; j++)
                {
                    int id = cells[i].Features[j] & HexMetrics.FeatureClearMask;
                    if (features.Contains(id))
                        continue;
                    features.Add(id);
                }
            }

            WorldMapData data = HexMapMgr.Instance.Data;
            using (BinaryWriter writer = new BinaryWriter(File.Open(MapDataPath, FileMode.Create)))
            {
                writer.Write(version);
                writer.Write(data.id);
                writer.Write(data.seed);
                writer.Write((byte)data.chunkCountX);
                writer.Write((byte)data.chunkCountZ);
                for (int i = 0; i < cells.Count; i++)
                {
                    cells[i].Export(writer);
                }
            }

            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(scene, MapScenePath, true);

            AssetJson.Noise = AssetDatabase.GetAssetPath(Noise);
            AssetJson.RiverMat = AssetDatabase.GetAssetPath(RiverMat);
            AssetJson.RoadMat = AssetDatabase.GetAssetPath(RoadMat);
            AssetJson.WaterMat = AssetDatabase.GetAssetPath(WaterMat);
            AssetJson.WaterShoreMat = AssetDatabase.GetAssetPath(WaterShoreMat);
            AssetJson.Bridge = AssetDatabase.GetAssetPath(Bridge);

            AssetJson.features = new List<string>(FeatureList.Count);
            for (int i = 0; i < FeatureList.Count; i++)
            {
                AssetJson.features.Add(AssetDatabase.GetAssetPath(FeatureList[i].Feature));
            }

            AssetJson.specials = new List<string>(SpecialList.Count);
            for (int i = 0; i < SpecialList.Count; i++)
            {
                AssetJson.specials.Add(AssetDatabase.GetAssetPath(SpecialList[i].Feature));
            }

            AssetJson.terrains = new List<int>(cells.Count);
            for (int i = 0; i < cells.Count; i++)
            {
                AssetJson.terrains.Add(cells[i].TerrainTypeIndex);
            }

            AssetJson.opacity = new List<int>(cells.Count);
            for (int i = 0; i < cells.Count; i++)
            {
                AssetJson.opacity.Add(cells[i].TerrainOpacity);
            }

            var settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, };
            var json = JsonConvert.SerializeObject(AssetJson, settings);
            File.WriteAllText(MapJsonPath + MapJsonName, json);

            //导出地形索引纹理
            if (File.Exists(Application.dataPath + MaterialApplicationPath + "TerrainTypeTexture.png"))
            {
                File.Delete(Application.dataPath + MaterialApplicationPath + "TerrainTypeTexture.png");
            }
            byte[] bytes = HexMapMgr.Instance.Root.GetComponent<HexCellShaderData>().TerrainTypeTexture.EncodeToPNG();
            string filename = Application.dataPath + MaterialApplicationPath + "TerrainTypeTexture.png";
            File.WriteAllBytes(filename, bytes);
            SetTextureProperties(MaterialPath + "TerrainTypeTexture.png");

            //导出地形透明度纹理
            if (File.Exists(Application.dataPath + MaterialApplicationPath + "TerrainOpacityTexture.png"))
            {
                File.Delete(Application.dataPath + MaterialApplicationPath + "TerrainOpacityTexture.png");
            }
            bytes = HexMapMgr.Instance.Root.GetComponent<HexCellShaderData>().TerrainOpacityTexture.EncodeToPNG();
            filename = Application.dataPath + MaterialApplicationPath + "TerrainOpacityTexture.png";
            File.WriteAllBytes(filename, bytes);
            SetTextureProperties(MaterialPath + "TerrainOpacityTexture.png");

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            Thread.Sleep(500); 

            Texture2D TerrainTypeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(MaterialPath + "TerrainTypeTexture.png");
            HexMapMgr.Instance.TerrainMaterial.SetTexture("_TerrainTypeTexture", TerrainTypeTexture);

            Texture2D TerrainOpacityTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(MaterialPath + "TerrainOpacityTexture.png");
            HexMapMgr.Instance.TerrainMaterial.SetTexture("_TerrainOpacityTexture", TerrainOpacityTexture);

            AssetDatabase.Refresh();

            state = State.Export;
        }

        void SetTextureProperties(string path)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            TextureImporter texture = AssetImporter.GetAtPath(path) as TextureImporter;

            texture.textureCompression = TextureImporterCompression.Uncompressed;

            texture.filterMode = FilterMode.Point;
            texture.npotScale = TextureImporterNPOTScale.None;
            texture.sRGBTexture = false;
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }

        private Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        partial void Import()
        {
            string path = EditorUtility.OpenFilePanel("Select Scene", MapPath, "unity");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (!File.Exists(path))
            {
                Debug.LogErrorFormat("关文件不存在, file:{0}", path);
                return;
            }
            string[] p = path.Split('/');
            if (p.Length == 0)
            {
                Debug.LogErrorFormat("文件名称错误, file:{0}", path);
                return;
            }

            path = p[p.Length - 1];

            p = path.Split('.');
            if (p.Length != 2)
            {
                Debug.LogErrorFormat("文件名称错误, file:{0}", path);
                return;
            }

            string Name = p[0];
            id = int.Parse(Name.Split('_')[0]);
            name = Name.Split('_')[1];
            Load();

        }


        void Load()
        {

            HexMapMgr.Instance.UnloadMap();
            EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);

            HexMapMgr.Instance.Data = new WorldMapData();
            HexMapMgr.Instance.LoadMaterial(MapPath + MapName + "/Material/Terrain.mat");

            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(MapDataPath);
            HexMapMgr.Instance.LoadScene(asset);

            cells = HexMapMgr.Instance.cells;

            LoadAsset();


            //SetLabel();
            InitBrush();

            if (HexMapMgr.Instance.Root)
                EditorUtility.SetDirty(HexMapMgr.Instance.Root);

            state = State.Import;
        }

        void LoadAsset()
        {
            string json = File.ReadAllText(MapJsonPath + MapJsonName);
            AssetJson = JsonConvert.DeserializeObject<AssetJson>(json);
            if (AssetJson == null)
            {
                EditorUtility.DisplayDialog("Result", "TextureArrays 不存在", "OK");
            }
            TerrainList.Clear();
            textures = new List<Texture2D>();
            for (int i = 0; i < AssetJson.textures.Length; i++)
            {
                Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetJson.textures[i]);
                if (t)
                {
                    textures.Add(t);
                    TerrainType terrain = new TerrainType();
                    terrain.id = i;
                    terrain.name = t.name;
                    TerrainList.Add(terrain);

                }
            }

            if (HexFeature.HexMapAssets == null)
                HexFeature.HexMapAssets = new Dictionary<int, HexMapAsset>();
            HexFeature.HexMapAssets.Clear();

            FeatureList.Clear();
            for (int i = 0; i < AssetJson.features.Count; i++)
            {
                HexMapFeature HexMapFeature = AssetDatabase.LoadAssetAtPath<HexMapFeature>(AssetJson.features[i]);
                Features Features = new Features();
                Features.id = i;
                Features.type = FeatureType.Feature;
                Features.Feature = HexMapFeature;
                FeatureList.Add(Features);
                int id = FeatureList[i].Feature.asset.id;
                HexFeature.HexMapAssets.Add(((int)FeatureType.Feature << HexMetrics.FeatureTypeBit) | id, FeatureList[i].Feature.asset);
                if (HexMapFeature)
                {

                }
            }
            SpecialList.Clear();
            for (int i = 0; i < AssetJson.specials.Count; i++)
            {
                HexMapFeature HexMapFeature = AssetDatabase.LoadAssetAtPath<HexMapFeature>(AssetJson.specials[i]);
                Features Features = new Features();
                Features.id = i;
                Features.type = FeatureType.Special;
                Features.Feature = HexMapFeature;
                SpecialList.Add(Features);

                int id = SpecialList[i].Feature.asset.id;
                HexFeature.HexMapAssets.Add(((int)FeatureType.Special << HexMetrics.FeatureTypeBit) | id, SpecialList[i].Feature.asset);
                if (HexMapFeature)
                {
                }
            }

            Noise = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetJson.Noise);
            RiverMat = AssetDatabase.LoadAssetAtPath<Material>(AssetJson.RiverMat);
            RoadMat = AssetDatabase.LoadAssetAtPath<Material>(AssetJson.RoadMat);
            WaterMat = AssetDatabase.LoadAssetAtPath<Material>(AssetJson.WaterMat);
            WaterShoreMat = AssetDatabase.LoadAssetAtPath<Material>(AssetJson.WaterShoreMat);
            Bridge = AssetDatabase.LoadAssetAtPath<Transform>(AssetJson.Bridge);
            HexMetrics.noiseSource = Noise;

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].TerrainTypeIndex = AssetJson.terrains[i];
            }

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].TerrainOpacity = AssetJson.opacity[i];
            }
        }


        void InitBrush()
        {
            //mode = EditorMode.Brush;
            IsTerrain = false;
            IsElevation = false;
            IsWater = false;
            IsRiver = false;
            IsRoad = false;
            IsFeatureRandomNumber = false;
            IsFeatureRandomDirecton = false;
            IsFeature = false;
            OnEditorModeChanged();
        }



    }
}