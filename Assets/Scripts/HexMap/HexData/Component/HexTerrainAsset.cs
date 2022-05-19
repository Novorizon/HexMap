using HexMap;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HexTerrainAsset : ScriptableObject
{
    public List<Texture2D> textures;
    public Material TerrainMaterial;
    public Texture2D TerrainTypeTexture;
    public Texture2D RoadTexture;
    public Texture2D TerrainOpacityTexture;

    public Texture2D Noise;
    public Material RiverMat;
    public Material WaterMat;
    public Material WaterShoreMat;
    public Material EstuaryMat;
    public Transform Bridge;
    public List<TerrainTexture> terrains;
    public List<Features> features;
    public List<Features> specials;
}


