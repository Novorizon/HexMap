using HexMap;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class HexTerrain
{
#if UNITY_EDITOR
    void ReCreate()
    {
        HexMapData data = HexMapMgr.Instance.Data;
        data.chunkCountX = chunkCountX;
        data.chunkCountZ = chunkCountZ;
        HexMapMgr.Instance.ReCreate(gameObject, data);

        OnRiverMatChanged();
        OnWaterMatChanged();
        OnWaterShoreMatChanged();
        OnNoiseChanged();
    }


    static void Create(MenuCommand menuCommand)
    {
        MapName = "New HexTerrain";
        if (Directory.Exists(AbsolutePath))
        {
            for (int i = 1; i < int.MaxValue; i++)
            {
                MapName = "New HexTerrain " + i;
                if (Directory.Exists(AbsolutePath))
                    continue;

                break;
            }
        }
        Directory.CreateDirectory(AbsolutePath);
        Directory.CreateDirectory(AbsolutePath + "/Material/");

        GameObject root = new GameObject("HexMap");
        root.AddComponent<HexTerrain>();

        Texture2D Default = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.guanjinbiao.hexmap/Assets/Texture/Default.png");
        if (Default == null)
            Default = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/hexmap/Assets/Texture/Default.png", typeof(Texture2D));

        HexMetrics.NoiseSource = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.guanjinbiao.hexmap/Assets/Texture/Noise.png");
        if (HexMetrics.NoiseSource == null)
            HexMetrics.NoiseSource = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/hexmap/Assets/Texture/Noise.png", typeof(Texture2D));

        Instance.Noise = HexMetrics.NoiseSource;

        HexMapMgr.Instance.GridTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.guanjinbiao.hexmap/Assets/Texture/Grid.png");
        if (HexMapMgr.Instance.GridTex == null)
            HexMapMgr.Instance.GridTex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/hexmap/Assets/Texture/Grid.png", typeof(Texture2D));

        //Texture2DArray textureArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>("Packages/com.guanjinbiao.hexmap/Assets/Texture/TerrainTextureArray.asset");
        //if (textureArray == null)
        //    textureArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>("Assets/hexmap/Assets/Texture/TerrainTextureArray.asset");

        Shader shader = Shader.Find("Custom/Lit/Terrain");
        Material mat = new Material(shader);
        mat.SetFloat("_EDITOR", 1);
        //mat.SetTexture("_MainTex", textureArray);
        mat.SetTexture("_EditorTexture", Default);
        mat.SetTexture("_NoiseTex", HexMetrics.NoiseSource);
        mat.SetTexture("_GridTex", HexMapMgr.Instance.GridTex);
        mat.SetColor("_Specular", new Color(60 / 255f, 60 / 255f, 60 / 255f));
        mat.SetFloat("_Smoothness", 0.5f);
        mat.enableInstancing = true;
        AssetDatabase.CreateAsset(mat, MaterialPath + "/Terrain.mat");
        AssetDatabase.Refresh();
        HexMapMgr.Instance.SetMaterial(mat);

        HexMapMgr.Instance.Create(root);
        CreateTexture(HexMapMgr.Instance.Data.cellCountX, HexMapMgr.Instance.Data.cellCountZ);


        for (int i = 0; i < HexMapMgr.Instance.cells.Count; i++)
        {
            HexMapMgr.Instance.RefreshTerrain(HexMapMgr.Instance.cells[i]);
        }
        GameObjectUtility.SetParentAndAlign(root, menuCommand.context as GameObject);//设置父节点为当前选中物体
        Undo.RegisterCreatedObjectUndo(root, "Create" + root.name);//注册到Undo系统,允许撤销
        Selection.activeObject = root;//将新建物体设为当前选中物体

    }

    void Load()
    {
        HexTerrainData terrain = gameObject.GetComponent<HexTerrainData>();
        if (terrain == null)
        {
            //EditorUtility.DisplayDialog("Result", "HexTerrainData 不存在", "OK");
            return;
        }
        LoadAsset(terrain);
        //SetTextureProperties(AbsoluteTerrainTypeTexturePath, true);
        //SetTextureProperties(AbsoluteTerrainOpacityTexturePath, true);

        HexMapMgr.Instance.Load(gameObject);

        id = HexMapMgr.Instance.Data.id;
        name = HexMapMgr.Instance.Data.name;
        MapName = HexMapMgr.Instance.Data.name;
        RoadCost = HexMapMgr.Instance.Data.RoadCost;
        FlatCost = HexMapMgr.Instance.Data.FlatCost;
        SlopeCost = HexMapMgr.Instance.Data.SlopeCost;

        cellCountX = HexMapMgr.Instance.Data.cellCountX;
        cellCountZ = HexMapMgr.Instance.Data.cellCountZ;
        cellCount = cellCountX * cellCountZ;
        cellMetreX = cellCountX * radius * HexMetrics.outerToInner * 2;
        cellMetreZ = 1.5f * cellCountZ * radius + 0.5f * radius;
        InitBrush();
    }

    void LoadAsset(HexTerrainData terrain)
    {
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("Result", "HexTerrainData 不存在", "OK");
        }

        if (HexFeature.HexMapAssets == null)
            HexFeature.HexMapAssets = new Dictionary<int, HexMapAsset>();
        HexFeature.HexMapAssets.Clear();

        HexMapMgr.Instance.TerrainMaterial = terrain.terrain.TerrainMaterial;
        HexMapMgr.Instance.TerrainTypeTexture = terrain.terrain.TerrainTypeTexture;
        HexMapMgr.Instance.TerrainOpacityTexture = terrain.terrain.TerrainOpacityTexture;
        HexMapMgr.Instance.RoadTexture = terrain.terrain.RoadTexture;
        SetTextureProperties(HexMapMgr.Instance.TerrainTypeTexture, true);
        SetTextureProperties(HexMapMgr.Instance.TerrainOpacityTexture, true);
        SetTextureProperties(HexMapMgr.Instance.RoadTexture, true);


        Noise = terrain.terrain.Noise;
        HexMetrics.NoiseSource = Noise;

        //terrain.terrain.textures = textures;
        RiverMat = terrain.terrain.RiverMat;
        WaterMat = terrain.terrain.WaterMat;
        WaterShoreMat = terrain.terrain.WaterShoreMat;
        EstuaryMat = terrain.terrain.EstuaryMat;
        Bridge = terrain.terrain.Bridge;

        List<HexMap.TerrainTexture> terrains = terrain.terrain.terrains;
        List<HexMap.Features> features = terrain.terrain.features;
        List<HexMap.Features> specials = terrain.terrain.specials;

        textures.Clear();
        for (int i = 0; i < terrains.Count; i++)
        {
            if (terrain.terrain.terrains[i].texture != null)
            {

                TerrainTexture terrainType = new TerrainTexture();
                terrainType.id = terrains[i].id;
                terrainType.opacity = terrains[i].opacity;
                terrainType.cost = terrains[i].id;
                terrainType.t = terrains[i].texture;
                textures.Add(terrainType);
            }
        }


        FeatureList.Clear();
        for (int i = 0; i < features.Count; i++)
        {
            Features features1 = new Features();
            features1.id = features[i].id;
            features1.cost = features[i].id;
            features1.path = features[i].path;
            features1.Feature = features[i].Feature;
            features1.type = features[i].type;
            FeatureList.Add(features1);

            int id = FeatureList[i].Feature.asset.id;
            HexFeature.HexMapAssets.Add(((int)FeatureType.Feature << HexMetrics.FeatureTypeBit) | id, FeatureList[i].Feature.asset);
        }

        SpecialList.Clear();
        for (int i = 0; i < specials.Count; i++)
        {
            Features features1 = new Features();
            features1.id = features[i].id;
            features1.cost = features[i].id;
            features1.path = features[i].path;
            features1.Feature = features[i].Feature;
            features1.type = features[i].type;
            SpecialList.Add(features1);

            int id = SpecialList[i].Feature.asset.id;
            HexFeature.HexMapAssets.Add(((int)FeatureType.Special << HexMetrics.FeatureTypeBit) | id, SpecialList[i].Feature.asset);
        }
    }
    //Path.GetFileNameWithoutExtension(path)
    void Export()
    {
        if (MapName != name)
        {
            AssetDatabase.RenameAsset(HexTerrainAssetPath, name);
            AssetDatabase.RenameAsset(MapJsonPath, name);
            AssetDatabase.RenameAsset(MapScenePath, name);
            AssetDatabase.RenameAsset(Path, name);
            MapName = name;
        }
        if (!Directory.Exists(AbsolutePath))
        {
            Directory.CreateDirectory(AbsolutePath);
        }

        HexMapMgr.Instance.Data.id = id;
        HexMapMgr.Instance.Data.name = name;
        HexMapMgr.Instance.Data.version = version;
        HexMapMgr.Instance.Data.RoadCost = RoadCost;
        HexMapMgr.Instance.Data.FlatCost = FlatCost;
        HexMapMgr.Instance.Data.SlopeCost = SlopeCost;

        HexTerrainData terrain = HexMapMgr.Instance.Root.GetComponent<HexTerrainData>();
        terrain.terrain = ScriptableObject.CreateInstance<HexTerrainAsset>();

        terrain.cells = HexMapMgr.Instance.cells;
        terrain.data = HexMapMgr.Instance.Data;

        terrain.terrain.TerrainMaterial = HexMapMgr.Instance.TerrainMaterial;
        terrain.terrain.TerrainTypeTexture = HexMapMgr.Instance.TerrainTypeTexture;
        terrain.terrain.TerrainOpacityTexture = HexMapMgr.Instance.TerrainOpacityTexture;
        terrain.terrain.RoadTexture = HexMapMgr.Instance.RoadTexture;

        terrain.terrain.Bridge = Bridge;
        terrain.terrain.Noise = Noise;
        terrain.terrain.RiverMat = RiverMat;
        terrain.terrain.WaterMat = WaterMat;
        terrain.terrain.WaterShoreMat = WaterShoreMat;
        terrain.terrain.EstuaryMat = EstuaryMat;

        terrain.terrain.terrains = new List<HexMap.TerrainTexture>();
        terrain.terrain.features = new List<HexMap.Features>();
        terrain.terrain.specials = new List<HexMap.Features>();
        for (int i = 0; i < textures.Count; i++)
        {
            HexMap.TerrainTexture terrainType = new HexMap.TerrainTexture();
            terrainType.id = textures[i].id;
            terrainType.opacity = textures[i].opacity;
            terrainType.cost = textures[i].id;
            terrainType.texture = textures[i].t;
            terrain.terrain.terrains.Add(terrainType);
        }
        for (int i = 0; i < FeatureList.Count; i++)
        {
            HexMap.Features features1 = new HexMap.Features();
            features1.id = FeatureList[i].id;
            features1.cost = FeatureList[i].id;
            features1.path = FeatureList[i].path;
            features1.Feature = FeatureList[i].Feature;
            features1.type = FeatureList[i].type;
            terrain.terrain.features.Add(features1);

        }
        for (int i = 0; i < SpecialList.Count; i++)
        {
            HexMap.Features features1 = new HexMap.Features();
            features1.id = SpecialList[i].id;
            features1.cost = SpecialList[i].id;
            features1.path = SpecialList[i].path;
            features1.Feature = SpecialList[i].Feature;
            features1.type = SpecialList[i].type;
            terrain.terrain.specials.Add(features1);
        }

        AssetDatabase.CreateAsset(terrain.terrain, HexTerrainAssetPath);

        ServerTerrainData data = new ServerTerrainData();
        data.data = terrain.data;
        data.cells = terrain.cells;

        var settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, };
        var json = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(MapJsonPath, json);

        Scene scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene, MapScenePath, true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

#endif
}