using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using HexMap;

namespace HexMap
{

    [Serializable]
    public class Features
    {
        [HideInInspector,]
        public int id;

        public FeatureType type;
        public bool use;
        public HexMapFeature Feature;
        public int cost;
        public string path { get; set; }
    }

    [Serializable]
    public class TerrainTexture
    {
        public int id;
        public bool use;

        public Texture2D texture = null;
        public float opacity = 1;
        public int cost;

    }
}