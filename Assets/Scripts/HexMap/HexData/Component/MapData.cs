using HexMap;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexMap
{
    [ExecuteInEditMode]
    public class MapData : MonoBehaviour
    {
        [HideInInspector]
        public List<HexCell> cells;

        private void OnEnable()
        {
            //if (cells != null)
            //    HexMapMgr.Instance.cells = cells;
            //for (int i = 0; i < HexMapMgr.Instance.cells.Count; i++)
            //{
            //    HexMapMgr.Instance.cells[i].Refresh();
            //}
        }
    }
}