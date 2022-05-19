using UnityEngine;

namespace HexMap
{
    public static partial class Triangular
    {

        static Color weights1 = new Color(1f, 0f, 0f);
        static Color weights2 = new Color(0f, 1f, 0f);
        static Color weights3 = new Color(0f, 0f, 1f);

        static HexMesh terrain;
        static HexMesh rivers;

        static HexMesh water;
        static HexMesh waterShore;
        static HexMesh estuaries;
        static HexFeature features;

        public static void Init(HexMesh terrain, HexMesh rivers, HexMesh roads, HexMesh water, HexMesh waterShore, HexMesh estuaries, HexFeature features)
        {
            Triangular.terrain = terrain;
            Triangular.rivers = rivers;
            Triangular.water = water;
            Triangular.waterShore = waterShore;
            Triangular.estuaries = estuaries;
            Triangular.features = features;
        }

        public static void Init(HexChunk chunk)
        {
            Triangular.terrain = chunk.Terrain;
            Triangular.rivers = chunk.Rivers;
            Triangular.water = chunk.Water;
            Triangular.waterShore = chunk.WaterShore;
            Triangular.estuaries = chunk.Estuary;
            Triangular.features = chunk.Features;
        }
        public static void Triangulate(HexCell cell)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                Triangulate(d, cell);
            }

            if (!cell.IsUnderwater)
            {
                if (!cell.HasRiver && !cell.HasRoads)
                {
                    features.AddFeature(cell, cell.Position);
                }
                if (cell.IsSpecial)
                {
                    features.AddSpecialFeature(cell, cell.Position);
                }
            }
        }

        public static void Triangulate(HexDirection direction, HexCell cell)
        {
            Vector3 center = cell.Position;
            EdgeVertices e = new EdgeVertices(center + HexMetrics.GetFirstSolidCorner(direction), center + HexMetrics.GetSecondSolidCorner(direction));

            if (cell.HasRiver)
            {
                if (cell.HasRiverThroughEdge(direction))
                {
                    e.v3.y = cell.StreamBedY;
                    if (cell.HasRiverBeginOrEnd)
                    {
                        TriangulateWithRiverBeginOrEnd(direction, cell, center, e);
                    }
                    else
                    {
                        TriangulateWithRiver(direction, cell, center, e);
                    }
                }
                else
                {
                    TriangulateAdjacentToRiver(direction, cell, center, e);
                }
            }
            else
            {
                TriangulateWithoutRiver(direction, cell, center, e);

                if (!cell.IsUnderwater && !cell.HasRoadThroughEdge(direction))
                {
                    features.AddFeatureRandom(cell, (center + e.v1 + e.v5) * (1f / 3f));
                }
            }

            if (direction <= HexDirection.SE)
            {
                TriangulateConnection(direction, cell, e);
            }

            if (cell.IsUnderwater)
            {
                TriangulateWater(direction, cell, center);
            }
        }

        static void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1)
        {
            HexCell neighbor = cell.GetNeighbor(direction);
            if (neighbor == null)
            {
                return;
            }

            Vector3 bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.Position.y - cell.Position.y;
            EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

            bool hasRiver = cell.HasRiverThroughEdge(direction);
            bool hasRoad = cell.HasRoadThroughEdge(direction);
            if (hasRiver)
            {

                e2.v3.y = neighbor.StreamBedY;
                //迷雾
                Vector3 indices;
                indices.x = indices.z = cell.id;
                indices.y = neighbor.id;

                if (!cell.IsUnderwater)
                {
                    if (!neighbor.IsUnderwater)
                    {
                        TriangulateRiverQuad(e1.v2, e1.v4, e2.v2, e2.v4, cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f, cell.HasIncomingRiver && cell.IncomingRiver == direction, indices);
                    }
                    else if (cell.Elevation > neighbor.WaterLevel)
                    {
                        TriangulateWaterfallInWater(e1.v2, e1.v4, e2.v2, e2.v4, cell.RiverSurfaceY, neighbor.RiverSurfaceY, neighbor.WaterSurfaceY, indices);
                    }
                }
                else if (!neighbor.IsUnderwater && neighbor.Elevation > cell.WaterLevel)
                {
                    TriangulateWaterfallInWater(e2.v4, e2.v2, e1.v4, e1.v2, neighbor.RiverSurfaceY, cell.RiverSurfaceY, cell.WaterSurfaceY, indices);
                }
            }

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerraces(e1, cell, e2, neighbor, hasRoad);
            }
            else
            {
                TriangulateEdgeStrip(e1, weights1, cell.id, e2, weights2, neighbor.id, hasRoad);
            }

            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            if (direction <= HexDirection.E && nextNeighbor != null)
            {
                Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbor.Position.y;

                if (cell.Elevation <= neighbor.Elevation)
                {
                    if (cell.Elevation <= nextNeighbor.Elevation)
                    {
                        TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                    }
                    else
                    {
                        TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                    }
                }
                else if (neighbor.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                }
            }
        }

        static void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope)
            {
                if (rightEdgeType == HexEdgeType.Slope)
                {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
                else if (rightEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                }
                else
                {
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            else if (rightEdgeType == HexEdgeType.Slope)
            {
                if (leftEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                if (leftCell.Elevation < rightCell.Elevation)
                {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                }
            }
            else
            {
                terrain.AddTriangle(bottom, left, right);
                //迷雾
                Vector3 indices;
                indices.x = bottomCell.id;
                indices.y = leftCell.id;
                indices.z = rightCell.id;
                terrain.AddTriangleCellData(indices, weights1, weights2, weights3);
            }
        }

        static void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell, bool hasRoad)
        {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color w2 = HexMetrics.TerraceLerp(weights1, weights2, 1);
            float i1 = beginCell.id;
            float i2 = endCell.id;

            TriangulateEdgeStrip(begin, weights1, i1, e2, w2, i2, hasRoad);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                EdgeVertices e1 = e2; 
                Color w1 = w2;
                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                w2 = HexMetrics.TerraceLerp(weights1, weights2, i);
                TriangulateEdgeStrip(e1, w1, i1, e2, w2, i2, hasRoad);
            }

            TriangulateEdgeStrip(e2, w2, i1, end, weights2, i2, hasRoad);
        }

        static void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
            Color w3 = HexMetrics.TerraceLerp(weights1, weights2, 1);
            Color w4 = HexMetrics.TerraceLerp(weights1, weights3, 1);
            Vector3 indices;
            indices.x = beginCell.id;
            indices.y = leftCell.id;
            indices.z = rightCell.id;

            terrain.AddTriangle(begin, v3, v4);
            terrain.AddTriangleCellData(indices, weights1, w3, w4);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color w1 = w3;
                Color w2 = w4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                w3 = HexMetrics.TerraceLerp(weights1, weights2, i);
                w4 = HexMetrics.TerraceLerp(weights1, weights3, i);
                terrain.AddQuad(v1, v2, v3, v4);
                terrain.AddQuadCellData(indices, w1, w2, w3, w4);
            }

            terrain.AddQuad(v3, v4, left, right);
            terrain.AddQuadCellData(indices, w3, w4, weights2, weights3);
        }


        static void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);

            Color boundaryWeights = Color.Lerp(weights1, weights3, b);
            Vector3 indices;
            indices.x = beginCell.id;
            indices.y = leftCell.id;
            indices.z = rightCell.id;

            TriangulateBoundaryTriangle(begin, weights1, left, weights2, boundary, boundaryWeights, indices);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeights, indices);
            }
            else
            {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
            }
        }



        static void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
            Color boundaryWeights = Color.Lerp(weights1, weights2, b);
            Vector3 indices;
            indices.x = beginCell.id;
            indices.y = leftCell.id;
            indices.z = rightCell.id;

            TriangulateBoundaryTriangle(right, weights3, begin, weights1, boundary, boundaryWeights, indices);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeights, indices);
            }
            else
            {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
            }
        }

        static void TriangulateBoundaryTriangle(Vector3 begin, Color beginWeights, Vector3 left, Color leftWeights, Vector3 boundary, Color boundaryWeights, Vector3 indices)
        {
            Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);

            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
            terrain.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                Vector3 v1 = v2;
                Color w1 = w2;
                v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);
                terrain.AddTriangleUnperturbed(v1, v2, boundary);
                terrain.AddTriangleCellData(indices, w1, w2, boundaryWeights);
            }

            terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
            terrain.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
        }



        static void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float index)
        {
            terrain.AddTriangle(center, edge.v1, edge.v2);
            terrain.AddTriangle(center, edge.v2, edge.v3);
            terrain.AddTriangle(center, edge.v3, edge.v4);
            terrain.AddTriangle(center, edge.v4, edge.v5);

            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            terrain.AddTriangleCellData(indices, weights1);
            terrain.AddTriangleCellData(indices, weights1);
            terrain.AddTriangleCellData(indices, weights1);
            terrain.AddTriangleCellData(indices, weights1);
        }


        static void TriangulateEdgeStrip(EdgeVertices e1, Color w1, float index1, EdgeVertices e2, Color w2, float index2, bool hasRoad = false)
        {
            terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;
            terrain.AddQuadCellData(indices, w1, w2);
            terrain.AddQuadCellData(indices, w1, w2);
            terrain.AddQuadCellData(indices, w1, w2);
            terrain.AddQuadCellData(indices, w1, w2);

            //if (hasRoad)
            //{
            //    TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4, w1, w2, indices);
            //}
        }
    }
}