using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
//using Cinemachine;

namespace HexMap
{
    [Serializable]
    public partial class HexMapMgr : Singleton<HexMapMgr>
    {
        public GameObject Root;

        GameObject HexChunkPrefab;//动态创建
        HexChunk HexChunk;
        public Material TerrainMaterial;
        public Texture2D GridTex;

        WorldMapData data;

        HexCellShaderData cellShaderData;

        public HexChunkMgr[] chunks;
        public List<HexCell> cells;

        public Action<object> callback;

        public WorldMapData Data
        {
            get { return data; }
            set { data = value; }
        }


        public void Create(WorldMapData data, GameObject root = null, Action<object> callback = null, object userdata = null)
        {
            Data = data;
            HexMetrics.InitializeHashGrid(data.seed);

            Root = root;
            if (Root == null)
                Root = new GameObject("WorldMap");
#if UNITY_EDITOR
            GameObject go = new GameObject("Canvas");
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;

            Canvas = go.AddComponent<Canvas>();
            RectTransform RectTransform = go.GetComponent<RectTransform>();
            RectTransform.position = new Vector3(0, 0.1f, 0);
            //RectTransform. = new Vector3(0, 0.1f, 0);
            RectTransform.anchorMax = Vector2.zero;
            RectTransform.anchorMin = Vector2.zero;
            RectTransform.anchorMax = Vector2.zero;
            RectTransform.pivot = Vector2.zero;
            RectTransform.Rotate(Vector3.right, 90);


            go.transform.SetParent(Root.transform);
#endif

            Shader.EnableKeyword("HEX_MAP_VISION");

            CreateData();
            CreateChunks();
            CreateCells();
            RefreshChunk();

            Root.AddComponent<MapData>().cells = cells;
            callback?.Invoke(userdata);
        }


#if UNITY_EDITOR
        public void LoadScene(TextAsset asset)
        {
            if (asset != null)
            {
                Load(asset, true);

                Transform Transform = Root.transform.Find("Canvas");
                if (Transform == null)
                {
                    GameObject go = new GameObject("Canvas");
                    go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;
                    Canvas = go.AddComponent<Canvas>();
                    RectTransform RectTransform = go.GetComponent<RectTransform>();
                    RectTransform.position = new Vector3(0, 0.1f, 0);
                    RectTransform.anchorMin = Vector2.zero;
                    RectTransform.anchorMax = Vector2.zero;
                    RectTransform.pivot = Vector2.zero;
                    RectTransform.Rotate(Vector3.right, 90);

                    go.transform.SetParent(Root.transform);
                }
            }
        }
#endif



        public void Load(TextAsset asset, bool vision = true, Action<object> callback = null, object userdata = null)
        {
            if (asset != null)
            {
                Root = GameObject.Find("WorldMap");
                if (Root == null)
                {
                    Debug.LogError("Can not find WorldMap node");
                    return;
                }

                data = new WorldMapData();
                cells = Root.GetComponent<MapData>().cells;

                if (vision)
                    Shader.EnableKeyword("HEX_MAP_VISION");
                else
                    Shader.DisableKeyword("HEX_MAP_VISION");


                Stream ms = new MemoryStream(asset.bytes);
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    Reader(reader);
                    HexMetrics.InitializeHashGrid(data.seed);
                    CreateData();
                    LoadCells(reader, data.version);
                    LoadChunks();
                }

                callback?.Invoke(userdata);
            }
        }


        public void Reader(BinaryReader reader)
        {
            data.version = reader.ReadInt32();
            data.id = reader.ReadInt32();
            data.seed = reader.ReadInt32();
            data.chunkCountX = reader.ReadByte();
            data.chunkCountZ = reader.ReadByte();
            HexMetrics.InitializeHashGrid(data.seed);
        }


        public void CreateHexChunk()
        {
            HexChunkPrefab = new GameObject("Chunk");
            HexChunkPrefab.SetActive(false);

            HexChunk = HexChunkPrefab.AddComponent<HexChunk>();
            HexChunk.Init();

            GameObject Terrain = new GameObject("Terrain");
            Terrain.transform.SetParent(HexChunkPrefab.transform);
            Terrain.AddComponent<MeshRenderer>().material = TerrainMaterial;
            HexChunk.Terrain = Terrain.AddComponent<HexMesh>();
            HexChunk.Terrain.Set(true, true, false, false);

            GameObject Rivers = new GameObject("Rivers");
            Rivers.transform.SetParent(HexChunkPrefab.transform);
            HexChunk.Rivers = Rivers.AddComponent<HexMesh>();
            HexChunk.Rivers.Set(false, true, true, false);

            GameObject Roads = new GameObject("Roads");
            Roads.transform.SetParent(HexChunkPrefab.transform);
            HexChunk.Roads = Roads.AddComponent<HexMesh>();
            HexChunk.Roads.Set(false, true, true, false);

            GameObject Water = new GameObject("Water");
            Water.transform.SetParent(HexChunkPrefab.transform);
            HexChunk.Water = Water.AddComponent<HexMesh>();
            HexChunk.Water.Set(false, true, false, false);

            GameObject WaterShore = new GameObject("WaterShore");
            WaterShore.transform.SetParent(HexChunkPrefab.transform);
            HexChunk.WaterShore = WaterShore.AddComponent<HexMesh>();
            HexChunk.WaterShore.Set(false, true, true, false);

            GameObject Estuary = new GameObject("Estuary");
            Estuary.transform.SetParent(HexChunkPrefab.transform);
            HexChunk.Estuary = Estuary.AddComponent<HexMesh>();
            HexChunk.Estuary.Set(false, true, true, true);


            GameObject Features = new GameObject("Features");
            Features.transform.SetParent(HexChunkPrefab.transform);
            HexChunk.Features = Features.AddComponent<HexFeature>();
            //HexChunk.Features.bridge = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        }

        void CreateChunks()
        {
            chunks = new HexChunkMgr[data.chunkCountX * data.chunkCountZ];
            CreateHexChunk();

            for (int z = 0, i = 0; z < data.chunkCountZ; z++)
            {
                for (int x = 0; x < data.chunkCountX; x++)
                {
                    HexChunkMgr ChunkMgr = chunks[i++] = new HexChunkMgr();
                    ChunkMgr.chunk = GameObject.Instantiate(HexChunk);
                    ChunkMgr.chunk.gameObject.SetActive(true);
                    ChunkMgr.chunk.name = x + "_" + z;

                    ChunkMgr.chunk.transform.SetParent(Root.transform);
                    ChunkMgr.Coordinate = new Vector2Int(x, z);
                    ChunkMgr.cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];


                    ChunkMgr.chunk.Terrain.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.Terrain.hexMesh = new Mesh();
                    ChunkMgr.chunk.Rivers.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.Rivers.hexMesh = new Mesh();
                    ChunkMgr.chunk.Roads.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.Roads.hexMesh = new Mesh();
                    ChunkMgr.chunk.Water.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.Water.hexMesh = new Mesh();
                    ChunkMgr.chunk.WaterShore.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.WaterShore.hexMesh = new Mesh();
                    ChunkMgr.chunk.Estuary.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.Estuary.hexMesh = new Mesh();
                }
            }
            GameObject.DestroyImmediate(HexChunkPrefab);
        }

        void LoadChunks()
        {
            chunks = new HexChunkMgr[data.chunkCountX * data.chunkCountZ];
            for (int z = 0, i = 0; z < data.chunkCountZ; z++)
            {
                for (int x = 0; x < data.chunkCountX; x++)
                {
                    HexChunkMgr ChunkMgr = chunks[i++] = new HexChunkMgr();
                    ChunkMgr.chunk = GameObject.Find(x + "_" + z).GetComponent<HexChunk>();

                    ChunkMgr.Coordinate = new Vector2Int(x, z);
                    ChunkMgr.cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
                }
            }

            for (int i = 0; i < cells.Count; i++)
            {
                AddCellToChunk(cells[i]);
            }
        }

        void CreateData()
        {
            data.cellCountX = data.chunkCountX * HexMetrics.chunkSizeX;
            data.cellCountZ = data.chunkCountZ * HexMetrics.chunkSizeZ;
            data.cellCount = data.cellCountX * data.cellCountZ;

            cellShaderData = Root.GetComponent<HexCellShaderData>();
            if (cellShaderData == null)
                cellShaderData = Root.AddComponent<HexCellShaderData>();
            cellShaderData.enabled = true;
            cellShaderData.Initialize(data.cellCountX, data.cellCountZ);


#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                TerrainMaterial.SetTexture("_TerrainTypeTexture", cellShaderData.cellTexture);
                TerrainMaterial.SetTexture("_TerrainOpacityTexture", cellShaderData.TerrainOpacityTexture);
            }
#endif
        }


        void LoadCells(BinaryReader reader, int version)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].ShaderData = cellShaderData;
                cells[i].Import(reader, version);
            }

            for (int i = 0; i < cells.Count; i++)
            {
                SetNeighbor(cells[i]);
            }
        }


        void RefreshChunk()
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].Refresh();
            }
        }


        void CreateCells()
        {
            cells = new List<HexCell>(data.cellCountZ * data.cellCountX);
            for (int z = 0, i = 0; z < data.cellCountZ; z++)
            {
                for (int x = 0; x < data.cellCountX; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        void CreateCell(int x, int z, int i)
        {
            HexCell cell = new HexCell();
            cell.Init(i, x, z);

            cells.Add(cell);
            cell.ShaderData = cellShaderData;
            cell.Explorable = x > 0 && z > 0 && x < data.cellCountX - 1 && z < data.cellCountZ - 1;

            //SetPosition
            cell.xz.x = x;
            cell.xz.y = z;
            cell.center.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
            cell.center.y = 0f;
            cell.center.z = z * (HexMetrics.outerRadius * 1.5f);
            cell.RefreshPosition();

            //SetNeighbor
            if (x > 0)
            {
                cell.SetNeighbor(HexDirection.W, cells[i - 1]);
            }
            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - data.cellCountX]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, cells[i - data.cellCountX - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - data.cellCountX]);
                    if (x < data.cellCountX - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, cells[i - data.cellCountX + 1]);
                    }
                }
            }

            AddCellToChunk(x, z, cell);
            //SetLabel(cell);
        }

        void SetNeighbor(HexCell cell)
        {
            int x = cell.xz.x;
            int z = cell.xz.y;
            int i = cell.id;
            if (x > 0)
            {
                cell.SetNeighbor(HexDirection.W, cells[i - 1]);
            }
            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - data.cellCountX]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, cells[i - data.cellCountX - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - data.cellCountX]);
                    if (x < data.cellCountX - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, cells[i - data.cellCountX + 1]);
                    }
                }
            }

            //AddCellToChunk(x, z, cell);
        }

        void AddCellToChunk(int x, int z, HexCell cell)
        {
            int chunkX = x / HexMetrics.chunkSizeX;
            int chunkZ = z / HexMetrics.chunkSizeZ;
            HexChunkMgr chunk = chunks[chunkX + chunkZ * data.chunkCountX];
            cell.ChunkMgr = chunk;

            int localX = x - chunkX * HexMetrics.chunkSizeX;
            int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
            chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);

        }

        void AddCellToChunk(HexCell cell)
        {
            int x = cell.xz.x;
            int z = cell.xz.y;
            int chunkX = x / HexMetrics.chunkSizeX;
            int chunkZ = z / HexMetrics.chunkSizeZ;
            HexChunkMgr chunk = chunks[chunkX + chunkZ * data.chunkCountX];
            cell.ChunkMgr = chunk;

            int localX = x - chunkX * HexMetrics.chunkSizeX;
            int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
            chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);

        }



        public HexCell GetCell(Vector3 position)
        {
            position = Root.transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            int index = coordinates.X + coordinates.Z * data.cellCountX + coordinates.Z / 2;
            return cells[index];
        }


        public HexCell GetCell(HexCoordinates coordinates)
        {
            int z = coordinates.Z;
            if (z < 0 || z >= data.cellCountZ)
            {
                return null;
            }
            int x = coordinates.X + z / 2;
            if (x < 0 || x >= data.cellCountX)
            {
                return null;
            }
            return cells[x + z * data.cellCountX];
        }


        public void UnloadMap(GameObject root = null)
        {
            if (root != null)
                Root = root;
            if (cells != null)
                cells.Clear();
            if (chunks != null)
            {
                for (int i = 0; i < chunks.Length; i++)
                {
                    if (chunks[i] == null)
                        continue;
                    chunks[i].cells = null;
#if UNITY_EDITOR
                    GameObject.DestroyImmediate(chunks[i].chunk);
#else
                    GameObject.Destroy(chunks[i].chunk);
#endif
                    chunks[i] = null;
                }
            }
            if (Root != null)
#if UNITY_EDITOR
                GameObject.DestroyImmediate(Root);
#else
            GameObject.Destroy(Root);
#endif
        }

#if UNITY_EDITOR
        Canvas Canvas;
        public Text Label;
        void SetLabel()
        {
            if (cells == null)
                return;
            for (int i = 0; i < cells.Count; i++)
            {
                HexCell cell = cells[i];
                Text label = GameObject.Instantiate(Label);
                label.rectTransform.anchoredPosition = new Vector2(cell.center.x, cell.center.z);
                cell.uiRect = label.rectTransform;
                cell.uiRect.SetParent(Canvas.transform, false);
            }
        }
        void SetLabel(HexCell cell)
        {
            return;
            if (cell == null)
                return;
            Text label = GameObject.Instantiate(Label);
            label.rectTransform.anchoredPosition = new Vector2(cell.center.x, cell.center.z);
            cell.uiRect = label.rectTransform;
            cell.uiRect.SetParent(Canvas.transform, false);
        }

        public void LoadMaterial(string path = "")
        {
            TerrainMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
            TerrainMaterial?.DisableKeyword("GRID_ON");
        }

        public void SetMaterial(Material material)
        {
            TerrainMaterial = material;// HexChunk.Terrain.GetComponent<MeshRenderer>().sharedMaterial;
            TerrainMaterial?.DisableKeyword("GRID_ON");
        }

#endif
    }
}