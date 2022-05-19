using System;

namespace HexMap
{

    [Serializable]
    public class HexMapData
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

        public int RoadCost;
        public int FlatCost;
        public int SlopeCost;
        public bool isFOW = false;
        public bool isExplorer = false;

        public HexMapData()
        {
            chunkCountX = 1;
            chunkCountZ = 1;

            cellCountX = chunkCountX * HexMetrics.chunkSizeX;
            cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
            cellCount = cellCountX * cellCountZ;
        }

    }
    //[Serializable]
    //public class HexMapData
    //{
    //    public int id;
    //    public string name;
    //    public int version = 0;

    //    public int chunkCountX;
    //    public int chunkCountZ;
    //    public int seed;

    //    public int cellCount;
    //    public int cellCountX;
    //    public int cellCountZ;

    //    public HexMapData()
    //    {
    //        chunkCountX = 1;
    //        chunkCountZ = 1;

    //        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
    //        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
    //        cellCount = cellCountX * cellCountZ;
    //    }

    //}
}