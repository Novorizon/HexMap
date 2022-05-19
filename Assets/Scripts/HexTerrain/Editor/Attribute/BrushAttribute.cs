using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
[Conditional("UNITY_EDITOR")]
public class BrushAttribute : Attribute
{
    public string group;
    public BrushAttribute(string group)
    {
        this.group = group;
    }

    public BrushAttribute()
    {
    }
}