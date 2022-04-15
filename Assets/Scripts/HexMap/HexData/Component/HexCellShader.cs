//using HexMap;
//using UnityEditor;
//using UnityEngine;

//[ExecuteInEditMode]
//public class HexCellShader : MonoBehaviour
//{
//    Texture2D cellTexture;//每8bit存储一张纹理的索引（2bit）和透明度 一共4张
//    Color32[] cellTextureData;

//    public void Initialize(int x, int z)
//    {
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
//        }
//        else
//        {
//            for (int i = 0; i < cellTextureData.Length; i++)
//            {
//                cellTextureData[i] = new Color32(0, 0, 0, 0);
//            }
//        }
//        enabled = true;
//    }

//    public void RefreshTerrain(HexCell cell)
//    {
//        byte[] bytes = System.BitConverter.GetBytes(cell.TerrainTypeIndex);
//        cellTextureData[cell.id].r = bytes[0];
//        cellTextureData[cell.id].g = bytes[1];
//        cellTextureData[cell.id].b = bytes[2];
//        cellTextureData[cell.id].a = bytes[3];
//        enabled = true;
//    }

//    public void RefreshVisibility(HexCell cell)
//    {
//        Shader.SetGlobalInt("_Fog", cell.IsVisible ? 1 : 0);
//        Shader.SetGlobalInt("_IsExplored", cell.IsExplored ? 1 : 0);
//        enabled = true;
//    }

//    void LateUpdate()
//    {
//        if (cellTexture == null)
//            return;
//        cellTexture.SetPixels32(cellTextureData);
//        cellTexture.Apply();
//        enabled = false;
//    }

//    public void SetFogOfWar(bool show)
//    {
//        Shader.SetGlobalInt("_Fog", show ? 0 : 1);
//        enabled = true;

//    }

//    public void ViewElevationChanged()
//    {
//        enabled = true;
//    }
//    public void RefreshTerrain1(HexCell cell)
//    {
//        Shader.SetGlobalInt("_TerrainWeight", cell.TerrainOpacity);
//    }

//}