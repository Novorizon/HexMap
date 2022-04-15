using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "HexMapFeatures", menuName = "HexMapFeatures", order = 1000)]
[Serializable]
public class HexMapFeatures : ScriptableObject
{
    public List<HexMapFeatureGroup> FeatureGroups;
}

[Serializable]
public class HexMapFeatureGroup
{
    public int id;
    public string name;
    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
    public HexMapFeature[] Features = new HexMapFeature[3];

}