using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HexMap
{
    public static partial class Triangular
    {
        public static Vector2 GetRoadInterpolators(HexDirection direction, HexCell cell)
        {
            Vector2 interpolators;
            if (cell.HasRoadThroughEdge(direction))
            {
                interpolators.x = interpolators.y = 0.5f;
            }
            else
            {
                interpolators.x = cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
                interpolators.y = cell.HasRoadThroughEdge(direction.Next()) ? 0.5f : 0.25f;
            }
            return interpolators;
        }


        static void TriangulateRoadAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
        {
            bool hasRoadThroughEdge = cell.HasRoadThroughEdge(direction);
            bool previousHasRiver = cell.HasRiverThroughEdge(direction.Previous());
            bool nextHasRiver = cell.HasRiverThroughEdge(direction.Next());
            Vector2 interpolators = GetRoadInterpolators(direction, cell);
            Vector3 roadCenter = center;

            if (cell.HasRiverBeginOrEnd)
            {
                roadCenter += HexMetrics.GetSolidEdgeMiddle(cell.RiverBeginOrEndDirection.Opposite()) * (1f / 3f);
            }
            else if (cell.IncomingRiver == cell.OutgoingRiver.Opposite())
            {
                Vector3 corner;
                if (previousHasRiver)
                {
                    if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Next()))
                    {
                        return;
                    }
                    corner = HexMetrics.GetSecondSolidCorner(direction);
                }
                else
                {
                    if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Previous()))
                    {
                        return;
                    }
                    corner = HexMetrics.GetFirstSolidCorner(direction);
                }
                roadCenter += corner * 0.5f;

                if (cell.IncomingRiver == direction.Next() && (cell.HasRoadThroughEdge(direction.Next2()) || cell.HasRoadThroughEdge(direction.Opposite())))
                {
                    features.AddBridge(roadCenter, center - corner * 0.5f);
                }

                center += corner * 0.25f;
            }
            else if (cell.IncomingRiver == cell.OutgoingRiver.Previous())
            {
                roadCenter -= HexMetrics.GetSecondCorner(cell.IncomingRiver) * 0.2f;
            }
            else if (cell.IncomingRiver == cell.OutgoingRiver.Next())
            {
                roadCenter -= HexMetrics.GetFirstCorner(cell.IncomingRiver) * 0.2f;
            }
            else if (previousHasRiver && nextHasRiver)
            {
                if (!hasRoadThroughEdge)
                {
                    return;
                }
                Vector3 offset = HexMetrics.GetSolidEdgeMiddle(direction) * HexMetrics.innerToOuter;
                roadCenter += offset * 0.7f;
                center += offset * 0.5f;
            }
            else
            {
                HexDirection middle;
                if (previousHasRiver)
                {
                    middle = direction.Next();
                }
                else if (nextHasRiver)
                {
                    middle = direction.Previous();
                }
                else
                {
                    middle = direction;
                }
                if (
                    !cell.HasRoadThroughEdge(middle) &&
                    !cell.HasRoadThroughEdge(middle.Previous()) &&
                    !cell.HasRoadThroughEdge(middle.Next())
                )
                {
                    return;
                }
                //roadCenter += HexMetrics.GetSolidEdgeMiddle(middle) * 0.25f;
                Vector3 offset = HexMetrics.GetSolidEdgeMiddle(middle);
                roadCenter += offset * 0.25f;
                if (direction == middle && cell.HasRoadThroughEdge(direction.Opposite()))
                {
                    features.AddBridge(roadCenter, center - offset * (HexMetrics.innerToOuter * 0.7f));
                }
            }
        }
    }
}