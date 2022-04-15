//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;
//using UnityEngine.UI;

//namespace HexMap
//{
//    public partial class HexCell
//    {


//        public HexCell GetNeighbor(HexDirection direction)
//        {
//            return HexMapMgr.Instance.GetNeighbor(this, direction);
//        }
//        public HexCell GetNeighbor(int i)
//        {
//            return HexMapMgr.Instance.GetNeighbor(this, i);
//        }

//        public void SetNeighbor(HexDirection direction, int id)
//        {
//             HexMapMgr.Instance.SetNeighbor(this, direction, id);
//        }

//        public HexEdgeType GetEdgeType(HexCell otherCell)
//        {
//            return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
//        }

//        public HexEdgeType GetEdgeType(HexDirection direction)
//        {
//            return HexMapMgr.Instance.GetEdgeType(this, direction);
//        }


//        public bool HasRiverThroughEdge(HexDirection direction)
//        {
//            return
//                hasIncomingRiver && incomingRiver == direction ||
//                hasOutgoingRiver && outgoingRiver == direction;
//        }



//        public bool HasRoadThroughEdge(HexDirection direction)
//        {
//            return roads[(int)direction];
//        }





//        public bool IsValidRiverDestination(HexCell neighbor)
//        {
//            return neighbor != null && (elevation >= neighbor.elevation || waterLevel == neighbor.elevation);
//        }


//        public void IncreaseVisibility()
//        {
//            visibility += 1;
//            if (visibility == 1)
//            {
//                IsExplored = true;
//                ShaderData.RefreshVisibility(this);
//            }
//        }

//        public void DecreaseVisibility()
//        {
//            visibility -= 1;
//            if (visibility == 0)
//            {
//                ShaderData.RefreshVisibility(this);
//            }
//        }



//        public void RefreshSelfOnly()
//        {
//#if UNITY_EDITOR
//            ChunkMgr.Refresh();
//#endif
//        }


//        public void RefreshPosition()
//        {
//            center.y = elevation * HexMetrics.elevationStep;
//            center.y += (HexMetrics.SampleNoise(center).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;

//#if UNITY_EDITOR
//            if (uiRect == null)
//                return;
//            Vector3 uiPosition = uiRect.localPosition;
//            uiPosition.z = -center.y;
//            uiRect.localPosition = uiPosition;
//#endif

//        }


//        public void Export(BinaryWriter writer)
//        {
//            writer.Write((byte)id);

//            writer.Write(xz.x);
//            writer.Write(xz.y);

//            writer.Write((byte)terrainTypeIndex);

//            writer.Write((byte)elevation);
//            writer.Write((byte)waterLevel);
//            writer.Write((byte)features.Count);
//            for (int i = 0; i < features.Count; i++)
//            {
//                writer.Write(features[i]);
//            }
//            writer.Write((byte)specialIndex);

//            if (hasIncomingRiver)
//            {
//                writer.Write((byte)(incomingRiver + 128));
//            }
//            else
//            {
//                writer.Write((byte)0);
//            }

//            if (hasOutgoingRiver)
//            {
//                writer.Write((byte)(outgoingRiver + 128));
//            }
//            else
//            {
//                writer.Write((byte)0);
//            }

//            int roadFlags = 0;
//            for (int i = 0; i < roads.Length; i++)
//            {
//                if (roads[i])
//                {
//                    roadFlags |= 1 << i;
//                }
//            }
//            writer.Write((byte)roadFlags);
//            writer.Write(IsExplored);
//        }

//        public void Import(BinaryReader reader, int version = 0)
//        {
//            id = reader.ReadByte();

//            int x = reader.ReadInt32();
//            int z = reader.ReadInt32();
//            xz = new Vector2Int(x, z);
//            coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

//            terrainTypeIndex = reader.ReadByte();
//            ShaderData.RefreshTerrain(this);

//            elevation = reader.ReadByte();
//            RefreshPosition();
//            waterLevel = reader.ReadByte();
//            int Count = reader.ReadByte();
//            for (int i = 0; i < Count; i++)
//            {
//                features.Add(reader.ReadInt32());
//            }
//            specialIndex = reader.ReadByte();

//            byte riverData = reader.ReadByte();
//            if (riverData >= 128)
//            {
//                hasIncomingRiver = true;
//                incomingRiver = (HexDirection)(riverData - 128);
//            }
//            else
//            {
//                hasIncomingRiver = false;
//            }

//            riverData = reader.ReadByte();
//            if (riverData >= 128)
//            {
//                hasOutgoingRiver = true;
//                outgoingRiver = (HexDirection)(riverData - 128);
//            }
//            else
//            {
//                hasOutgoingRiver = false;
//            }

//            int roadFlags = reader.ReadByte();
//            for (int i = 0; i < roads.Length; i++)
//            {
//                roads[i] = (roadFlags & (1 << i)) != 0;
//            }

//            IsExplored = reader.ReadBoolean();
//            ShaderData.RefreshVisibility(this);
//        }


//        public void ResetVisibility()
//        {
//            if (visibility != 0)
//            {
//                visibility = 0;
//                ShaderData.RefreshVisibility(this);
//            }
//        }


//#if UNITY_EDITOR






//        public RectTransform uiRect;
//        Text label;

//        public void EnableHighlight(Color color)
//        {
//            if (uiRect == null)
//                return;
//            Image highlight = uiRect.GetChild(0).GetComponent<Image>();
//            highlight.color = color;
//            highlight.enabled = true;
//        }

//        public void DisableHighlight()
//        {
//            if (uiRect == null)
//                return;
//            Image highlight = uiRect.GetChild(0).GetComponent<Image>();
//            highlight.enabled = false;
//        }

//        public void SetLabel(string text)
//        {
//            if (uiRect == null)
//                return;
//            if (label == null)
//                label = uiRect.GetComponent<Text>();

//            label.text = text;
//        }

//        public void SetCoordinateLabel()
//        {
//            if (label == null)
//                label = uiRect.GetComponent<Text>();

//            label.fontSize = 3;
//            label.text = coordinates.ToStringOnSpearateLines();
//        }

//        public void SetDistanceLabel()
//        {
//            if (label == null)
//                label = uiRect.GetComponent<Text>();

//            label.fontSize = 6;
//            label.text = distance == int.MaxValue ? "" : distance.ToString();
//        }

//        public void ClearLabel()
//        {
//            if (uiRect == null)
//                return;
//            if (label == null)
//                label = uiRect.GetComponent<Text>();
//            label.text = "";
//        }

//        public void RefreshFeature()
//        {
//            RefreshSelfOnly();
//        }
//#endif
//    }
//}
