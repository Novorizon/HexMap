using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using HexMap;
using UnityEditor;

namespace WorldMapEditor
{

    public partial class HexMapEditor
    {
        [LabelText("视野范围"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Settings), LabelWidth(100),]
        [Range(1, 10), VerticalGroup("FogOfWar"), OnValueChanged("OnVisionChanged"), ShowIf("IsFogOfWar")] public int Vision;

    }
}