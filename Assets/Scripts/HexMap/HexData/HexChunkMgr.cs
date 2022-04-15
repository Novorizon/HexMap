using System;
using UnityEngine;

namespace HexMap
{
    public partial class HexChunkMgr
    {
        public HexChunk chunk;

        public Vector2Int Coordinate;
        public Vector3 center;

        public HexCell[] cells;

        public   HexChunkMgr()
        {

        }
        public void AddCell(int index, HexCell cell)
        {
            cells[index] = cell;
            cell.ChunkMgr = this;
            cell.chunkCoordinate = Coordinate;

        }


        public void Refresh()
        {
            Triangulates();
        }

        public void Triangulates()
        {

            chunk.Terrain.Clear();
            chunk.Rivers.Clear();
            chunk.Roads.Clear();
            chunk.Water.Clear();
            chunk.WaterShore.Clear();
            chunk.Estuary.Clear();
            chunk.Features.Clear();
            Triangular.Init(chunk);// terrain, rivers, roads, water, waterShore, estuaries, features);
            for (int i = 0; i < cells.Length; i++)
            {
                Triangular.Triangulate(cells[i]);
            }
            chunk.Terrain.Apply();
            chunk.Rivers.Apply();
            chunk.Roads.Apply();
            chunk.Water.Apply();
            chunk.WaterShore.Apply();
            chunk.Estuary.Apply();
            chunk.Features.Apply();
        }
    }
}