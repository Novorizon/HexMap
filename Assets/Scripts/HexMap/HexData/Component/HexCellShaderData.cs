//using HexMap;
//using UnityEditor;
//using UnityEngine;

//[ExecuteInEditMode]
//public class HexCellShaderData : MonoBehaviour
//{

//    public Texture2D cellTexture;
//    Color32[] cellTextureData;

//    //[HideInInspector]
//     Texture2D terrainOpacityTexture;
//     Color32[] terrainOpacityData;

//    public Texture2D TerrainTypeTexture
//    {
//        get { return cellTexture; }
//        set { cellTexture = value; }
//    }

//    public Texture2D TerrainOpacityTexture
//    {
//        get { return terrainOpacityTexture; }
//        set { terrainOpacityTexture = value; }
//    }

//    public Color32[] TerrainOpacityData
//    {
//        get { return terrainOpacityData; }
//        set { terrainOpacityData = value; }
//    }

//    public void Initialize(int x, int z)
//    {
//        //if (cellTexture)
//        //{
//        //    cellTexture.Resize(x, z);
//        //}
//        //else
//        //{
//        //    cellTexture = new Texture2D(x, z, TextureFormat.RGBA32, false, true);
//        //    cellTexture.filterMode = FilterMode.Point;
//        //    cellTexture.wrapMode = TextureWrapMode.Clamp;
//        //    Shader.SetGlobalTexture("_HexCellData", cellTexture);
//        //    Shader.SetGlobalColor("_BackgroundColor", Color.gray);
//        //}

//        Shader.SetGlobalVector("_HexCellData_TexelSize", new Vector4(1f / x, 1f / z, x, z));

//        if (cellTextureData == null || cellTextureData.Length != x * z)
//        {
//            cellTextureData = new Color32[x * z];
//            for (int i = 0; i < cellTextureData.Length; i++)
//            {
//                cellTextureData[i] = new Color32(0, 0, 0, 1);
//            }
//        }
//        else
//        {
//            for (int i = 0; i < cellTextureData.Length; i++)
//            {
//                cellTextureData[i] = new Color32(0, 0, 0, 1);
//            }
//        }

//#if UNITY_EDITOR
//        if (!EditorApplication.isPlaying)
//        {
//            //if (terrainOpacityTexture)
//            //{
//            //    terrainOpacityTexture.Resize(x, z);
//            //}
//            //else
//            //{
//            //    terrainOpacityTexture = new Texture2D(x, z, TextureFormat.RGBA32, false, true);
//            //    terrainOpacityTexture.filterMode = FilterMode.Point;
//            //    terrainOpacityTexture.wrapMode = TextureWrapMode.Clamp;
//            //    //Shader.SetGlobalTexture("_TerrainOpacityTexture", TerrainOpacityTexture);
//            //}

//            if (TerrainOpacityData == null || TerrainOpacityData.Length != x * z)
//            {
//                TerrainOpacityData = new Color32[x * z];
//                for (int i = 0; i < TerrainOpacityData.Length; i++)
//                {
//                    TerrainOpacityData[i] = new Color32(255, 255, 255, 255);
//                }
//            }
//            else
//            {
//                for (int i = 0; i < TerrainOpacityData.Length; i++)
//                {
//                    TerrainOpacityData[i] = new Color32(255, 255, 255, 255);
//                }
//            }
//        }
//#endif
//        enabled = true;
//    }

//    public void Load(Texture2D opacity, int x, int z)
//    {
//        terrainOpacityTexture = opacity;

//        if (cellTexture)
//        {
//            cellTexture.Resize(x, z);
//        }
//        else
//        {
//            cellTexture = new Texture2D(x, z, TextureFormat.RGBA32, false, true);
//            cellTexture.filterMode = FilterMode.Point;
//            cellTexture.wrapMode = TextureWrapMode.Clamp;
//            Shader.SetGlobalTexture("_HexCellData", cellTexture);
//            Shader.SetGlobalColor("_BackgroundColor", Color.gray);
//        }
//        Shader.SetGlobalVector("_HexCellData_TexelSize", new Vector4(1f / x, 1f / z, x, z));

//        if (cellTextureData == null || cellTextureData.Length != x * z)
//        {
//            cellTextureData = new Color32[x * z];
//            for (int i = 0; i < cellTextureData.Length; i++)
//            {
//                cellTextureData[i] = new Color32(0, 0, 0, 1);
//            }
//        }
//        else
//        {
//            for (int i = 0; i < cellTextureData.Length; i++)
//            {
//                cellTextureData[i] = new Color32(0, 0, 0, 1);
//            }
//        }

//        TerrainOpacityData = new Color32[x * z];
//        for (int i = 0; i < TerrainOpacityTexture.height; i++)
//        {
//            for (int j = 0; j < TerrainOpacityTexture.width; j++)
//            {
//                //HexMapMgr.Instance.Root.GetComponent<HexCellShaderData>().TerrainOpacityData[i * TerrainOpacityTexture.height + j] = TerrainOpacityTexture.GetPixel(i, j);
//            }
//        }
//        enabled = true;
//    }
//    public void RefreshTerrain(HexCell cell)
//    {
//        if (!EditorApplication.isPlaying)
//        {
//            byte[] bytes = System.BitConverter.GetBytes(cell.TerrainOpacity);
//            TerrainOpacityData[cell.id].r = bytes[0];
//            TerrainOpacityData[cell.id].g = bytes[1];
//            TerrainOpacityData[cell.id].b = bytes[2];
//            TerrainOpacityData[cell.id].a = bytes[3];
//        }

//        cellTextureData[cell.id].a = (byte)cell.TerrainTypeIndex;
//        enabled = true;
//    }

//    public void RefreshVisibility(HexCell cell)
//    {
//        int index = cell.id;
//        cellTextureData[index].r = cell.IsVisible ? (byte)255 : (byte)0;
//        cellTextureData[index].g = cell.IsExplored ? (byte)255 : (byte)0;
//        enabled = true;
//    }

//    void LateUpdate()
//    {
//        enabled = false;
//        if (cellTexture == null)
//            return;
//        cellTexture.SetPixels32(cellTextureData);
//        cellTexture.Apply();
//        if (terrainOpacityTexture == null)
//            return;
//        terrainOpacityTexture.SetPixels32(TerrainOpacityData);
//        terrainOpacityTexture.Apply();
//    }

//    public void SetFogOfWar(bool show)
//    {
//        byte value = show ? (byte)0 : (byte)255;

//        if (cellTextureData == null)
//            return;
//        for (int i = 0; i < cellTextureData.Length; i++)
//        {
//            cellTextureData[i].r = value;
//        }
//        enabled = true;

//    }

//    public void ViewElevationChanged()
//    {
//        enabled = true;
//    }

//}