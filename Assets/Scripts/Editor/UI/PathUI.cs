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

        [LabelText("显示距离"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Settings), HorizontalGroup("Distance"), OnValueChanged("OnShowDistanceChanged"), ShowIf("IsPathfinding")]
        public bool showDistance = false;

        [LabelText("回合制行动速度"), ToggleLeft, PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Settings), LabelWidth(30), HorizontalGroup("Speed"), ShowIf("IsPathfinding")]
        public bool isTurnBased = false;
        [HideLabel, PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Settings), HorizontalGroup("Speed"), ShowIf("IsPathfinding")]
        public int speed = 100000000;



    }
}