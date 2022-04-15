using HexMap;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap
{

    [Serializable]
    public class WorldMapData
    {
        public int id;
        public string name;
        public int version = 0;

        public int chunkCountX;
        public int chunkCountZ;
        public int seed;

        public int cellCount;
        public int cellCountX;
        public int cellCountZ;
        public bool isFOW = false;
        public bool isExplorer = false;

        public WorldMapData()
        {
        }

    }
}