using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "HexMapAssets", menuName = "HexMapAssets", order = 1000)]
public class HexMapAssets : ScriptableObject
{
    [Searchable, ListDrawerSettings(), OnValueChanged("OnValueChanged")]
    public List<HexMapAsset> Assets;
    void OnValueChanged()
    {

    }

#if UNITY_EDITOR

    [Button("µ¼³ö", ButtonSizes.Medium), PropertySpace(SpaceBefore = 40)]
    public void Import()
    {
        for (int i = 0; i < Assets.Count; i++)
        {
            HexMapFeature feature = ScriptableObject.CreateInstance<HexMapFeature>();
            feature.name = Assets[i].name;
            feature.asset = Assets[i];


            AssetDatabase.CreateAsset(feature, "Assets/Plugins/HexMap/Editor/Assets/" + Assets[i].name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
    }
#endif
}
