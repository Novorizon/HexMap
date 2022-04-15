using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

namespace HexMap
{
    public partial class HexCell
    {
        public HexCell()
        {
            //terrainTypeIndex = 1;
        }

        public void Init(int i, int x, int z)
        {
            id = i;
            coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            roads = new bool[6];
            terrainTypeIndex = 1;
            terrainOpacity = 255;
            Elevation = 0;
            features = new List<int>();
            neighbors = new HexCell[6];
        }

        public HexCell GetNeighbor(HexDirection direction)
        {
            if (direction < HexDirection.NE || direction > HexDirection.NW)
                return null;
            return neighbors[(int)direction];
        }

        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            if (direction < HexDirection.NE || direction > HexDirection.NW)
                return;
            neighbors[(int)direction] = cell;
            cell.neighbors[(int)direction.Opposite()] = this;
        }

        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
        }

        public HexEdgeType GetEdgeType(HexCell otherCell)
        {
            return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
        }


        public bool HasRiverThroughEdge(HexDirection direction)
        {
            return hasIncomingRiver && incomingRiver == direction || hasOutgoingRiver && outgoingRiver == direction;
        }

        public void RemoveIncomingRiver()
        {
            if (!hasIncomingRiver)
            {
                return;
            }
            hasIncomingRiver = false;
            RefreshSelfOnly();

            HexCell neighbor = GetNeighbor(incomingRiver);
            neighbor.hasOutgoingRiver = false;
            neighbor.RefreshSelfOnly();
        }

        public void RemoveOutgoingRiver()
        {
            if (!hasOutgoingRiver)
            {
                return;
            }
            hasOutgoingRiver = false;
            RefreshSelfOnly();

            HexCell neighbor = GetNeighbor(outgoingRiver);
            neighbor.hasIncomingRiver = false;
            neighbor.RefreshSelfOnly();
        }

        public void RemoveRiver()
        {
            RemoveOutgoingRiver();
            RemoveIncomingRiver();
        }

        public void SetOutgoingRiver(HexDirection direction)
        {
            if (hasOutgoingRiver && outgoingRiver == direction)
            {
                return;
            }

            HexCell neighbor = GetNeighbor(direction);
            if (!IsValidRiverDestination(neighbor))
            {
                return;
            }

            RemoveOutgoingRiver();
            if (hasIncomingRiver && incomingRiver == direction)
            {
                RemoveIncomingRiver();
            }
            hasOutgoingRiver = true;
            outgoingRiver = direction;
            specialIndex = 0;

            neighbor.RemoveIncomingRiver();
            neighbor.hasIncomingRiver = true;
            neighbor.incomingRiver = direction.Opposite();
            neighbor.specialIndex = 0;


            SetRoad((int)direction, false);
        }

        public bool HasRoadThroughEdge(HexDirection direction)
        {
            return roads[(int)direction];
        }

        public void AddRoad(HexDirection direction)
        {
            if (!roads[(int)direction]
                && !HasRiverThroughEdge(direction)
                && !IsSpecial
                && !GetNeighbor(direction).IsSpecial
                && GetElevationDifference(direction) <= 1)
            {
                SetRoad((int)direction, true);
            }
        }

        public void RemoveRoads()
        {
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (roads[i])
                {
                    SetRoad(i, false);
                }
            }
        }

        public int GetElevationDifference(HexDirection direction)
        {
            int difference = elevation - GetNeighbor(direction).elevation;
            return difference >= 0 ? difference : -difference;
        }

        void SetRoad(int index, bool state)
        {
            roads[index] = state;
            neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
            neighbors[index].RefreshSelfOnly();
            RefreshSelfOnly();
        }

        bool IsValidRiverDestination(HexCell neighbor)
        {
            return neighbor != null && (elevation >= neighbor.elevation || waterLevel == neighbor.elevation);
        }

        void ValidateRivers()
        {
            if (hasOutgoingRiver && !IsValidRiverDestination(GetNeighbor(outgoingRiver)))
            {
                RemoveOutgoingRiver();
            }
            if (hasIncomingRiver && !GetNeighbor(incomingRiver).IsValidRiverDestination(this))
            {
                RemoveIncomingRiver();
            }
        }


        public void IncreaseVisibility()
        {
            visibility += 1;
            if (visibility == 1)
            {
                IsExplored = true;
                ShaderData.RefreshVisibility(this);
            }
        }

        public void DecreaseVisibility()
        {
            visibility -= 1;
            if (visibility == 0)
            {
                ShaderData.RefreshVisibility(this);
            }
        }


        public void Refresh()
        {
            if (ChunkMgr != null)
            {
                ChunkMgr.Refresh();
                for (int i = 0; i < neighbors.Length; i++)
                {
                    HexCell neighbor = neighbors[i];
                    if (neighbor != null && neighbor.ChunkMgr != ChunkMgr)
                    {
                        neighbor.ChunkMgr.Refresh();
                    }
                }
            }
        }

        void RefreshSelfOnly()
        {
#if UNITY_EDITOR
            ChunkMgr.Refresh();
#endif
        }


        public void RefreshPosition()
        {
            center.y = elevation * HexMetrics.elevationStep;
            float noise = Mathf.Max(0, HexMetrics.SampleNoise(center).y * 2f - 1f);
            center.y += noise * HexMetrics.elevationPerturbStrength;

#if UNITY_EDITOR
            if (uiRect == null)
                return;
            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -center.y;
            uiRect.localPosition = uiPosition;
#endif

        }


        public void Export(BinaryWriter writer)
        {
            writer.Write(id);

            writer.Write(xz.x);
            writer.Write(xz.y);

            //writer.Write((byte)terrainTypeIndex);
            //writer.Write(terrainOpacity);

            writer.Write((byte)elevation);
            writer.Write((byte)waterLevel);
            writer.Write((byte)features.Count);
            for (int i = 0; i < features.Count; i++)
            {
                writer.Write(features[i]);
            }
            writer.Write((byte)specialIndex);

            if (hasIncomingRiver)
            {
                writer.Write((byte)(incomingRiver + 128));
            }
            else
            {
                writer.Write((byte)0);
            }

            if (hasOutgoingRiver)
            {
                writer.Write((byte)(outgoingRiver + 128));
            }
            else
            {
                writer.Write((byte)0);
            }

            int roadFlags = 0;
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    roadFlags |= 1 << i;
                }
            }
            writer.Write((byte)roadFlags);
            //writer.Write(IsExplored);
        }

        public void Import(BinaryReader reader, int version = 0)
        {
            id = reader.ReadInt32();

            int x = reader.ReadInt32();
            int z = reader.ReadInt32();
            xz = new Vector2Int(x, z);
            coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

            //terrainTypeIndex = reader.ReadByte();
            //terrainOpacity = reader.ReadInt32();
            //ShaderData.RefreshTerrain(this);

            elevation = reader.ReadByte();
            RefreshPosition();
            waterLevel = reader.ReadByte();
            int Count = reader.ReadByte();
            for (int i = 0; i < Count; i++)
            {
                features.Add(reader.ReadInt32());
            }
            specialIndex = reader.ReadByte();

            byte riverData = reader.ReadByte();
            if (riverData >= 128)
            {
                hasIncomingRiver = true;
                incomingRiver = (HexDirection)(riverData - 128);
            }
            else
            {
                hasIncomingRiver = false;
            }

            riverData = reader.ReadByte();
            if (riverData >= 128)
            {
                hasOutgoingRiver = true;
                outgoingRiver = (HexDirection)(riverData - 128);
            }
            else
            {
                hasOutgoingRiver = false;
            }

            int roadFlags = reader.ReadByte();
            for (int i = 0; i < roads.Length; i++)
            {
                roads[i] = (roadFlags & (1 << i)) != 0;
            }

            //IsExplored = reader.ReadBoolean();
            //ShaderData.RefreshVisibility(this);
            //HexCellShader.RefreshVisibility(this);

            neighbors = new HexCell[6];
        }


        public void ResetVisibility()
        {
            if (visibility != 0)
            {
                visibility = 0;
                ShaderData.RefreshVisibility(this);
                //HexCellShader.RefreshVisibility(this);
            }
        }


#if UNITY_EDITOR
        public RectTransform uiRect;
        Text label;

        public void EnableHighlight(Color color)
        {
            if (uiRect == null)
                return;
            Image highlight = uiRect.GetChild(0).GetComponent<Image>();
            highlight.color = color;
            highlight.enabled = true;
        }

        public void DisableHighlight()
        {
            if (uiRect == null)
                return;
            Image highlight = uiRect.GetChild(0).GetComponent<Image>();
            highlight.enabled = false;
        }

        public void SetLabel(string text)
        {
            if (uiRect == null)
                return;
            if (label == null)
                label = uiRect.GetComponent<Text>();

            label.text = text;
        }

        public void SetCoordinateLabel()
        {
            if (uiRect == null)
                return;
            if (label == null)
                label = uiRect.GetComponent<Text>();

            label.fontSize = 3;
            label.text = coordinates.ToStringOnSpearateLines();
        }

        public void SetDistanceLabel()
        {
            if (uiRect == null)
                return;
            if (label == null)
                label = uiRect.GetComponent<Text>();

            label.fontSize = 6;
            label.text = distance == int.MaxValue ? "" : distance.ToString();
        }

        public void ClearLabel()
        {
            if (uiRect == null)
                return;
            if (label == null)
                label = uiRect.GetComponent<Text>();
            label.text = "";
        }

        public void RefreshFeature()
        {
            RefreshSelfOnly();
        }
#endif
    }
}
