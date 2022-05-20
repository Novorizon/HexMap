using UnityEngine;
using System;
using System.Reflection;
using System.Linq.Expressions;

public partial class HexTerrain
{
#if UNITY_EDITOR
    static string MapName;
    static string AbsolutePath { get { return Application.dataPath + "/" + MapName; } }
    static string Path
    {
        get
        {
            int start = AbsolutePath.IndexOf("Assets/");
            return AbsolutePath.Substring(start);
        }
    }

    static string AbsoluteTerrainTextureArrayPath { get { return AbsolutePath + "/Material/" + "TerrainTextureArray.asset"; } }

    static string MaterialPath { get { return Path + "/Material/"; } }
    static string AbsoluteMaterialPath { get { return AbsolutePath + "/Material/"; } }

    string HexTerrainAssetPath { get { return Path + "/" + MapName + ".asset"; } }
    string MapJsonPath { get { return Path + "/" + MapName + ".json"; } }
    public string MapScenePath { get { return Path + "/" + MapName + ".unity"; } }


    [EditorMode] bool IsBrush = true;
    [EditorMode] bool IsFeature = false;
    [EditorMode] bool IsPathfinding = false;
    [EditorMode] bool IsFogOfWar = false;
    [EditorMode] bool IsSettings = false;

    public void OnValueChanged()
    {
        changed = true;
        UType = UpdateType.ReCreate;

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
        cellCount = cellCountX * cellCountZ;
        cellMetreX = cellCountX * radius * HexMetrics.outerToInner * 2;
        cellMetreZ = 1.5f * cellCountZ * radius + 0.5f * radius;
        HexMetrics.Radius = radius;
        HexMetrics.noiseIF = noiseIF;
    }


    EditorMode mode = EditorMode.Brush;

    void UpdateMode()
    {
        Type type = typeof(HexTerrain);
        FieldInfo[] Infos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        string Name = mode switch
        {
            EditorMode.Brush => GetVariableName(() => IsBrush),
            EditorMode.Feature => GetVariableName(() => IsFeature),
            EditorMode.Pathfinding => GetVariableName(() => IsPathfinding),
            EditorMode.FogOfWar => GetVariableName(() => IsFogOfWar),
            EditorMode.Settings => GetVariableName(() => IsSettings),
            _ => IsBrush.ToString(),
        };

        for (int i = 0; i < Infos.Length; i++)
        {
            EditorModeAttribute a = Attribute.GetCustomAttribute(Infos[i], typeof(EditorModeAttribute)) as EditorModeAttribute;
            if (a != null)
            {
                Infos[i].SetValue(this, false);
                if (Name == Infos[i].Name)
                {
                    Infos[i].SetValue(this, true);
                }
            }
        }
        if (IsFogOfWar)
            Shader.DisableKeyword("HEX_MAP_VISION");
        else
            Shader.EnableKeyword("HEX_MAP_VISION");
    }

    public static string GetVarName<T>(Expression<Func<T, T>> exp)
    {
        return ((MemberExpression)exp.Body).Member.Name;
    }

    string GetVariableName<T>(Expression<Func<T>> expr)
    {
        var body = (MemberExpression)expr.Body;

        return body.Member.Name;
    }

    public void OnEditorModeChanged()
    {
        IsBrush = mode == EditorMode.Brush;
        IsFeature = mode == EditorMode.Feature;
        IsPathfinding = mode == EditorMode.Pathfinding;
        IsFogOfWar = mode == EditorMode.FogOfWar;
        IsSettings = mode == EditorMode.Settings;
    }
#endif
}
