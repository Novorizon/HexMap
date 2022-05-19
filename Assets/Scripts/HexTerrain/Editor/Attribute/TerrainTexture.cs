using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using HexMap;
using System.Reflection;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

[Serializable]
public class TerrainTexture
{
    [HideInInspector]
    public int id;

    [HideLabel, LabelWidth(10), OnValueChanged("OnTerrainTypeChanged"), HorizontalGroup("TerrainType"), PropertySpace(SpaceBefore = 5),]
    public bool use;

    [HideLabel, HorizontalGroup("TerrainType"), OnValueChanged("OnTerrainChanged"), PropertySpace(SpaceBefore = 5)]
    public Texture2D t = null;

    [HideLabel, ProgressBar(0, 1), HorizontalGroup("opacity"), OnValueChanged("OnOpacityChanged"), PropertySpace(SpaceBefore = 3),ShowIf("use")]
    public float opacity = 1;


    [LabelText("    cost"), LabelWidth(40), HorizontalGroup("opacity"), OnValueChanged("OnTerrainCostChanged"), PropertySpace(SpaceBefore = 3), ShowIf("use")]
    public int cost;


    string dllName = "HexMapInspector"; //程序集名
    string className = "HexMapHierarchy"; //类全名
    public void Invoke(string methodName, object[] args = null)
    {

        BindingFlags flag = BindingFlags.Static | BindingFlags.Public;
        FieldInfo Instance = typeof(HexTerrain).GetField("Instance", flag);
        object instance = Instance.GetValue(Instance);


        Assembly assembly = Assembly.Load(dllName);
        if (assembly != null)
        {
            Type type = assembly.GetType(className);
            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod(methodName);

                if (methodInfo != null)
                {
                    methodInfo.Invoke(instance, args);
                }
            }
        }
    }

    public void OnTerrainChanged()
    {
        HexTerrain.Instance.CreateTextures();
    }

    public void OnTerrainTypeChanged()
    {
        HexTerrain.Instance.OnTerrainTypeChanged(id);
    }



    public void OnOpacityChanged()
    {
        HexTerrain.Instance.OnOpacityChanged(id);
        //Invoke("OnOpacityChanged", new object[] { id });
    }
    public void OnTerrainCostChanged()
    {
        HexTerrain.Instance.OnTerrainCostChanged();
        //Invoke("OnTerrainCostChanged");
    }
}

