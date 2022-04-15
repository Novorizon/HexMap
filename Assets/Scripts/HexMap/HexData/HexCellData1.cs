//using UnityEngine;
//using System;
//using Newtonsoft.Json;
//using System.Collections.Generic;

//namespace HexMap
//{

//    [Serializable]
//    public partial class HexCell
//    {
//        public HexCell()
//        {
//            neighbors = new int[6] { -1, -1, -1, -1, -1, -1 };
//            roads = new bool[6];
//            features = new List<int>();
//        }

//        public HexCell(int i, int x, int z)
//        {
//            id = i;
//            coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
//            neighbors = new int[6] { -1, -1, -1, -1, -1, -1 };
//            roads = new bool[6];
//            TerrainTypeIndex = 0;
//            Elevation = 0;
//            features = new List<int>();
//        }

//        public Vector2Int chunkCoordinate;
//        [JsonIgnore] public HexChunkMgr ChunkMgr;

//        public int id;
//        public Vector3 center;
//        public HexCoordinates coordinates;
//        public Vector2Int xz;

//        int[] neighbors;
//        public int[] Neighbors
//        {
//            get { return neighbors; }
//        }
//        public bool[] roads;

//        //[HexMap] public Color color;
//        int elevation = int.MinValue;
//        int waterLevel;
//        int specialIndex;
//        public List<int> features;
//        int terrainTypeIndex;
//        int distance;


//        bool hasIncomingRiver;
//        bool hasOutgoingRiver;
//        HexDirection incomingRiver;
//        HexDirection outgoingRiver;

//        int visibility = 0;//战争迷雾
//        bool explored;//探索

//        public bool IsExplored
//        {
//            get { return explored && Explorable; }
//            private set { explored = value; }
//        }
//        public bool Explorable { get; set; }//视野阻挡

//        public HexCellShaderData ShaderData { get; set; }
//        public int Distance
//        {
//            get { return distance; }
//            set { distance = value; }
//        }

//        public HexCell PathFrom { get; set; }
//        public int SearchHeuristic { get; set; }
//        public int SearchPriority { get { return distance + SearchHeuristic; } }
//        public HexCell NextWithSamePriority { get; set; }
//        public int SearchPhase { get; set; }


//        public int TerrainTypeIndex
//        {
//            get { return terrainTypeIndex; }
//            set
//            {
//                if (terrainTypeIndex != value)
//                {
//                    terrainTypeIndex = value;
//                    ShaderData.RefreshTerrain(this);
//                }
//            }
//        }



//        public int Elevation
//        {
//            get { return elevation; }
//            set
//            {
//                if (elevation == value) return;

//                //视野阻挡
//                int originalViewElevation = ViewElevation;
//                elevation = value;
//                if (ViewElevation != originalViewElevation)
//                {
//                    ShaderData.ViewElevationChanged();
//                }
//                Vector3 position = center;
//                position.y = value * HexMetrics.elevationStep;
//                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;

//                center = position;

//                HexMapMgr.Instance.ValidateRivers(this);

//                for (int i = 0; i < roads.Length; i++)
//                {
//                    if (roads[i] && HexMapMgr.Instance.GetElevationDifference(this, (HexDirection)i) > 1)
//                    {
//                        HexMapMgr.Instance.SetRoad(this, i, false);
//                    }
//                }

//                HexMapMgr.Instance.Refresh(this);
//            }
//        }

//        public Vector3 Position { get { return center; } }

//        public bool HasIncomingRiver { get { return hasIncomingRiver; } set { hasIncomingRiver = value; } }

//        public bool HasOutgoingRiver { get { return hasOutgoingRiver; } set { hasOutgoingRiver = value; } }

//        public bool HasRiver { get { return hasIncomingRiver || hasOutgoingRiver; } }

//        public bool HasRiverBeginOrEnd { get { return hasIncomingRiver != hasOutgoingRiver; } }

//        public HexDirection IncomingRiver { get { return incomingRiver; } set { incomingRiver = value; } }

//        public HexDirection OutgoingRiver { get { return outgoingRiver; } set { outgoingRiver = value; } }

//        public float RiverSurfaceY { get { return (elevation + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep; } }

//        public float StreamBedY { get { return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep; } }


//        public HexDirection RiverBeginOrEndDirection { get { return hasIncomingRiver ? incomingRiver : outgoingRiver; } }

//        public bool HasRoads
//        {
//            get
//            {
//                for (int i = 0; i < roads.Length; i++)
//                {
//                    if (roads[i])
//                    {
//                        return true;
//                    }
//                }
//                return false;
//            }
//        }

//        public int WaterLevel
//        {
//            get { return waterLevel; }
//            set
//            {
//                if (waterLevel == value)
//                    return;

//                //视野阻挡
//                int originalViewElevation = ViewElevation;
//                waterLevel = value;
//                if (ViewElevation != originalViewElevation)
//                {
//                    ShaderData.ViewElevationChanged();
//                }
//                HexMapMgr.Instance.ValidateRivers(this);
//                HexMapMgr.Instance.Refresh(this);
//            }
//        }

//        public bool IsUnderwater { get { return waterLevel > elevation; } }


//        public float WaterSurfaceY
//        {
//            get { return (waterLevel + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep; }
//        }




//        public int SpecialIndex
//        {
//            get
//            {
//                return specialIndex;
//            }
//            set
//            {
//                if (specialIndex != value && !HasRiver)
//                {
//                    specialIndex = value;
//                    HexMapMgr.Instance.RemoveRoads(this);
//                    RefreshSelfOnly();
//                }
//            }
//        }

//        public bool IsSpecial { get { return specialIndex > 0; } }


//        public List<int> Features
//        {
//            get
//            {
//                return features;
//            }
//            set
//            {
//                if (features != value)
//                {
//                    features = value;
//                    RefreshSelfOnly();
//                }
//            }
//        }

//        public bool IsVisible { get { return visibility > 0; } }


//        public int ViewElevation { get { return elevation >= waterLevel ? elevation : waterLevel; } }

//    }
//}
