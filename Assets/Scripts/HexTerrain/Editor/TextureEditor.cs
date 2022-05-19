using HexMap;
using System.IO;
using UnityEditor;
using UnityEngine;

public partial class HexTerrain
{
#if UNITY_EDITOR

    static public void CreateTexture(int x, int z)
    {
        HexMapMgr.Instance.TerrainTypeTexture = ExportTexture("TerrainTypeTexture", x, z);
        HexMapMgr.Instance.TerrainOpacityTexture = ExportTexture("TerrainOpacityTexture", x, z);
        HexMapMgr.Instance.RoadTexture = ExportTexture("RoadTexture", x, z);

        SetTexture();
    }

    public void CreateTexture()
    {
        HexMapMgr.Instance.TerrainTypeTexture = ExportTexture(HexMapMgr.Instance.TerrainTypeTexture);
        HexMapMgr.Instance.TerrainOpacityTexture = ExportTexture(HexMapMgr.Instance.TerrainOpacityTexture);
        HexMapMgr.Instance.RoadTexture = ExportTexture(HexMapMgr.Instance.RoadTexture);

        SetTexture();
    }

    static public void SetTexture()
    {
        HexMapMgr.Instance.TerrainMaterial.SetTexture("_TerrainTypeTexture", HexMapMgr.Instance.TerrainTypeTexture);
        HexMapMgr.Instance.TerrainMaterial.SetTexture("_TerrainOpacityTexture", HexMapMgr.Instance.TerrainOpacityTexture);
        HexMapMgr.Instance.TerrainMaterial.SetTexture("_RoadTexture", HexMapMgr.Instance.RoadTexture);
    }

    static public Texture2D ExportTexture(string name, int x, int z)
    {
        Texture2D t = new Texture2D(x, z, TextureFormat.RGBA32, false, true);
        t.name = name;
        return ExportTexture(t, true);
    }


    static public Texture2D ExportTexture(Texture2D t, bool isReadable = false)
    {
        byte[] bytes = t.EncodeToPNG();

        string filename = AbsoluteMaterialPath + t.name + ".png";
        File.WriteAllBytes(filename, bytes);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(MaterialPath + t.name + ".png");
        SetTextureProperties(tex, isReadable);
        return tex;
    }



    static void SetTextureProperties(Texture2D tex, bool isReadable)
    {
        string path = AssetDatabase.GetAssetPath(tex);

        TextureImporter texture = AssetImporter.GetAtPath(path) as TextureImporter;
        texture.textureCompression = TextureImporterCompression.Uncompressed;

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.npotScale = TextureImporterNPOTScale.None;
        texture.sRGBTexture = false;
        texture.isReadable = isReadable;

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
    }



    //void SetTextureProperties(string path, bool isReadable)
    //{
    //    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
    //    if (File.Exists(path))
    //    {
    //        path = path.Substring(path.IndexOf("Assets"));
    //        path = path.Replace('\\', '/');

    //        TextureImporter texture = AssetImporter.GetAtPath(path) as TextureImporter;
    //        texture.textureCompression = TextureImporterCompression.Uncompressed;

    //        texture.filterMode = FilterMode.Point;
    //        texture.wrapMode = TextureWrapMode.Clamp;
    //        texture.npotScale = TextureImporterNPOTScale.None;
    //        texture.sRGBTexture = false;
    //        texture.isReadable = isReadable;
    //    }
    //    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
    //}

    private Texture2D DuplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }


#endif

}