using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using Unity.Mathematics;

namespace HexMap
{
    public partial class HexMapMgr
    {


        public List<HexCell> GetRing(HexCell cell, int radius)
        {
            List<HexCell> results = new List<HexCell>();

            List<Hexagon> ring = cell.hexagon.SingleRing(radius);
            for (int i = 0; i < ring.Count; i++)
            {
                int2 xz = Hexagon.ToXZ(ring[i]);
                int id = xz.x + xz.y * Data.cellCountX;
                if (id <= 0 || id > Data.cellCount)
                    continue;
                results.Add(cells[id]);
            }

            return results;
        }


    }
}