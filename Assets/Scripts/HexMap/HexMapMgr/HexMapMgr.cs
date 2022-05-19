using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using Unity.Mathematics;

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

        HexMapData data;

        public List<HexChunkMgr> chunks;
        public List<HexCell> cells;

        public Action<object> callback;

        public HexMapData Data
        {
            get { return data; }
            set { data = value; }
        }

        public HexCell Cell(int id) => cells[id];
        public HexCell Cell(Vector2Int xz) => cells[xz.x + xz.y * data.cellCountX];
        public HexCell Cell(Hexagon hexagon) => cells[hexagon.q + hexagon.s / 2 + hexagon.s * data.cellCountX];


        public void ReCreate(GameObject root, HexMapData mapdata = null)
        {
            if (root == null)
            {
                Debug.LogError("Can not find node");
                return;
            }

            UnloadChunk();

            Root = root;
            data = mapdata;
            if (data == null)
                data = new HexMapData();

            SetCanvas();

            HexMetrics.InitializeHashGrid(data.seed);

            CreateData();
            TerrainMaterial.SetFloat("_CellX", data.cellCountX);
            TerrainMaterial.SetFloat("_Radius", HexMetrics.outerRadius);
            CreateChunks();
            CreateCells();
            RefreshChunk();

            HexTerrainData terrain = Root.GetComponent<HexTerrainData>();
            terrain.cells = cells;
            terrain.data = data;
            terrain.chunks = chunks;
#if UNITY_2020
            TerrainTypeTexture.Resize(data.cellCountX, data.cellCountZ);
            TerrainOpacityTexture.Resize(data.cellCountX, data.cellCountZ);
            RoadTexture.Resize(data.cellCountX, data.cellCountZ);
#elif UNITY_2021
            TerrainTypeTexture.Reinitialize(data.cellCountX, data.cellCountZ);
            TerrainOpacityTexture.Reinitialize(data.cellCountX, data.cellCountZ);
            RoadTexture.Reinitialize(data.cellCountX, data.cellCountZ);
#endif
            Refresh();

        }

        public void Create(GameObject root)
        {

            Root = root;
            if (Root == null)
                Root = new GameObject("HexTerrain");

            data = new HexMapData();
            SetCanvas();

            HexMetrics.InitializeHashGrid(data.seed);

            CreateData();
            TerrainMaterial.SetFloat("_CellX", data.cellCountX);
            TerrainMaterial.SetFloat("_Radius", HexMetrics.outerRadius);
            CreateChunks();
            CreateCells();
            RefreshChunk();

            HexTerrainData terrain = Root.AddComponent<HexTerrainData>();
            terrain.Root = root;
            terrain.cells = cells;
            terrain.data = data;

            terrain.chunks = chunks;
            Shader.EnableKeyword("HEX_MAP_VISION");
        }


        public void Create(HexMapData mapdata, GameObject root = null, Action<object> callback = null, object userdata = null)
        {
            data = mapdata;
            HexMetrics.InitializeHashGrid(data.seed);

            Root = root;
            if (Root == null)
                Root = new GameObject("HexTerrain");

            SetCanvas();

            CreateData();
            TerrainMaterial.SetFloat("_CellX", data.cellCountX);
            TerrainMaterial.SetFloat("_Radius", HexMetrics.outerRadius);
            CreateChunks();
            CreateCells();
            RefreshChunk();

            Root.AddComponent<HexTerrainData>();

            Shader.EnableKeyword("HEX_MAP_VISION");

            callback?.Invoke(userdata);
        }



        public void Load(GameObject root, Action<object> callback = null, object userdata = null)
        {
            if (root == null)
            {
                Debug.LogError("Can not find Root");
                return;
            }
            Root = root;

            HexTerrainData terrain = Root.GetComponent<HexTerrainData>();
            if (terrain == null)
            {
                Debug.LogError("Can not find terrain");
                return;
            }
            cells = terrain.cells;
            data = terrain.data;
            chunks = terrain.chunks;

            HexMetrics.InitializeHashGrid(data.seed);
            CreateData();

            for (int z = 0, i = 0; z < data.cellCountZ; z++)
            {
                for (int x = 0; x < data.cellCountX; x++)
                {
                    cells[i].hexagon = Hexagon.FromXZ(x, z);
                    i++;
                }
            }

            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
            }
            for (int i = 0; i < cells.Count; i++)
            {
                SetNeighbor(cells[i]);
                AddCellToChunk(cells[i]);
            }

            Refresh();
            callback?.Invoke(userdata);

        }


        public void Load(HexTerrainData terrain, Action<object> callback = null, object userdata = null)
        {
            if (terrain == null)
            {
                Debug.LogError("Can not find HexTerrainData");
                return;
            }
            Root = terrain.gameObject;

            if (Root == null)
            {
                Debug.LogError("Can not find Root");
                return;
            }
            cells = terrain.cells;
            data = terrain.data;
            chunks = terrain.chunks;

            HexMetrics.InitializeHashGrid(data.seed);

            for (int z = 0, i = 0; z < data.cellCountZ; z++)
            {
                for (int x = 0; x < data.cellCountX; x++)
                {
                    cells[i].hexagon = Hexagon.FromXZ(x, z);
                    i++;
                }
            }
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
            }
            for (int i = 0; i < cells.Count; i++)
            {
                SetNeighbor(cells[i]);
                AddCellToChunk(cells[i]);
            }
            callback?.Invoke(userdata);
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
            chunks = new List<HexChunkMgr>();
            CreateHexChunk();

            for (int z = 0, i = 0; z < data.chunkCountZ; z++)
            {
                for (int x = 0; x < data.chunkCountX; x++)
                {
                    HexChunkMgr ChunkMgr = new HexChunkMgr();
                    chunks.Add(ChunkMgr);
                    ChunkMgr.chunk = GameObject.Instantiate(HexChunk);
                    ChunkMgr.chunk.gameObject.SetActive(true);
                    ChunkMgr.chunk.name = x + "_" + z;

                    ChunkMgr.chunk.transform.SetParent(Root.transform);
                    ChunkMgr.Coordinate = new Vector2Int(x, z);
                    ChunkMgr.cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];


                    ChunkMgr.chunk.Terrain.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.Terrain.hexMesh = new Mesh();
                    ChunkMgr.chunk.Rivers.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.Rivers.hexMesh = new Mesh();
                    ChunkMgr.chunk.Water.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.Water.hexMesh = new Mesh();
                    ChunkMgr.chunk.WaterShore.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.WaterShore.hexMesh = new Mesh();
                    ChunkMgr.chunk.Estuary.GetComponent<MeshFilter>().mesh = ChunkMgr.chunk.Estuary.hexMesh = new Mesh();
                }
            }
            GameObject.DestroyImmediate(HexChunkPrefab);
        }


        void CreateData()
        {
            data.cellCountX = data.chunkCountX * HexMetrics.chunkSizeX;
            data.cellCountZ = data.chunkCountZ * HexMetrics.chunkSizeZ;
            data.cellCount = data.cellCountX * data.cellCountZ;

            Initialize(data.cellCountX, data.cellCountZ);
        }




        void RefreshChunk()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].Refresh();
            }
        }


        void CreateCells()
        {
            Label = AssetDatabase.LoadAssetAtPath<Text>("Packages/com.guanjinbiao.hexmap/Assets/Prefabs/Label.prefab");
            if (Label == null)
                Label = AssetDatabase.LoadAssetAtPath<Text>("Assets/hexmap/Assets/Prefabs/Label.prefab");

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
            SetLabel(cell);
        }

        void SetNeighbor(HexCell cell)
        {
            cell.neighbors = new HexCell[6];
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
            Hexagon coordinates = Hexagon.FromPosition(position);
            int index = coordinates.q + coordinates.s * data.cellCountX + coordinates.s / 2;
            return cells[index];
        }


        public HexCell GetCell(Hexagon coordinates)
        {
            int z = coordinates.s;
            if (z < 0 || z >= data.cellCountZ)
            {
                return null;
            }
            int x = coordinates.q + z / 2;
            if (x < 0 || x >= data.cellCountX)
            {
                return null;
            }
            return cells[x + z * data.cellCountX];
        }

        public void UnloadChunk()
        {
            if (cells != null)
                cells.Clear();
            if (chunks != null)
                for (int i = 0; i < chunks.Count; i++)
                {

                    chunks[i] = null;
                }

            for (int i = Root.transform.childCount - 1; i >= 0; i--)
            {
                //if (Root.transform.GetChild(i).GetComponent<HexChunk>() != null)
                {
                    GameObject.DestroyImmediate(Root.transform.GetChild(i).gameObject);
                }
            }
        }


        public void UnloadMap()
        {
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

        void SetCanvas()
        {
            GameObject go = new GameObject("Canvas");
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;

            Canvas = go.GetComponent<Canvas>();
            RectTransform RectTransform = Canvas.GetComponent<RectTransform>();
            RectTransform.sizeDelta = Vector2.zero;
            RectTransform.position = new Vector3(0, 0.1f, 0);
            //RectTransform. = new Vector3(0, 0.1f, 0);
            RectTransform.anchorMax = Vector2.zero;
            RectTransform.anchorMin = Vector2.zero;
            RectTransform.anchorMax = Vector2.zero;
            RectTransform.pivot = Vector2.zero;
            RectTransform.Rotate(Vector3.right, 90);

            go.transform.SetParent(Root.transform);
        }

        void SetLabel(HexCell cell)
        {
            if (cell == null)
                return;
            if (Label == null)
                return;
            Text label = GameObject.Instantiate(Label);
            label.rectTransform.anchoredPosition = new Vector2(cell.center.x, cell.center.z);
            cell.uiRect = label.rectTransform;
            cell.uiRect.SetParent(Canvas.transform, false);
        }



        public void SetMaterial(Material material)
        {
            TerrainMaterial = material;
            TerrainMaterial?.DisableKeyword("GRID_ON");
        }

#endif
    }
}