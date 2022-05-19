
using UnityEngine;
using HexMap;
using UnityEditor;
using System;
using System.IO;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif


public enum PropertySettings
{
    Property,
    Texture2D,
    Chunk,
    Grid,
    Button,

}

public partial class HexTerrain
{
#if UNITY_EDITOR

    [Title("地图属性")]
    [LabelText("地图id"), MinValue(0), PropertyOrder((int)PropertySettings.Texture2D), PropertySpace(SpaceBefore = 10), LabelWidth(50), VerticalGroup("ID"), ShowIf("IsSettings")]
    public int id;
    [LabelText("地图名称"), PropertyOrder((int)PropertySettings.Texture2D), PropertySpace(SpaceBefore = 3), LabelWidth(50), VerticalGroup("ID"), ShowIf("IsSettings")]
    public new string name;
    [LabelText("版本号"), PropertyOrder((int)PropertySettings.Texture2D), PropertySpace(SpaceBefore = 3), LabelWidth(50), VerticalGroup("ID"), ShowIf("IsSettings")]
    int version = 0;


    [LabelText("噪点图"), ToggleLeft, PropertySpace(SpaceBefore = 30), PropertyOrder((int)PropertySettings.Chunk), OnValueChanged("OnIsNoiseChanged"), LabelWidth(3), HorizontalGroup("ID "), ShowIf("IsSettings")]
    public bool IsNoise;
    [HideLabel, PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 30), OnValueChanged("OnNoiseChanged"), LabelWidth(50), HorizontalGroup("ID ", marginLeft: -100), ShowIf("IsSettings")]
    public Texture2D Noise;


    [Title("地图块")]
    [LabelText("单元格半径"), PropertySpace(SpaceBefore = 30), PropertyOrder((int)PropertySettings.Chunk), LabelWidth(60), OnValueChanged("OnValueChanged"), HorizontalGroup("radius ", marginRight: 100), ShowIf("IsSettings")]
    public float radius = 1;
    [LabelText("宽"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertySettings.Chunk), LabelWidth(30), MinValue(1), HorizontalGroup("chunkCountX", marginRight: 100), MaxValue(256), OnValueChanged("OnValueChanged"), ShowIf("IsSettings")]
    public int chunkCountX = 1;
    [LabelText("高"), PropertySpace(SpaceBefore = 3), PropertyOrder((int)PropertySettings.Chunk), LabelWidth(30), MinValue(1), HorizontalGroup("chunkCountZ", marginRight: 100), MaxValue(256), OnValueChanged("OnValueChanged"), ShowIf("IsSettings")]
    public int chunkCountZ = 1;


    [LabelText("单元格数量"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 10), LabelWidth(60), ReadOnly, HorizontalGroup("cellCount "), ShowIf("IsSettings")]
    public int cellCount = 25;

    [LabelText("宽(x)"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 10), LabelWidth(30), ReadOnly, HorizontalGroup("cellCountX "), ShowIf("IsSettings")]
    public int cellCountX = 5;
    [HideLabel, SuffixLabel("(m)"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 10), LabelWidth(30), ReadOnly, HorizontalGroup("cellCountX "), ShowIf("IsSettings")]
    public float cellMetreX = 8.66025404f;

    [LabelText("高(z)"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 10, SpaceAfter = 30), LabelWidth(30), ReadOnly, HorizontalGroup("cellCountZ "), ShowIf("IsSettings")]
    public int cellCountZ = 5;
    [HideLabel, SuffixLabel("(m)"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 10, SpaceAfter = 30), LabelWidth(30), ReadOnly, HorizontalGroup("cellCountZ "), ShowIf("IsSettings")]
    public float cellMetreZ = 8;

    [LabelText("显示网格"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertySettings.Chunk), HorizontalGroup("showGrid"), OnValueChanged("OnShowGridChanged"), ShowIf("IsSettings")]
    public bool showGrid = false;
    [LabelText("显示3D坐标"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertySettings.Chunk), HorizontalGroup("showCoordinate"), OnValueChanged("OnShowCoordinateChanged"), ShowIf("IsSettings")]
    public bool showCoordinate = false;
    [LabelText("显示ID"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertySettings.Chunk), HorizontalGroup("showId"), OnValueChanged("OnShowIdChanged"), ShowIf("IsSettings")]
    public bool showId = false;
    [LabelText("显示XZ坐标"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertySettings.Chunk), HorizontalGroup("showXZ"), OnValueChanged("SetXZLabel"), ShowIf("IsSettings")]
    public bool showXZ = false;


    [LabelText("随机数种子"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 30), LabelWidth(60), HorizontalGroup("Random", marginLeft: 10, marginRight: 10), OnValueChanged("OnValueChanged"), ShowIf("IsSettings")]
    [InlineButton("Random")]
    int seed;

    [Button("Apply"), PropertyOrder((int)PropertySettings.Chunk), PropertySpace(SpaceBefore = 30, SpaceAfter = 30), LabelWidth(30), ShowIf("IsSettings")]
    public void Apply()
    {
        Export();
    }

    public void Random()
    {
        seed = UnityEngine.Random.Range(0, int.MaxValue);
        seed ^= (int)System.DateTime.Now.Ticks;
        seed ^= (int)Time.unscaledTime;
        seed &= int.MaxValue;

        changed = true;
        UType = UpdateType.ReCreate;
    }

    public void CreateTextures()
    {
        for (int i = textures.Count - 1; i >= 0; i--)
        {
            if (textures[i] == null)
            {
                textures.RemoveAt(i);
                return;
            }
        }

        if (textures == null || textures.Count == 0)
            return;

        Texture2D t = textures[0].t;
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
                    Graphics.CopyTexture(textures[i].t, 0, m, textureArray, i, m);

                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }

            }
            textures[i].id = i;
            //TerrainList.Add(new TerrainType() { use = false, name = textures[i].name, id = i, t = textures[i] });
        }
        File.Delete(AbsoluteTerrainTextureArrayPath);
        AssetDatabase.CreateAsset(textureArray, MaterialPath + "TerrainTextureArray.asset");
        AssetDatabase.Refresh();

        if (HexMapMgr.Instance.TerrainMaterial)
        {
            HexMapMgr.Instance.TerrainMaterial.SetTexture("_MainTex", textureArray);
        }
    }



    public void OnIsNoiseChanged()
    {
        HexMetrics.UseNoise = IsNoise && Noise != null;
        HexMetrics.NoiseSource = Noise;
        //更新所有与扰动相关的内容

        HexMapMgr.Instance.Refresh();
    }

    public void OnNoiseChanged()
    {
        if (HexMapMgr.Instance.TerrainMaterial != null)
            HexMapMgr.Instance.TerrainMaterial.SetTexture("_NoiseTex", Noise);

        HexMetrics.UseNoise = IsNoise && Noise != null;
        HexMetrics.NoiseSource = Noise;

        HexMapMgr.Instance.Refresh();
    }

    public bool DeleteFiles(string path)
    {
        return true;
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

    public void OnShowGridChanged()
    {
        if (showGrid)
        {
            EnableHighlight();
        }
        else
        {
            DisableHighlight();
        }
    }

    public void OnShowCoordinateChanged()
    {
        if (showCoordinate)
        {
            SetCoordinateLabel();
        }
        else
        {
            ClearLabel();
        }
    }

    public void OnShowIdChanged()
    {
        if (showId)
        {
            SetIdLabel();
        }
        else
        {
            ClearLabel();
        }
    }

    public void OnShowXZChanged()
    {
        if (showXZ)
        {
            SetXZLabel();
        }
        else
        {
            ClearLabel();
        }
    }

#endif
}