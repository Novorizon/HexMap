
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using HexMap;
using UnityEditor;
using System.Linq;
using Newtonsoft.Json;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif


[Serializable]
public class Features
{
    [HideInInspector,]
    public int id;

    [HideInInspector,]
    public FeatureType type;

    [ToggleLeft, HideLabel, LabelWidth(10), HorizontalGroup("FeatureType"), OnValueChanged("OnUseChanged")]
    public bool use;

    [HideLabel, LabelWidth(100), HorizontalGroup("FeatureType"), OnValueChanged("OnValueChanged"), JsonIgnore]
    public HexMapFeature Feature;

    [LabelText("   cost"), LabelWidth(40), HorizontalGroup("FeatureType"), OnValueChanged("OnFeatureCostChanged")]
    public int cost;

    [HideInInspector,]
    public string path { get; set; }
    public void OnValueChanged()
    {
        HexTerrain.Instance.OnFeaturesChanged();
        path = AssetDatabase.GetAssetPath(Feature);
    }
    public void OnUseChanged()
    {
        if (type == FeatureType.Special)
            HexTerrain.Instance.OnUseChanged(id);
    }
    public void OnFeatureCostChanged()
    {
        HexTerrain.Instance.OnFeatureCostChanged();
    }
}
public partial class HexTerrain
{
#if UNITY_EDITOR
    [Title("自定义资源")]

    [LabelText("橡皮擦"), ToggleLeft, PropertyOrder((int)PropertyOrder.Brush), /*Brush("Erase"), */ShowIf("IsBrush"), ShowIf("IsFeature"), OnValueChanged("OnEraseChanged"), HorizontalGroup("isFeatureErase")]
    public bool isFeatureErase;

    [LabelText("随机数量"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), LabelWidth(30), ShowIf("IsFeature")]
    public bool IsFeatureRandomNumber = false;
    [LabelText("随机朝向"), ToggleLeft, PropertySpace(SpaceBefore = 3), PropertyOrder((int)PropertyOrder.Brush), LabelWidth(30), ShowIf("IsFeature")]
    public bool IsFeatureRandomDirecton = false;


    [LabelText("特征"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsFeature"),]
    [Brush("Feature"), OnValueChanged("OnFeatureChanged"), HorizontalGroup("Feature")]
    public bool IsFeatureBrush = false;

    [ListDrawerSettings(DraggableItems = false), LabelText("特征数组"), PropertySpace(SpaceBefore = 3, SpaceAfter = 3), PropertyOrder((int)PropertyOrder.Brush), OnValueChanged("OnFeaturesChanged"), ShowIf("IsFeature"),]
    public List<Features> FeatureList = new List<Features>();


    [LabelText("特殊"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsFeature"),]
    [Brush("Special"), OnValueChanged("OnSpecialChanged"), HorizontalGroup("Special")]
    public bool IsSpecialBrush = false;

    [ListDrawerSettings(DraggableItems = false), LabelText("特殊特征数组"), PropertySpace(SpaceBefore = 3, SpaceAfter = 3), PropertyOrder((int)PropertyOrder.Brush), OnValueChanged("OnFeaturesChanged"), ShowIf("IsFeature"),]
    public List<Features> SpecialList = new List<Features>();

    [LabelText("桥梁"), PropertySpace(SpaceBefore = 30), PropertyOrder((int)PropertyOrder.Brush), ShowIf("IsFeature"), OnValueChanged("OnBridgeChanged"), HorizontalGroup("Bridge")] public Transform Bridge;

    public void OnUseChanged(int id)
    {
        for (int i = 0; i < SpecialList.Count; i++)
        {
            SpecialList[i].use = false;

            if (SpecialList[i].Feature == null || SpecialList[i].Feature.asset.asset == null)
                continue;

            if (SpecialList[i].id == id)
                SpecialList[i].use = true;
        }
    }

    //为了特征 、特殊特征用到的资源存到一个容器中（HexFeature.HexMapAssets ), 避免加载资源时 多次加载，简化外部api调用
    //HexFeature.HexMapAssets  的key 为 ：(FeatureType<<FeatureTypeBit) | id
    //这样 一个完整的feature id:
    //  0000 0000 0000 0000 0000 0000 0000 0000
    //低16位(0~15)存储资源id
    //16位存储 是否随机数量
    //17位存储 是否随机朝向
    //28~30存储资源类型
    public void OnFeaturesChanged()
    {
        if (HexFeature.HexMapAssets == null)
            HexFeature.HexMapAssets = new Dictionary<int, HexMapAsset>();
        HexFeature.HexMapAssets.Clear();

        for (int i = 0; i < FeatureList.Count; i++)
        {
            if (FeatureList[i].Feature == null || FeatureList[i].Feature.asset.asset == null)
                continue;

            FeatureList[i].id = i;
            FeatureList[i].type = FeatureType.Feature;
            int id = FeatureList[i].Feature.asset.id;
            HexFeature.HexMapAssets.Add(((int)FeatureType.Feature << HexMetrics.FeatureTypeBit) | id, FeatureList[i].Feature.asset);
        }

        for (int i = 0; i < SpecialList.Count; i++)
        {
            if (SpecialList[i].Feature == null || SpecialList[i].Feature.asset.asset == null)
                continue;

            SpecialList[i].id = i;
            SpecialList[i].type = FeatureType.Special;
            int id = SpecialList[i].Feature.asset.id;
            HexFeature.HexMapAssets.Add(((int)FeatureType.Special << HexMetrics.FeatureTypeBit) | id, SpecialList[i].Feature.asset);
        }
    }


    public void OnBridgeChanged()
    {
        for (int i = 0; i < HexMapMgr.Instance.chunks.Count; i++)
        {
            HexMapMgr.Instance.chunks[i].chunk.Features.bridge = Bridge;
        }

    }
    public void OnFeatureCostChanged()
    {
        List<HexCell> cells = HexMapMgr.Instance.cells;
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].featureCost = 0;
            int count = cells[i].features.Count(a => (a & HexMetrics.FeatureTypeNormalMask) > 0);
            if (count > 0)
            {
                for (int j = 0; j < cells[i].features.Count; j++)
                {
                    Features features = FeatureList.Find(a => a.Feature.asset.id == (cells[i].features[j] & HexMetrics.FeatureMask));
                    if (features == null)
                        continue;
                    cells[i].featureCost += features.cost;
                }
            }

            //目前普通特征和特殊特征是互斥  因此可以都存到List<int> features，通过下句来判断有哪种类型。当前不使用此实现 2022-4-20
            //count = cells[i].features.Count(a => (a & HexMetrics.FeatureTypeSpecialMask) > 0);

            if (cells[i].SpecialIndex > 0)
            {
                Features features = SpecialList.Find(a => a.Feature.asset.id == (cells[i].SpecialIndex & HexMetrics.FeatureMask));
                if (features == null)
                    return;
                cells[i].featureCost = features.cost;

            }
        }


    }
#endif
}
