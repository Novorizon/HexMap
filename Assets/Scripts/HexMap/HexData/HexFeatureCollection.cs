using UnityEngine;

[System.Serializable]
public struct HexFeatureCollection
{

    public GameObject[] prefabs;

    public GameObject Pick(float choice)
    {
        return prefabs[(int)(choice * prefabs.Length)];
    }
}