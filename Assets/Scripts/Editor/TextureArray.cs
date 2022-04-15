using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldMapEditor
{
    [Serializable]
    public class AssetJson
    {
        public string[] textures { get; set; }
        public string Noise;
        public string RiverMat;
        public string RoadMat;
        public string WaterMat;
        public string WaterShoreMat;
        public string Bridge;
        public List< string> features { get; set; }
        public List<string> specials { get; set; }
        public List<int> terrains { get; set; }
        public List<int> opacity { get; set; }
    }
    [Serializable]
    public class TextureArray
    {
        public int id { get; set; }
        public string name { get; set; }
        public long Guid { get; set; }
        public string path { get; set; }
    }
}
