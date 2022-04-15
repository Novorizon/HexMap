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
        [JsonIgnore] public HexChunkMgr ChunkMgr;

        public int id;
        public Vector3 center;
        public HexCoordinates coordinates;
        public Vector2Int xz;

        [NonSerialized]
        HexCell[] neighbors;
        public bool[] roads;

        //[HexMap] public Color color;
        int elevation = int.MinValue;
        int waterLevel;
        int specialIndex;
        public List<int> features;
        int terrainTypeIndex;
        int distance;
        int terrainOpacity;


        bool hasIncomingRiver;
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

        public HexCellShaderData ShaderData { get; set; }
        public int Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        public HexCell PathFrom { get; set; }
        public int SearchHeuristic { get; set; }
        public int SearchPriority { get { return distance + SearchHeuristic; } }
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
                    ShaderData.RefreshTerrain(this);
                }
            }
        }


        public int TerrainOpacity
        {
            get { return terrainOpacity; }
            set
            {
                if (terrainOpacity != value)
                {
                    terrainOpacity = value;
                    ShaderData.RefreshTerrain(this);
                }
            }
        }



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
                    ShaderData.ViewElevationChanged();
                    //HexCellShader.ViewElevationChanged();
                }
                Vector3 position = center;
                position.y = value * HexMetrics.elevationStep;
                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;

                center = position;

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
                    ShaderData.ViewElevationChanged();
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
