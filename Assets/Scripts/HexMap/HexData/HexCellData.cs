using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HexMap
{

    [Serializable]
    public partial class HexCell
    {
        public Vector2Int chunkCoordinate;
        [JsonIgnore]
        //[NonSerialized]
        public HexChunkMgr ChunkMgr;

        [SerializeField]
        public int id;
        public Vector3 center;
        [SerializeField]
        public HexCoordinates coordinates;

        [JsonIgnore]
        public Hexagon hexagon;
        [SerializeField]
        public Vector2Int xz;

        [NonSerialized]
        [JsonIgnore]
        public HexCell[] neighbors;

        [SerializeField]
        int terrainTypeIndex;
        [SerializeField]
        int terrainOpacity;
        [SerializeField]
        public int terrainCost;

        [SerializeField]
        int elevation = int.MinValue;
        [SerializeField]
        int waterLevel;
        [SerializeField]
        int specialIndex;
        public List<int> features;
        public int featureCost;

        public bool[] roads;//道路朝向
        [SerializeField]
        int road;//道路纹理索引
        [SerializeField]
        int roadOpacity;//道路纹理透明度
        [SerializeField]
        float roadWidthIF;//道路宽度系数
        [SerializeField]
        float roadNoiseIF;//道路噪声系数
        //public RoadNoiseType roadNoiseType;

        int distance;

        [SerializeField]
        bool hasIncomingRiver;
        [SerializeField]
        bool hasOutgoingRiver;
        HexDirection incomingRiver;
        HexDirection outgoingRiver;

        int visibility = 0;//战争迷雾
        bool explored;//探索

        public bool IsExplored
        {
            get { return explored && Explorable; }
            private set { explored = value; }
        }
        public bool Explorable { get; set; }//视野阻挡

        //public HexCellShaderData ShaderData { get; set; }
        public int Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        [JsonIgnore]
        public HexCell PathFrom { get; set; }
        [JsonIgnore]
        public int SearchHeuristic { get; set; }
        [JsonIgnore]
        public int SearchPriority { get { return distance + SearchHeuristic; } }
        [JsonIgnore]
        public HexCell NextWithSamePriority { get; set; }
        public int SearchPhase { get; set; }


        public int TerrainTypeIndex
        {
            get { return terrainTypeIndex; }
            set
            {
                if (terrainTypeIndex != value)
                {
                    terrainTypeIndex = value;
                    //ShaderData.RefreshTerrain(this);
                }
            }
        }

        public int Road { get { return road; } set { road = value; } }

        public float RoadWidthIF { get { return roadWidthIF; } set { roadWidthIF = value; } }

        public float RoadNoiseIF { get { return roadNoiseIF; } set { roadNoiseIF = value; } }

        public int TerrainOpacity { get { return terrainOpacity; } set { terrainOpacity = value; } }

        public int RoadOpacity { get { return roadOpacity; } set { roadOpacity = value; } }



        public int Elevation
        {
            get { return elevation; }
            set
            {
                if (elevation == value) return;

                //视野阻挡
                int originalViewElevation = ViewElevation;
                elevation = value;
                if (ViewElevation != originalViewElevation)
                {
                    //ShaderData.ViewElevationChanged();
                }
                RefreshPosition();
                ValidateRivers();

                for (int i = 0; i < roads.Length; i++)
                {
                    if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                    {
                        SetRoad(i, false);
                    }
                }

                Refresh();
            }
        }

        public Vector3 Position { get { return center; } }

        public bool HasIncomingRiver { get { return hasIncomingRiver; } }

        public bool HasOutgoingRiver { get { return hasOutgoingRiver; } }

        public bool HasRiver { get { return hasIncomingRiver || hasOutgoingRiver; } }

        public bool HasRiverBeginOrEnd { get { return hasIncomingRiver != hasOutgoingRiver; } }

        public HexDirection IncomingRiver { get { return incomingRiver; } }

        public HexDirection OutgoingRiver { get { return outgoingRiver; } }

        public float RiverSurfaceY { get { return (elevation + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep; } }

        public float StreamBedY { get { return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep; } }


        public HexDirection RiverBeginOrEndDirection { get { return hasIncomingRiver ? incomingRiver : outgoingRiver; } }

        public bool HasRoads
        {
            get
            {
                for (int i = 0; i < roads.Length; i++)
                {
                    if (roads[i])
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int WaterLevel
        {
            get { return waterLevel; }
            set
            {
                if (waterLevel == value)
                    return;

                //视野阻挡
                int originalViewElevation = ViewElevation;
                waterLevel = value;
                if (ViewElevation != originalViewElevation)
                {
                    //ShaderData.ViewElevationChanged();
                    //HexCellShader.ViewElevationChanged();
                }
                ValidateRivers();
                Refresh();
            }
        }

        public bool IsUnderwater { get { return waterLevel > elevation; } }


        public float WaterSurfaceY
        {
            get { return (waterLevel + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep; }
        }




        public int SpecialIndex
        {
            get
            {
                return specialIndex;
            }
            set
            {
                if (specialIndex != value && !HasRiver)
                {
                    specialIndex = value;
                    RemoveRoads();
                    RefreshSelfOnly();
                }
            }
        }

        public bool IsSpecial { get { return specialIndex > 0; } }


        public List<int> Features
        {
            get
            {
                return features;
            }
            set
            {
                if (features != value)
                {
                    features = value;
                    RefreshSelfOnly();
                }
            }
        }

        public bool IsVisible { get { return visibility > 0; } }


        public int ViewElevation { get { return elevation >= waterLevel ? elevation : waterLevel; } }

    }
}
