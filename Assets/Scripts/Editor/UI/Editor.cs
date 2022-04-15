using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using HexMap;
using UnityEditor;
using System.Reflection;
using System.Linq.Expressions;

namespace WorldMapEditor
{

    public partial class HexMapEditor
    {

        [EditorMode] bool IsBrush = true;
        [EditorMode] bool IsFeature = false;
        [EditorMode] bool IsPathfinding = false;
        [EditorMode] bool IsFogOfWar = false;
        [EditorMode] bool IsSettings = false;


        private static Texture2D _new_ico, _save_ico, _open_ico, _warn_ico;

        private static GUIContent new_ico;
        //}

        //编辑模式
        EditorMode mode = EditorMode.Brush;

        void UpdateMode()
        {
            Type type = typeof(HexMapEditor);
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
    }
}