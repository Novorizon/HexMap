using System.Collections.Generic;
using UnityEngine;

namespace HexMap
{
    public enum FeatureType
    {
        Feature = 1,
        Special = 2,
    }
    public class HexFeature : MonoBehaviour
    {
        //public List<HexMapAsset[]> FeatureGroups;
        public static Dictionary<int, HexMapAsset> HexMapAssets;//只记录当前地图使用的特征资源id和对应的资源

        public Transform bridge;
        public Transform[] special;

        Transform container;

        public void Clear()
        {
            if (transform && transform.Find("Features Container"))
            {
                GameObject o = transform.Find("Features Container").gameObject;
                if (o)
                {
                    GameObject.DestroyImmediate(o);
                }
            }
            container = new GameObject("Features Container").transform;
            container.SetParent(transform, false);
        }

        public void Apply() { }


        public void AddFeatureRandom(HexCell cell, Vector3 position)
        {
            if (cell.IsSpecial)
            {
                return;
            }
            for (int i = 0; i < cell.Features.Count; i++)
            {
                //去掉随机字段，加上 FeatureType字段
                int feature = cell.Features[i] & HexMetrics.FeatureMask | ((int)FeatureType.Feature << HexMetrics.FeatureTypeBit);

                bool isrn = (cell.Features[i] & HexMetrics.FeatureRandomNumberMask) > 0;
                bool isrd = (cell.Features[i] & HexMetrics.FeatureRandomDirectionMask) > 0;

                HexHash hash = HexMetrics.SampleHashGrid(position);

                if (!isrn)
                    continue;
                if (hash.features[i % HexMetrics.FeatureCount] > 0.5f)
                    continue;

                HexMapAssets.TryGetValue(feature, out HexMapAsset Asset);
                if (Asset == null)
                {
                    Debug.LogError("no prefab");
                    continue;
                }
                GameObject prefab = Asset.asset;

                if (prefab)
                {
                    GameObject instance = GameObject.Instantiate(prefab);
                    position.y += instance.transform.localScale.y * 0.5f;
                    instance.transform.localPosition = HexMetrics.Perturb(position);
                    if (isrd)
                        instance.transform.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
                    else
                        instance.transform.localRotation = Quaternion.identity;
                    instance.transform.SetParent(container, false);
                }
            }

        }

        public void AddFeature(HexCell cell, Vector3 position)
        {
            if (cell.IsSpecial)
            {
                return;
            }
            for (int i = 0; i < cell.Features.Count; i++)
            {
                //去掉随机字段，加上 FeatureType字段
                int feature = cell.Features[i] & HexMetrics.FeatureMask | ((int)FeatureType.Feature << HexMetrics.FeatureTypeBit);

                bool isrn = (cell.Features[i] & HexMetrics.FeatureRandomNumberMask) > 0;
                bool isrd = (cell.Features[i] & HexMetrics.FeatureRandomDirectionMask) > 0;

                HexHash hash = HexMetrics.SampleHashGrid(position);

                if (isrn && hash.features[i % HexMetrics.FeatureCount] > 0.5f)
                    continue;

                HexMapAssets.TryGetValue(feature, out HexMapAsset Asset);
                if (Asset == null)
                {
                    Debug.LogError("no prefab");
                    continue;
                }
                GameObject prefab = Asset.asset;

                if (prefab)
                {
                    GameObject instance = GameObject.Instantiate(prefab);
                    //position.y += instance.transform.localScale.y * 0.5f;
                    instance.transform.localPosition = HexMetrics.Perturb(position);
                    if (isrd)
                        instance.transform.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
                    else
                        instance.transform.localRotation = Quaternion.identity;
                    instance.transform.SetParent(container, false);
                }

            }
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = cell.GetNeighbor(d);
                //neighbor.features
            }

        }


        public void AddSpecialFeature(HexCell cell, Vector3 position)
        {
            HexHash hash = HexMetrics.SampleHashGrid(position);

            //去掉随机字段，加上 FeatureType字段
            int feature = cell.SpecialIndex & HexMetrics.FeatureMask | ((int)FeatureType.Special << HexMetrics.FeatureTypeBit);

            bool isrn = (cell.SpecialIndex & HexMetrics.FeatureRandomNumberMask) > 0;
            bool isrd = (cell.SpecialIndex & HexMetrics.FeatureRandomDirectionMask) > 0;


            if (isrn && hash.e > 0.5f)
                return;

            HexMapAssets.TryGetValue(feature, out HexMapAsset Asset);
            if (Asset == null)
            {
                Debug.LogError("no prefab");
                return;
            }

            if (Asset.asset)
            {
                GameObject instance = GameObject.Instantiate(Asset.asset);
                position.y += instance.transform.localScale.y * 0.5f;
                instance.transform.localPosition = HexMetrics.Perturb(position);
                if (isrd)
                    instance.transform.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
                else
                    instance.transform.localRotation = Quaternion.identity;
                instance.transform.SetParent(container, false);
            }
        }


        public void AddBridge(Vector3 roadCenter1, Vector3 roadCenter2)
        {
            roadCenter1 = HexMetrics.Perturb(roadCenter1);
            roadCenter2 = HexMetrics.Perturb(roadCenter2);
            Transform instance = Instantiate(bridge);
            instance.localPosition = (roadCenter1 + roadCenter2) * 0.5f;
            instance.forward = roadCenter2 - roadCenter1;
            float length = Vector3.Distance(roadCenter1, roadCenter2);
            instance.localScale = new Vector3(1f, 1f, length * (1f / HexMetrics.bridgeDesignLength)
            );
            instance.SetParent(container, false);
        }

    }
}
