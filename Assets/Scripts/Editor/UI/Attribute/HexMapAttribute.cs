using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
[Conditional("UNITY_EDITOR")]
public class HexMapAttribute : Attribute
{
    //public string group;
    //public HexMapAttribute(string group)
    //{
    //    this.group = group;
    //}

}
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
[Conditional("UNITY_EDITOR")]
public class EditorModeAttribute : Attribute
{
    //public string group;
    //public HexMapAttribute(string group)
    //{
    //    this.group = group;
    //}

}