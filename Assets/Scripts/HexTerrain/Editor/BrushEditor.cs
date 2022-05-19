using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using HexMap;
using System.Linq;

public partial class HexTerrain
{
#if UNITY_EDITOR
    void InitBrush()
    {
        //mode = EditorMode.Brush;
        IsTerrain = false;
        IsElevation = false;
        IsWater = false;
        IsRiver = false;
        IsRoad = false;
        IsFeatureRandomNumber = false;
        IsFeatureRandomDirecton = false;
        IsFeature = false;
        OnEditorModeChanged();
    }


    private void UpdateBrush()
    {
        switch (UType)
        {
            case UpdateType.ReCreate:
                if ((BrushType & BrushType.Terrain) > 0 || ((BrushType & BrushType.Elevation) > 0))
                {
                    UType = UpdateType.Brush;
                }
                else
                {
                    UType = UpdateType.None;
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
                            if (currentEvent.type == EventType.MouseDrag)
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


    void BrushCells(HexCell center)
    {
        //int centerX = center.coordinates.X;
        //int centerZ = center.coordinates.Z;
        int centerX = center.hexagon.q;
        int centerZ = center.hexagon.s;


        int BrushSize = brushSize - 1;
        for (int r = 0, z = centerZ - BrushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + BrushSize; x++)
            {
                BrushCell(HexMapMgr.Instance.GetCell(new Hexagon(x, z)));
            }
        }
        for (int r = 0, z = centerZ + BrushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - BrushSize; x <= centerX + r; x++)
            {
                BrushCell(HexMapMgr.Instance.GetCell(new Hexagon(x, z)));
            }
        }
    }


    void BrushCell(HexCell cell)
    {
        if (cell == null)
            return;
        switch (BrushType)
        {
            case BrushType.Terrain | BrushType.Elevation:
                cell.TerrainOpacity = opacity;
                cell.TerrainTypeIndex = isErase ? 0 : terrain;
                cell.Elevation = isErase ? 0 : Elevation;
                HexMapMgr.Instance.RefreshTerrain(cell);
                cell.terrainCost = terrainCost;
                break;

            case BrushType.Terrain:
                cell.TerrainOpacity = opacity;
                cell.TerrainTypeIndex = isErase ? 0 : terrain;

                if (isErase || terrain > 0)
                    HexMapMgr.Instance.RefreshTerrain(cell);
                cell.terrainCost = terrainCost;
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
                cell.Road = isErase ? 0 : road;
                cell.RoadOpacity = opacity;
                if (isErase)
                {
                    cell.RemoveRoads();
                }
                else if (isDrag)
                {
                    cell.RoadWidthIF = roadWidth;
                    cell.RoadNoiseIF = roadNoiseIF;
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
                idSpecial = idSpecial | HexMetrics.FeatureTypeSpecialMask;
                cell.SpecialIndex = idSpecial;
                cell.featureCost = Special.cost;
                break;

            case BrushType.Feature:
                Type type = typeof(HexTerrain);
                cell.featureCost = 0;
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
                        id = id | HexMetrics.FeatureTypeNormalMask;
                        cell.features.Add(id);
                        cell.featureCost += FeatureList[i].cost;
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


    public void OnBrushChanged(string name)
    {
        Type type = typeof(HexTerrain);
        FieldInfo Info = type.GetField(name);
        FieldInfo[] Infos = type.GetFields();

        string Name = Info.Name;

        if (Name != "isErase")
        {

            if (Info.GetValue(this) is bool b && b == true)
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
        UType = UpdateType.None;
        for (int i = 0; i < Infos.Length; i++)
        {
            if (Attribute.IsDefined(Infos[i], typeof(BrushAttribute)))
            {
                if (Infos[i].GetValue(this) is bool bo && bo == true)
                {
                    UType = UpdateType.Brush;
                }
            }
        }

        if (isErase)
            UType = UpdateType.Brush;
    }

    public void ResetType()
    {
        for (int i = 0; i < textures.Count; i++)
        {
            textures[i].use = false;
        }
    }

    public void OnTerrainChanged()
    {
        OnBrushChanged("IsTerrain");
        OnTerrainTypeChanged();
    }
    public void OnRoadChanged()
    {
        OnBrushChanged("IsRoad");
        ResetType();
        //OnTerrainTypeChanged();
    }

    public void OnElevationChanged() => OnBrushChanged("IsElevation");
    public void OnRiverChanged() => OnBrushChanged("IsRiver");
    public void OnWaterChanged() => OnBrushChanged("IsWater");
    public void OnSpecialChanged() => OnBrushChanged("IsSpecialBrush");
    public void OnFeatureChanged() => OnBrushChanged("IsFeatureBrush");


    public void OnTerrainTypeChanged(int id)
    {
        int Count = textures.Count(t => t.use);
        TerrainTexture Terrain = textures.Find(t => t.id == id);
        if (Terrain == null)
            return;

        if (IsRoad)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i].id != id)
                {
                    textures[i].use = false;
                }
            }
        }
        else
        {
            if (Count > 4)
            {
                Terrain.use = false;
                return;
            }
        }
        OnTerrainTypeChanged();

    }


    public void OnTerrainTypeChanged()
    {
        if (IsRoad)
        {
            road = 0;
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i].use)
                {
                    road = 1 << i;
                    opacity = (int)(textures[i].opacity * 255);
                }
            }

        }
        else
        {
            terrain = 0;
            terrainCost = 0;
            byte[] bytes = new byte[4];
            int total = 0;
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i].use)
                {
                    terrainCost = terrainCost + textures[i].cost;
                    terrain = (terrain | 1 << i);
                    bytes[i] = (byte)(textures[i].opacity * 255);
                    total = total + bytes[i];
                }
            }
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)((float)bytes[i] / total * 255);
            }
            opacity = BitConverter.ToInt32(bytes, 0);
        }
    }

    public void OnOpacityChanged(int id)
    {
        if (IsRoad)
        {
            road = 0;
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i].id == id)
                {
                    if (textures[i].use)
                    {
                        road = 1 << i;
                        opacity = (int)(textures[i].opacity * 255);
                    }
                }
                else
                {
                    textures[i].use = false;
                }
            }

        }

        else
        {
            byte[] bytes = new byte[4];
            int total = 0;
            for (byte i = 0; i < textures.Count; i++)
            {
                if (textures[i].use)
                    bytes[i] = (byte)(textures[i].opacity * 255);
                total = total + bytes[i];
            }

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)((float)bytes[i] / total * 255);
            }
            opacity = BitConverter.ToInt32(bytes, 0);
        }
    }

    public void OnTerrainCostChanged()
    {
        for (int i = 0; i < HexMapMgr.Instance.cells.Count; i++)
        {
            HexMapMgr.Instance.cells[i].terrainCost = 0;
            int t = HexMapMgr.Instance.cells[i].TerrainTypeIndex;
            for (int j = 0; j < 4; j++)
            {
                if ((t & 1) > 0)
                {
                    HexMapMgr.Instance.cells[i].terrainCost += textures[j].cost;
                }
                t = t >> 1;
            }
        }

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
        for (int i = 0; i < HexMapMgr.Instance.chunks.Count; i++)
        {
            MeshRenderer mr = HexMapMgr.Instance.chunks[i].chunk.Rivers.gameObject.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = HexMapMgr.Instance.chunks[i].chunk.Rivers.gameObject.AddComponent<MeshRenderer>();
            mr.material = RiverMat;
        }
    }


    public void OnEstuaryMatChanged()
    {
        for (int i = 0; i < HexMapMgr.Instance.chunks.Count; i++)
        {
            MeshRenderer mr = HexMapMgr.Instance.chunks[i].chunk.Estuary.gameObject.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = HexMapMgr.Instance.chunks[i].chunk.Estuary.gameObject.AddComponent<MeshRenderer>();
            mr.material = EstuaryMat;
        }
    }

    public void OnWaterMatChanged()
    {
        for (int i = 0; i < HexMapMgr.Instance.chunks.Count; i++)
        {
            MeshRenderer mr = HexMapMgr.Instance.chunks[i].chunk.Water.gameObject.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = HexMapMgr.Instance.chunks[i].chunk.Water.gameObject.AddComponent<MeshRenderer>();
            mr.material = WaterMat;
        }

    }

    public void OnWaterShoreMatChanged()
    {
        for (int i = 0; i < HexMapMgr.Instance.chunks.Count; i++)
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
#endif
}
