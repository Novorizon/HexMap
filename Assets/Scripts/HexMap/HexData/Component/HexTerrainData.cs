using System;
using System.Collections.Generic;
using UnityEngine;
using HexMap;

[Serializable]

public class HexTerrainData : MonoBehaviour
{
    [HideInInspector]
    public GameObject Root;
    [HideInInspector]
    public HexMapData data;
    [HideInInspector]
    public List<HexCell> cells;

    [HideInInspector]
    public List<HexChunkMgr> chunks;

    public HexTerrainAsset terrain;
}



public class ServerTerrainData
{
    public HexMapData data;
    public List<HexCell> cells;
}


