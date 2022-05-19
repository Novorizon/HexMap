using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

#pragma warning disable IDE1006 // 命名样式
/// <summary>
/// An abstract hexagon description with cube croodnate.
/// refer to: https://www.redblobgames.com/grids/hexagons/#basics
/// </summary
/// 
namespace HexMap
{

    [Serializable]
    public struct Hexagon : IEquatable<Hexagon>
    {

        public readonly int3 qrs;

        public Hexagon(int q, int r, int s)
        {
            qrs = new int3(q, r, s);
        }

        public Hexagon(int3 qrs)
        {
            this.qrs = qrs;
        }

        public Hexagon(int q, int s)
        {
            qrs = new int3(q, 1 - q - s, s);
        }

        public static Hexagon FromXZ(int x, int z)
        {
            x = x - z / 2;
            return new Hexagon(x, 1 - x - z, z);
        }

        public static Hexagon FromPosition(float3 position)
        {
            float x = position.x / (HexMetrics.innerRadius * 2);
            float y = -x;
            float offset = position.z / (HexMetrics.outerRadius * 3f);
            x -= offset;
            y -= offset;

            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);

            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                if (dX > dY && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }

            return new Hexagon(iX, iZ);
        }
        public static int2 ToXZ(Hexagon hex)
        {
            int x = hex.q + hex.s / 2;
            return new int2(x, hex.s);
        }

        public int q => qrs.x;
        public int r => qrs.y;
        public int s => qrs.z;


        private static readonly int3[] direction_vectors = new int3[]
        {
            new int3(+1, 0, -1), new int3(+1, -1, 0), new int3(0, -1, +1),
            new int3(-1, 0, +1), new int3(-1, +1, 0), new int3(0, +1, -1)
        };

        private static readonly int3[] diagonal_vectors = new int3[]
        {
            new int3(+2, -1, -1), new int3(+1, -2, +1), new int3(-1, -1, +2),
            new int3(-2, +1, +1), new int3(-1, +2, -1), new int3(+1, +1, -2),
        };

        public int length => (math.abs(q) + math.abs(r) + math.abs(s)) / 2;
        public Hexagon normalized => new Hexagon(q / math.abs(q), r / math.abs(r), s / math.abs(s));

        public int DistanceTo(Hexagon other)
        {
            return ((q < other.q ? other.q - q : q - other.q) + (r < other.r ? other.r - r : r - other.r) + (s < other.s ? other.s - s : s - other.s)) / 2;
        }
        public string ToStringOnSpearateLines() { return q.ToString() + "\n" + r.ToString() + "\n" + s.ToString(); }

        public override string ToString() => $"Hexagon({q},{r},{s})";
        public override int GetHashCode() => (int)math.hash(qrs);
        public bool Equals(Hexagon other) => other.q == q && other.r == r && other.s == s;
        public override bool Equals(object obj) => obj is Hexagon converted && Equals(converted);

        public static bool operator ==(Hexagon a, Hexagon b) => a.Equals(b);
        public static bool operator !=(Hexagon a, Hexagon b) => !a.Equals(b);
        public static Hexagon operator +(Hexagon a, Hexagon b) => new Hexagon(a.qrs + b.qrs);
        public static Hexagon operator -(Hexagon a, Hexagon b) => new Hexagon(a.qrs - b.qrs);
        public static Hexagon operator *(Hexagon a, int k) => new Hexagon(a.qrs * k);
        public static Hexagon operator *(int k, Hexagon a) => new Hexagon(k * a.qrs);
        public static int Distance(Hexagon a, Hexagon b) => (a - b).length;

        public static Hexagon Round(double3 h)
        {
            int q = (int)math.round(h.x);
            int r = (int)math.round(h.y);
            int s = (int)math.round(h.z);

            double q_diff = math.abs(q - h.x);
            double r_diff = math.abs(r - h.y);
            double s_diff = math.abs(s - h.z);

            if (q_diff > r_diff && q_diff > s_diff)
                q = -r - s;
            else if (r_diff > s_diff)
                r = -q - s;
            else
                s = -q - r;

            return new Hexagon(q, r, s);
        }

        /// <summary>
        /// A hexagon line from a to b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Hexagon[] Line(Hexagon a, Hexagon b)
        {
            UnityEngine.Debug.Assert(a != b);
            var N = Distance(a, b);
            var array = new Hexagon[N + 1];

            double3 a_nudge = new double3(a.q + 1e-6, a.r + 1e-6, a.s - 2e-6);
            double3 b_nudge = new double3(b.q + 1e-6, b.r + 1e-6, b.s - 2e-6);

            for (int i = 0; i <= N; ++i)
            {
                array[i] = Round(math.lerp(a_nudge, b_nudge, 1.0f / N * i));
            }

            return array;
        }

        public static Hexagon[] Line(Hexagon a, Hexagon b, int width)
        {
            UnityEngine.Debug.Assert(width > 0);
            var line = Line(a, b);
            int count = (width - 1) >> 1;
            if (count == 0)
            {
                return line;
            }
            else
            {
                Hexagon a2 = line[1];
                Hexagon b2 = line[line.Length - 2];

                int dirA = a.NeighborDirection(a2);
                int dirB = b.NeighborDirection(b2);

                var L = NeighborLine(a, b, dirA, dirB, 1, count - 1, true);
                var R = NeighborLine(a, b, dirA, dirB, 1, count - 1, false);

                return line.Concat(L).Concat(R).ToArray();
            }
        }

        private static Hexagon[] NeighborLine(Hexagon originA, Hexagon originB, int originDirA, int originDirB, int depth, int count, bool isLeft)
        {
            int dirA, dirB;

            if (depth % 2 == 1)
            {
                if (isLeft)
                {
                    dirA = (originDirA + 1) % 6;
                    dirB = (originDirB + 5) % 6;
                }
                else
                {
                    dirA = (originDirA + 5) % 6;
                    dirB = (originDirB + 1) % 6;
                }
            }
            else
            {
                if (isLeft)
                {
                    dirA = (originDirA + 2) % 6;
                    dirB = (originDirB + 4) % 6;
                }
                else
                {
                    dirA = (originDirA + 4) % 6;
                    dirB = (originDirB + 2) % 6;
                }
            }

            var a = originA.Neighbor(dirA);
            var b = originB.Neighbor(dirB);

            var line = Line(a, b);

            if (count == 0)
                return line;
            else
                return line.Concat(NeighborLine(a, b, originDirA, originDirB, depth + 1, count - 1, isLeft)).ToArray();
        }

        /// <summary>
        /// pointy topped: E, NE, NW, W, SW, SE;
        /// flat topped: SE, NE, N, NW, SW, S
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Hexagon Direction(int dir) => new Hexagon(direction_vectors[dir]);

        /// <summary>
        /// pointy topped: NE, N, NW, SW, S, SE;
        /// flat topped: E, NE, NW, W, SW, SE;
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Hexagon Diagonal(int dir) => new Hexagon(diagonal_vectors[dir]);

        /// <summary>
        /// pointy topped: E, NE, NW, W, SW, SE;
        /// flat topped: SE, NE, N, NW, SW, S
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Hexagon Neighbor(int dir) => new Hexagon(qrs + direction_vectors[dir]);

        /// <summary>
        /// pointy topped: NE, N, NW, SW, S, SE;
        /// flat topped: E, NE, NW, W, SW, SE;
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Hexagon DiagonalNeighbor(int dir) => new Hexagon(qrs + diagonal_vectors[dir]);

        public int NeighborDirection(Hexagon h)
        {
            for (int i = 0; i < 6; i++)
            {
                if (h == Neighbor(i))
                    return i;
            }

            return -1;
        }

        public int DiagonalNeighborDirection(Hexagon h)
        {
            for (int i = 0; i < 6; i++)
            {
                if (h == DiagonalNeighbor(i))
                    return i;
            }

            return -1;
        }

        public Hexagon ReflectQ => new Hexagon(q, s, r);
        public Hexagon ReflectR => new Hexagon(s, r, q);
        public Hexagon ReflectS => new Hexagon(r, q, s);

        /// <summary>
        /// A hexagon single ring with specified radius
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public List<Hexagon> SingleRing(int radius)
        {
            List<Hexagon> result = new List<Hexagon>();
            var hex = this + Direction(4) * radius;
            for (int i = 0; i < 6; ++i)
            {
                for (int j = 0; j < radius; ++j)
                {
                    result.Add(hex);
                    hex = hex.Neighbor(i);
                }
            }

            return result;
        }
    }

}
#pragma warning restore IDE1006 // 命名样式
