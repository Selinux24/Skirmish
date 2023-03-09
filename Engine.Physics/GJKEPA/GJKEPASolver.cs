/* Copyright <2021> <Thorben Linneweber>
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
* 
*/
using SharpDX;
using System;

namespace Engine.Physics.GJKEPA
{

    public class GJKEPASolver
    {
        struct Triangle
        {
            public short A, B, C;
            public bool FacingOrigin;

            public short this[int i]
            {
                get { return i == 0 ? A : (i == 1 ? B : C); }
            }

            public Vector3 Normal;
            public Vector3 ClosestToOrigin;

            public float NormalSq;
            public float ClosestToOriginSq;
        }

        struct Edge
        {
            public short A;
            public short B;

            public Edge(short a, short b)
            {
                A = a;
                B = b;
            }

            public static bool Equals(in Edge a, in Edge b)
            {
                return ((a.A == b.A && a.B == b.B) || (a.A == b.B && a.B == b.A));
            }
        }

        [ThreadStatic]
        private static GJKEPASolver epaSolver;

        public Statistics Statistics = new Statistics();
        public MinkowskiDifference MKD = new MinkowskiDifference();

        private const float scale = 1e-4f;
        private readonly Vector3 v0 = scale * new Vector3((float)Math.Sqrt(8.0f / 9.0f), 0.0f, -1.0f / 3.0f);
        private readonly Vector3 v1 = scale * new Vector3(-(float)Math.Sqrt(2.0f / 9.0f), (float)Math.Sqrt(2.0f / 3.0f), -1.0f / 3.0f);
        private readonly Vector3 v2 = scale * new Vector3(-(float)Math.Sqrt(2.0f / 9.0f), -(float)Math.Sqrt(2.0f / 3.0f), -1.0f / 3.0f);
        private readonly Vector3 v3 = scale * new Vector3(0.0f, 0.0f, 1.0f);

        private const double NumericEpsilon = 1e-24d;
        private const double CollideEpsilon = 1e-6d;
        private const int MaxIter = 85;

        private const int MaxVertices = MaxIter + 4;
        private const int MaxTriangles = 3 * MaxVertices;

        private readonly Triangle[] Triangles = new Triangle[MaxTriangles];
        private readonly Vector3[] Vertices = new Vector3[MaxVertices];
        private readonly Vector3[] VerticesA = new Vector3[MaxVertices];
        private readonly Vector3[] VerticesB = new Vector3[MaxVertices];

        private readonly Edge[] edges = new Edge[256];

        private short vPointer = 0;
        private short tCount = 0;

        private bool originEnclosed = false;
        private Vector3 center;

        public static bool Detect(
            ISupportMappable supportA, ISupportMappable supportB, Matrix orientationA, Matrix orientationB, Vector3 positionA, Vector3 positionB,
            out Vector3 pointA, out Vector3 pointB, out Vector3 normal, out float separation)
        {
            epaSolver ??= new GJKEPASolver();

            epaSolver.MKD.SupportA = supportA;
            epaSolver.MKD.SupportB = supportB;
            epaSolver.MKD.OrientationA = orientationA;
            epaSolver.MKD.OrientationB = orientationB;
            epaSolver.MKD.PositionA = positionA;
            epaSolver.MKD.PositionB = positionB;

            return epaSolver.Solve(out pointA, out pointB, out normal, out separation);
        }

        private bool CalcBarycentric(Triangle tri, out Vector3 result, bool clamp = false)
        {
            Vector3 a = Vertices[tri.A];
            Vector3 b = Vertices[tri.B];
            Vector3 c = Vertices[tri.C];

            bool clamped = false;

            // Calculate the barycentric coordinates of the origin (0,0,0) projected
            // onto the plane of the triangle.
            // 
            // [W. Heidrich, Journal of Graphics, GPU, and Game Tools,Volume 10, Issue 3, 2005.]

            Vector3.Subtract(ref a, ref b, out Vector3 u);
            Vector3.Subtract(ref a, ref c, out Vector3 v);

            float t = tri.NormalSq;
            Vector3.Cross(ref u, ref a, out Vector3 tmp);
            float gamma = Vector3.Dot(tmp, tri.Normal) / t;
            Vector3.Cross(ref a, ref v, out tmp);
            float beta = Vector3.Dot(tmp, tri.Normal) / t;
            float alpha = 1f - gamma - beta;

            if (clamp)
            {
                // Clamp the projected barycentric coordinates to lie within the triangle,
                // such that the clamped coordinates are closest (euclidean) to the original point.
                //
                // [https://math.stackexchange.com/questions/
                //  1092912/find-closest-point-in-triangle-given-barycentric-coordinates-outside]

                if (alpha >= 0f && beta < 0f)
                {
                    t = Vector3.Dot(a, u);
                    if ((gamma < 0f) && (t > 0f))
                    {
                        beta = Math.Min(1f, t / u.LengthSquared());
                        alpha = 1f - beta;
                        gamma = 0f;
                    }
                    else
                    {
                        gamma = Math.Min(1f, Math.Max(0f, Vector3.Dot(a, v) / v.LengthSquared()));
                        alpha = 1f - gamma;
                        beta = 0f;
                    }

                    clamped = true;
                }
                else if (beta >= 0f && gamma < 0f)
                {
                    Vector3.Subtract(ref b, ref c, out Vector3 w);
                    t = Vector3.Dot(b, w);
                    if ((alpha < 0f) && (t > 0f))
                    {
                        gamma = Math.Min(1f, t / w.LengthSquared());
                        beta = 1f - gamma;
                        alpha = 0f;
                    }
                    else
                    {
                        alpha = Math.Min(1f, Math.Max(0f, -Vector3.Dot(b, u) / u.LengthSquared()));
                        beta = 1f - alpha;
                        gamma = 0f;
                    }

                    clamped = true;
                }
                else if (gamma >= 0f && alpha < 0f)
                {
                    Vector3.Subtract(ref b, ref c, out Vector3 w);
                    t = -Vector3.Dot(c, v);
                    if ((beta < 0f) && (t > 0f))
                    {
                        alpha = Math.Min(1f, t / v.LengthSquared());
                        gamma = 1f - alpha;
                        beta = 0f;
                    }
                    else
                    {
                        beta = Math.Min(1f, Math.Max(0f, -Vector3.Dot(c, w) / w.LengthSquared()));
                        gamma = 1f - beta;
                        alpha = 0f;
                    }

                    clamped = true;
                }

            }

            result.X = alpha;
            result.Y = beta;
            result.Z = gamma;

            return clamped;
        }

        private void ConstructInitialTetrahedron()
        {
            vPointer = 3;

            Vertices[0] = v0 + center;
            Vertices[1] = v1 + center;
            Vertices[2] = v2 + center;
            Vertices[3] = v3 + center;

            CreateTriangle(0, 2, 1);
            CreateTriangle(0, 1, 3);
            CreateTriangle(0, 3, 2);
            CreateTriangle(1, 2, 3);
        }

        private bool IsLit(int candidate, int w)
        {
            ref Triangle tr = ref Triangles[candidate];
            Vector3 deltaA = Vertices[w] - Vertices[tr.A];
            return Vector3.Dot(deltaA, tr.Normal) > 0;
        }

        private bool CreateTriangle(short a, short b, short c)
        {
            ref Triangle triangle = ref Triangles[tCount];
            triangle.A = a; triangle.B = b; triangle.C = c;

            Vector3.Subtract(ref Vertices[a], ref Vertices[b], out Vector3 u);
            Vector3.Subtract(ref Vertices[a], ref Vertices[c], out Vector3 v);
            Vector3.Cross(ref u, ref v, out triangle.Normal);
            triangle.NormalSq = triangle.Normal.LengthSquared();

            // no need to add degenerate triangles
            if (triangle.NormalSq < NumericEpsilon) return false;

            // do we need to flip the triangle? (the origin of the md has to be enclosed)
            float delta = Vector3.Dot(triangle.Normal, Vertices[a] - center);

            if (delta < 0)
            {
                (triangle.A, triangle.B) = (triangle.B, triangle.A);
                triangle.Normal = -triangle.Normal;
            }

            delta = Vector3.Dot(triangle.Normal, Vertices[a]);
            triangle.FacingOrigin = delta > 0.0d;

            if (!originEnclosed && CalcBarycentric(triangle, out Vector3 bc, true))
            {
                triangle.ClosestToOrigin = bc.X * Vertices[triangle.A] + bc.Y * Vertices[triangle.B] + bc.Z * Vertices[triangle.C];
                triangle.ClosestToOriginSq = triangle.ClosestToOrigin.LengthSquared();
            }
            else
            {
                // Prefer point-plane distance calculation if possible.
                Vector3.Multiply(ref triangle.Normal, delta / triangle.NormalSq, out triangle.ClosestToOrigin);
                triangle.ClosestToOriginSq = triangle.ClosestToOrigin.LengthSquared();
            }

            tCount++;
            return true;
        }

        private bool Solve(out Vector3 point1, out Vector3 point2, out Vector3 normal, out float separation)
        {
            tCount = 0;
            originEnclosed = false;

            MKD.SupportCenter(out center);
            ConstructInitialTetrahedron();

            int iter = 0;
            Triangle ctri; // closest Triangle

            while (++iter < MaxIter)
            {
                Statistics.Iterations = iter;

                // search for the closest triangle and check if the origin is enclosed
                int closestIndex = -1;
                double currentMin = double.MaxValue;
                originEnclosed = true;

                for (int i = 0; i < tCount; i++)
                {
                    if (Triangles[i].ClosestToOriginSq < currentMin)
                    {
                        currentMin = Triangles[i].ClosestToOriginSq;
                        closestIndex = i;
                    }

                    if (!Triangles[i].FacingOrigin) originEnclosed = false;
                }

                ctri = Triangles[closestIndex];
                Vector3 searchDir = ctri.ClosestToOrigin;
                if (originEnclosed) searchDir = -searchDir;

                if (ctri.ClosestToOriginSq < NumericEpsilon)
                {
                    searchDir = ctri.Normal;
                }

                vPointer++;
                MKD.Support(searchDir, out VerticesA[vPointer], out VerticesB[vPointer], out Vertices[vPointer]);

                // Termination condition
                //     c = Triangles[Head].ClosestToOrigin (closest point on the polytope)
                //     v = Vertices[vPointer] (support point)
                //     e = CollideEpsilon
                // The termination condition reads: 
                //     abs(dot(normalize(c), v - c)) < e
                //     <=>  abs(dot(c, v - c))/len(c) < e <=> abs((dot(c, v) - dot(c,c)))/len(c) < e
                //     <=>  (dot(c, v) - dot(c,c))^2 < e^2*c^2 <=> (dot(c, v) - c^2)^2 < e^2*c^2
                double deltaDist = ctri.ClosestToOriginSq - Vector3.Dot(Vertices[vPointer], ctri.ClosestToOrigin);

                if (deltaDist * deltaDist < CollideEpsilon * CollideEpsilon * ctri.ClosestToOriginSq)
                {
                    goto converged;
                }

                int ePointer = 0;
                for (int index = tCount; index-- > 0;)
                {
                    if (!IsLit(index, vPointer)) continue;
                    Edge edge; bool added;

                    for (int k = 0; k < 3; k++)
                    {
                        edge = new(Triangles[index][(k + 0) % 3], Triangles[index][(k + 1) % 3]);
                        added = true;
                        for (int e = ePointer; e-- > 0;)
                        {
                            if (Edge.Equals(edges[e], edge))
                            {
                                edges[e] = edges[--ePointer];
                                added = false;
                            }
                        }
                        if (added) edges[ePointer++] = edge;
                    }
                    Triangles[index] = Triangles[--tCount];
                }

                for (int i = 0; i < ePointer; i++)
                {
                    if (!CreateTriangle(edges[i].A, edges[i].B, vPointer))
                        goto converged;
                }

                if (ePointer > 0) continue;

                converged:
                separation = (float)Math.Sqrt(ctri.ClosestToOriginSq);
                if (originEnclosed) separation *= -1.0f;

                Statistics.Accuracy = Math.Abs(deltaDist / separation);

                CalcBarycentric(ctri, out Vector3 bc, !originEnclosed);

                point1 = bc.X * VerticesA[ctri.A] + bc.Y * VerticesA[ctri.B] + bc.Z * VerticesA[ctri.C];
                point2 = bc.X * VerticesB[ctri.A] + bc.Y * VerticesB[ctri.B] + bc.Z * VerticesB[ctri.C];

                normal = ctri.Normal * (1.0f / (float)Math.Sqrt(ctri.NormalSq));

                return true;
            }

            point1 = point2 = normal = Vector3.Zero;
            separation = 0.0f;

            System.Diagnostics.Debug.WriteLine($"EPA: Could not converge within {MaxIter} iterations.");

            return false;
        }
    }
}
