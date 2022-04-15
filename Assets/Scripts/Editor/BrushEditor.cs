using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
//using System.Diagnostics;
using System.Reflection;
using HexMap;
using System.Linq;

namespace WorldMapEditor
{
    public partial class HexMapEditor
    {
        void BrushCells(HexCell center)
        {
            int centerX = center.coordinates.X;
            int centerZ = center.coordinates.Z;


            int BrushSize = brushSize - 1;
            for (int r = 0, z = centerZ - BrushSize; z <= centerZ; z++, r++)
            {
                for (int x = centerX - r; x <= centerX + BrushSize; x++)
                {
                    BrushCell(HexMapMgr.Instance.GetCell(new HexCoordinates(x, z)));
                }
            }
            for (int r = 0, z = centerZ + BrushSize; z > centerZ; z--, r++)
            {
                for (int x = centerX - BrushSize; x <= centerX + r; x++)
                {
                    BrushCell(HexMapMgr.Instance.GetCell(new HexCoordinates(x, z)));
                }
            }
        }


        void BrushCell(HexCell cell)
        {
            if (this != Instance)
                return;
            if (cell == null)
                return;
            switch (BrushType)
            {
                case BrushType.Terrain | BrushType.Elevation:
                    cell.TerrainOpacity = opacity;
                    cell.TerrainTypeIndex = isErase ? 0 : terrain;
                    cell.Elevation = isErase ? 0 : Elevation;
                    break;

                case BrushType.Terrain:
                    cell.TerrainOpacity = opacity;
                    cell.TerrainTypeIndex = isErase ? 0 : terrain;
                    break;

                case BrushType.Elevation:
                    cell.Elevation = isErase ? 0 : Elevation;
                    break;

                case BrushType.River:
                    if (isErase)
                    {
                        cell.RemoveRiver();
                    }
                    else if (isDrag)
                    {
                        if (RiverMat == null)
                            return;
                        HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                        if (otherCell != null)
                        {
                            otherCell.SetOutgoingRiver(dragDirection);
                        }
                    }
                    break;
                case BrushType.Road:
                    if (isErase)
                    {
                        cell.RemoveRoads();
                    }
                    else if (isDrag)
                    {
                        if (RoadMat == null)
                            return;
                        HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                        if (otherCell != null)
                        {
                            otherCell.AddRoad(dragDirection);
                        }
                    }
                    break;
                case BrushType.Water:
                    cell.WaterLevel = isErase ? 0 : WaterLevel;
                    break;
                case BrushType.Special:
                    if (isFeatureErase)
                    {
                        cell.SpecialIndex = 0;
                        return;
                    }
                    //cell.SpecialIndex = isErase ? 0 : SpecialIndex;
                    Features Special = SpecialList.Find(s => s.use);
                    if (Special == null)
                        return;
                    if (Special.Feature == null)
                        return;
                    if (Special.Feature.asset == null)
                        return;
                    if (Special.Feature.asset.asset == null)
                        return;

                    int idSpecial = Special.Feature.asset.id;

                    if (IsFeatureRandomNumber)
                        idSpecial = idSpecial | HexMetrics.FeatureRandomNumberMask;
                    if (IsFeatureRandomDirecton)
                        idSpecial = idSpecial | HexMetrics.FeatureRandomDirectionMask;
                    cell.SpecialIndex = idSpecial;
                    break;

                case BrushType.Feature:
                    Type type = typeof(HexMapEditor);

                    for (int i = 0; i < FeatureList.Count; i++)
                    {
                        if (FeatureList[i].use)
                        {
                            if (FeatureList[i].Feature == null)
                                continue;
                            if (FeatureList[i].Feature.asset == null)
                                continue;
                            if (FeatureList[i].Feature.asset.asset == null)
                                continue;

                            int id = FeatureList[i].Feature.asset.id;
                            id = id & 0x0FFFF;
                            int feature = cell.features.Find(f => (f & HexMetrics.FeatureMask) == id);
                            if (feature > 0)
                            {
                                cell.features.Remove(feature);
                            }
                            if (isFeatureErase)
                                continue;

                            if (IsFeatureRandomNumber)
                                id = id | HexMetrics.FeatureRandomNumberMask;
                            if (IsFeatureRandomDirecton)
                                id = id | HexMetrics.FeatureRandomDirectionMask;
                            cell.features.Add(id);
                        }

                    }
                    cell.RefreshFeature();
                    break;
                default:
                    cell.TerrainTypeIndex = 0;
                    cell.Elevation = 0;
                    cell.WaterLevel = 0;
                    cell.RemoveRiver();
                    cell.RemoveRoads();
                    break;
            }
        }

        private void UpdateBrush()
        {
            switch (Instance.UType)
            {
                case UpdateType.ReCreate:
                    Create();
                    if ((BrushType & BrushType.Terrain) > 0 || ((BrushType & BrushType.Elevation) > 0))
                    {
                        Instance.UType = UpdateType.Brush;
                    }
                    else
                    {
                        Instance.UType = UpdateType.None;
                    }
                    break;
                case UpdateType.Brush:
                    Event currentEvent = Event.current;
                    if (currentEvent != null && (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) && currentEvent.button == 0)
                    {
                        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            HexCell currentCell = HexMapMgr.Instance.GetCell(hit.point);
                            if (previousCell != null && previousCell != currentCell)
                            {
                                ValidateDrag(currentCell);
                            }
                            else
                            {
                                isDrag = false;
                            }

                            BrushCells(currentCell);
                            previousCell = currentCell;
                        }
                        else
                        {
                            previousCell = null;
                        }
                    }
                    break;
            }
        }


        public void OnValueChanged()
        {
            if (!hasTextureArray)
            {
                return;
            }
            changed = true;
            UType = UpdateType.ReCreate;

            cellCountX = chunkCountX * HexMetrics.chunkSizeX;
            cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
            cellCount = cellCountX * cellCountZ;
            ReCreate();
        }

        public void OnBrushChanged(string name)
        {
            Type type = typeof(HexMapEditor);
            FieldInfo Info = type.GetField(name);
            FieldInfo[] Infos = type.GetFields();

            string Name = Info.Name;

            if (Name != "isErase")
            {

                if (Info.GetValue(Instance) is bool b && b == true)
                {
                    BrushType = Name switch
                    {
                        "IsTerrain" => BrushType.Terrain,
                        "IsElevation" => BrushType.Elevation,
                        "IsRiver" => BrushType.River,
                        "IsRoad" => BrushType.Road,
                        "IsWater" => BrushType.Water,
                        "IsSpecialBrush" => BrushType.Special,
                        "IsFeatureBrush" => BrushType.Feature,
                        _ => default
                    };
                    BrushAttribute attr = Attribute.GetCustomAttribute(Info, typeof(BrushAttribute)) as BrushAttribute;
                    if (attr != null)
                    {
                        for (int i = 0; i < Infos.Length; i++)
                        {
                            BrushAttribute a = Attribute.GetCustomAttribute(Infos[i], typeof(BrushAttribute)) as BrushAttribute;
                            if (a != null)
                            {
                                if (a.group != attr.group)
                                {
                                    if (Infos[i].GetValue(this) is bool)
                                    {
                                        Infos[i].SetValue(this, false);
                                    }
                                }
                                else
                                {
                                    if (Infos[i].GetValue(this) is bool bb && bb)
                                    {
                                        Name = Infos[i].Name;//.TrimEnd('0').TrimEnd('1').TrimEnd('2').TrimEnd('3').TrimEnd('4').TrimEnd('5');
                                        BrushType = Name switch
                                        {
                                            "IsTerrain" => BrushType | BrushType.Terrain,
                                            "IsElevation" => BrushType | BrushType.Elevation,
                                            "IsRiver" => BrushType | BrushType.River,
                                            "IsRoad" => BrushType | BrushType.Road,
                                            "IsWater" => BrushType | BrushType.Water,
                                            "IsSpecialBrush" => BrushType | BrushType.Special,
                                            "IsFeatureBrush" => BrushType.Feature,
                                            _ => default
                                        };
                                    }

                                }
                            }
                        }
                    }
                }
                else
                {
                    Name = Info.Name;//.TrimEnd('0').TrimEnd('1').TrimEnd('2').TrimEnd('3').TrimEnd('4').TrimEnd('5');
                    BrushType = Name switch
                    {
                        "IsTerrain" => BrushType & ~BrushType.Terrain,
                        "IsElevation" => BrushType & ~BrushType.Elevation,
                        "IsRiver" => BrushType.None,
                        "IsRoad" => BrushType.None,
                        "IsWater" => BrushType.None,
                        "IsFeatureBrush" => BrushType.None,
                        "IsSpecialBrush" => BrushType.None,
                        _ => default
                    };

                }
            }
            Instance.UType = UpdateType.None;
            for (int i = 0; i < Infos.Length; i++)
            {
                if (Attribute.IsDefined(Infos[i], typeof(BrushAttribute)))
                {
                    if (Infos[i].GetValue(this) is bool bo && bo == true)
                    {
                        Instance.UType = UpdateType.Brush;
                    }
                }
            }

            if (isErase)
                Instance.UType = UpdateType.Brush;
        }

        public void OnTerrainChanged() => OnBrushChanged("IsTerrain");
        public void OnElevationChanged() => OnBrushChanged("IsElevation");
        public void OnRiverChanged() => OnBrushChanged("IsRiver");
        public void OnRoadChanged() => OnBrushChanged("IsRoad");
        public void OnWaterChanged() => OnBrushChanged("IsWater");
        //public void OnBridgeChanged() => OnBrushChanged("IsBridge");
        public void OnSpecialChanged() => OnBrushChanged("IsSpecialBrush");
        public void OnFeatureChanged() => OnBrushChanged("IsFeatureBrush");
        public void OnRiverTypeChanged() { }
        public void OnRoadTypeChanged() { }


        public void OnTerrainListChanged(int id)
        {

            int Count = TerrainList.Count(t => t.use);
            TerrainType Terrain = TerrainList.Find(t => t.id == id);
            if (Terrain == null)
                return;
            if (Count > 4)
            {
                Terrain.use = false;
                return;
            }
            terrain = 0;
            byte[] bytes = new byte[4];
            for (int i = 0; i < TerrainList.Count; i++)
            {
                if (TerrainList[i].use)
                {
                    terrain = (terrain | 1 << i);
                    bytes[i] = (byte)(TerrainList[i].opacity * 255);
                }
            }
            opacity = BitConverter.ToInt32(bytes, 0);

            //terrain = id;
            //for (int i = 0; i < TerrainList.Count; i++)
            //{
            //    if (terrain == TerrainList[i].id)
            //        TerrainList[i].use = TerrainList[i].use;
            //    else
            //        TerrainList[i].use = false;

        }

        public void OnOpacityChanged(int id)
        {
            byte[] bytes = new byte[4];
            for (byte i = 0; i < TerrainList.Count; i++)
            {
                if (TerrainList[i].use)
                    bytes[i] = (byte)(TerrainList[i].opacity * 255);
            }
            opacity = BitConverter.ToInt32(bytes, 0);
        }


        public void OnEraseChanged()
        {
            OnBrushChanged("isErase");
        }

        public void OnBrushSizeChanged()
        {
            brushSize = Mathf.Max(brushSize, 1);
            brushSize = Mathf.Max(brushSize, 1);
        }


        public void OnRiverMatChanged()
        {
            for (int i = 0; i < HexMapMgr.Instance.chunks.Length; i++)
            {
                MeshRenderer mr = HexMapMgr.Instance.chunks[i].chunk.Rivers.gameObject.GetComponent<MeshRenderer>();
                if (mr == null)
                    mr = HexMapMgr.Instance.chunks[i].chunk.Rivers.gameObject.AddComponent<MeshRenderer>();
                mr.material = RiverMat;
            }
        }


        public void OnRoadMatChanged()
        {
            for (int i = 0; i < HexMapMgr.Instance.chunks.Length; i++)
            {
                MeshRenderer mr = HexMapMgr.Instance.chunks[i].chunk.Roads.gameObject.GetComponent<MeshRenderer>();
                if (mr == null)
                    mr = HexMapMgr.Instance.chunks[i].chunk.Roads.gameObject.AddComponent<MeshRenderer>();
                mr.material = RoadMat;
            }

        }

        public void OnWaterMatChanged()
        {
            for (int i = 0; i < HexMapMgr.Instance.chunks.Length; i++)
            {
                MeshRenderer mr = HexMapMgr.Instance.chunks[i].chunk.Water.gameObject.GetComponent<MeshRenderer>();
                if (mr == null)
                    mr = HexMapMgr.Instance.chunks[i].chunk.Water.gameObject.AddComponent<MeshRenderer>();
                mr.material = WaterMat;
            }

        }

        public void OnWaterShoreMatChanged()
        {
            for (int i = 0; i < HexMapMgr.Instance.chunks.Length; i++)
            {
                MeshRenderer mr = HexMapMgr.Instance.chunks[i].chunk.WaterShore.gameObject.GetComponent<MeshRenderer>();
                if (mr == null)
                    mr = HexMapMgr.Instance.chunks[i].chunk.WaterShore.gameObject.AddComponent<MeshRenderer>();
                mr.material = WaterShoreMat;
            }
        }


        void ValidateDrag(HexCell currentCell)
        {
            for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++)
            {
                if (previousCell.GetNeighbor(dragDirection) == currentCell)
                {
                    isDrag = true;
                    return;
                }
            }
            isDrag = false;
        }
    }
}