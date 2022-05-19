using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap
{
    public partial class HexMapMgr
    {

        public Texture2D cellTexture;
        Color32[] cellTextureData;

        public Texture2D roadTexture;
        Color32[] roadTextureData;

        Texture2D terrainOpacityTexture;
        Color32[] terrainOpacityData;

        public Texture2D TerrainTypeTexture
        {
            get { return cellTexture; }
            set { cellTexture = value; }
        }

        public Texture2D TerrainOpacityTexture
        {
            get { return terrainOpacityTexture; }
            set { terrainOpacityTexture = value; }
        }


        public Texture2D RoadTexture
        {
            get { return roadTexture; }
            set { roadTexture = value; }
        }
        public Color32[] TerrainOpacityData
        {
            get { return terrainOpacityData; }
            set { terrainOpacityData = value; }
        }


        public void Initialize(int x, int z)
        {
            Shader.SetGlobalVector("_HexCellData_TexelSize", new Vector4(1f / x, 1f / z, x, z));

            cellTextureData = new Color32[x * z];
            for (int i = 0; i < cellTextureData.Length; i++)
            {
                //if (i >= 1024)
                //    break;

                byte[] bytes = System.BitConverter.GetBytes(i);
                cellTextureData[i] = new Color32(0, 0, 0, 1);
                cellTextureData[i].r = bytes[1];
                cellTextureData[i].g = bytes[0];
                cellTextureData[i].a = 0;
            }


            TerrainOpacityData = new Color32[x * z];
            for (int i = 0; i < TerrainOpacityData.Length; i++)
            {
                TerrainOpacityData[i] = new Color32(255, 255, 255, 255);
            }

            roadTextureData = new Color32[x * z];
            for (int i = 0; i < roadTextureData.Length; i++)
            {
                TerrainOpacityData[i] = new Color32();
            }

        }

        public void Refresh()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                RefreshTerrain(cells[i]);
                RefreshRoad(cells[i]);
                cells[i].Refresh();
            }
        }


        public void RefreshTerrain(HexCell cell)
        {
            if (cellTexture == null)
                return;
            if (terrainOpacityTexture == null)
                return;

            byte[] bytes = System.BitConverter.GetBytes(cell.TerrainOpacity);
            TerrainOpacityData[cell.id].r = bytes[0];
            TerrainOpacityData[cell.id].g = bytes[1];
            TerrainOpacityData[cell.id].b = bytes[2];
            TerrainOpacityData[cell.id].a = bytes[3];


            cellTextureData[cell.id].a = (byte)cell.TerrainTypeIndex;

            cellTexture.SetPixels32(cellTextureData);
            cellTexture.Apply();

            terrainOpacityTexture.SetPixels32(TerrainOpacityData);
            terrainOpacityTexture.Apply();
        }

        public void RefreshRoad(HexCell cell)
        {
            if (cellTexture == null)
                return;
            if (roadTexture == null)
                return;
            //朝向
            byte dir = 0;
            for (int i = 0; i < 6; i++)
            {
                if ((cell.roads[i]))
                {
                    dir = (byte)(dir | (1 << i));
                }
            }
            cellTextureData[cell.id].b = (byte)(cell.Road);
            cellTexture.SetPixels32(cellTextureData);
            cellTexture.Apply();

            //roadTextureData[cell.id].r = (byte)(((int)cell.roadNoiseType << 7) | (int)dir);
            roadTextureData[cell.id].r = dir;
            roadTextureData[cell.id].g = (byte)(cell.RoadWidthIF * 255);
            roadTextureData[cell.id].b = (byte)(cell.RoadOpacity);
            roadTextureData[cell.id].a = (byte)(cell.RoadNoiseIF * 255);
            roadTexture.SetPixels32(roadTextureData);
            roadTexture.Apply();
        }

    }
}