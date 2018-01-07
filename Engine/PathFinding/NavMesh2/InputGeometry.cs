using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.PathFinding.NavMesh2
{
    public class InputGeometry
    {
        public const int MaxVolumes = 256;
        public const int MaxOffmeshConnections = 256;

        private ChunkyTriMesh m_chunkyMesh;
        private ConvexVolume[] m_volumes;
        private int m_volumeCount;

        private float[] m_offMeshConVerts;
        private float[] m_offMeshConRads;
        private byte[] m_offMeshConDirs;
        private byte[] m_offMeshConAreas;
        private short[] m_offMeshConFlags;
        private int[] m_offMeshConId;
        int m_offMeshConCount;

        public BoundingBox BoundingBox;
        public IEnumerable<Triangle> Triangles;

        public InputGeometry()
        {
            m_volumes = new ConvexVolume[MaxVolumes];
            m_volumeCount = 0;

            m_offMeshConVerts = new float[MaxOffmeshConnections * 3 * 2];
            m_offMeshConRads = new float[MaxOffmeshConnections];
            m_offMeshConDirs = new byte[MaxOffmeshConnections];
            m_offMeshConAreas = new byte[MaxOffmeshConnections];
            m_offMeshConFlags = new short[MaxOffmeshConnections];
            m_offMeshConId = new int[MaxOffmeshConnections];
            m_offMeshConCount = 0;
        }

        public InputGeometry(IEnumerable<Triangle> triangles) : this()
        {
            this.Triangles = triangles;
            this.BoundingBox = GeometryUtil.CreateBoundingBox(triangles);

            CreateChunkyTriMesh(triangles.ToArray(), 256, out m_chunkyMesh);
        }

        public ChunkyTriMesh GetChunkyMesh()
        {
            return m_chunkyMesh;
        }

        public ConvexVolume[] GetConvexVolumes()
        {
            return m_volumes;
        }

        public int GetConvexVolumeCount()
        {
            return m_volumeCount;
        }

        private bool CreateChunkyTriMesh(Triangle[] tris, int trisPerChunk, out ChunkyTriMesh cm)
        {
            cm = new ChunkyTriMesh();

            int ntris = tris.Count();
            int nchunks = (ntris + trisPerChunk - 1) / trisPerChunk;

            cm.nodes = new ChunkyTriMeshNode[nchunks * 4];

            cm.tris = new int[ntris];
            cm.ntris = ntris;

            // Build tree
            BoundsItem[] items = new BoundsItem[ntris];

            for (int i = 0; i < ntris; i++)
            {
                items[i].i = i;

                var t = tris[i];

                // Calc triangle XZ bounds.
                items[i].bmin.X = items[i].bmax.X = t.Point1.X;
                items[i].bmin.Y = items[i].bmax.Y = t.Point1.Z;

                for (int j = 1; j < 3; ++j)
                {
                    Vector3 v = t[j];
                    if (v.X < items[i].bmin.X) items[i].bmin.X = v.X;
                    if (v.Z < items[i].bmin.Y) items[i].bmin.Y = v.Z;

                    if (v.X > items[i].bmax.X) items[i].bmax.X = v.X;
                    if (v.Z > items[i].bmax.Y) items[i].bmax.Y = v.Z;
                }
            }

            int curNode = 0;
            int curTri = 0;
            Subdivide(
                items,
                0, ntris, trisPerChunk,
                ref curNode, cm.nodes, nchunks * 4,
                ref curTri, cm.tris, tris);

            items = null;
            cm.nnodes = curNode;

            // Calc max tris per node.
            cm.maxTrisPerChunk = 0;
            for (int i = 0; i < cm.nnodes; ++i)
            {
                ChunkyTriMeshNode node = cm.nodes[i];

                bool isLeaf = node.i >= 0;
                if (!isLeaf) continue;
                if (node.n > cm.maxTrisPerChunk)
                {
                    cm.maxTrisPerChunk = node.n;
                }
            }

            return true;
        }

        internal static Triangle[] DebugTris()
        {
            List<float> numbers = new List<float>();

            using (StreamReader rd = new StreamReader(@"./PathFinding/Navmesh2/DEBUGTris.txt"))
            {
                while (!rd.EndOfStream)
                {
                    string strNumber = rd.ReadLine();
                    if (!string.IsNullOrWhiteSpace(strNumber))
                    {
                        numbers.Add(float.Parse(strNumber, System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
            }

            List<Triangle> res = new List<Triangle>();

            for (int i = 0; i < numbers.Count / 9; i++)
            {
                var p1 = new Vector3(numbers[i * 9 + 0], numbers[i * 9 + 1], numbers[i * 9 + 2]);
                var p2 = new Vector3(numbers[i * 9 + 3], numbers[i * 9 + 4], numbers[i * 9 + 5]);
                var p3 = new Vector3(numbers[i * 9 + 6], numbers[i * 9 + 7], numbers[i * 9 + 8]);

                res.Add(new Triangle(p1, p2, p3));
            }

            return res.ToArray();
        }

        private void Subdivide(
            BoundsItem[] items, int imin, int imax, int trisPerChunk,
            ref int curNode, ChunkyTriMeshNode[] nodes, int maxNodes,
            ref int curTri, int[] outTris, Triangle[] inTris)
        {
            int inum = imax - imin;
            int icur = curNode;

            if (curNode > maxNodes)
            {
                return;
            }

            int iNode = curNode++;

            if (inum <= trisPerChunk)
            {
                // Leaf
                Vector2 bmin;
                Vector2 bmax;
                CalcExtends(items, imin, imax, out bmin, out bmax);

                // Copy triangles.
                nodes[iNode].bmin = bmin;
                nodes[iNode].bmax = bmax;
                nodes[iNode].i = curTri;
                nodes[iNode].n = inum;

                for (int i = imin; i < imax; ++i)
                {
                    outTris[curTri] = items[i].i;
                    curTri++;
                }
            }
            else
            {
                // Split
                Vector2 bmin;
                Vector2 bmax;
                CalcExtends(items, imin, imax, out bmin, out bmax);

                nodes[iNode].bmin = bmin;
                nodes[iNode].bmax = bmax;

                int axis = LongestAxis(bmax.X - bmin.X, bmax.Y - bmin.Y);
                if (axis == 0)
                {
                    // Sort along x-axis
                    QuickSort(items, imin, inum, (a, b) =>
                    {
                        if (a.bmin.X < b.bmin.X) return -1;
                        if (a.bmin.X > b.bmin.X) return 1;
                        if (a.i < b.i) return -1;
                        if (a.i > b.i) return 1;
                        return 0;
                    });
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    QuickSort(items, imin, inum, (a, b) =>
                    {
                        if (a.bmin.Y < b.bmin.Y) return -1;
                        if (a.bmin.Y > b.bmin.Y) return 1;
                        if (a.i < b.i) return -1;
                        if (a.i > b.i) return 1;
                        return 0;
                    });
                }

                int isplit = imin + inum / 2;

                // Left
                Subdivide(
                    items, imin, isplit, trisPerChunk,
                    ref curNode, nodes, maxNodes,
                    ref curTri, outTris, inTris);

                // Right
                Subdivide(
                    items, isplit, imax, trisPerChunk,
                    ref curNode, nodes, maxNodes,
                    ref curTri, outTris, inTris);

                int iescape = curNode - icur;

                // Negative index means escape.
                nodes[iNode].i = -iescape;
            }
        }

        private int LongestAxis(float x, float y)
        {
            return y > x ? 1 : 0;
        }

        private void CalcExtends(BoundsItem[] items, int imin, int imax, out Vector2 bmin, out Vector2 bmax)
        {
            bmin.X = items[imin].bmin.X;
            bmin.Y = items[imin].bmin.Y;

            bmax.X = items[imin].bmax.X;
            bmax.Y = items[imin].bmax.Y;

            for (int i = imin + 1; i < imax; ++i)
            {
                if (items[i].bmin.X < bmin.X) bmin.X = items[i].bmin.X;
                if (items[i].bmin.Y < bmin.Y) bmin.Y = items[i].bmin.Y;

                if (items[i].bmax.X > bmax.X) bmax.X = items[i].bmax.X;
                if (items[i].bmax.Y > bmax.Y) bmax.Y = items[i].bmax.Y;
            }
        }


        private static void QuickSort<T>(T[] arr, int begin, int end, Comparison<T> comparison)
        {
            end = arr.Length <= end ? arr.Length - 1 : end;

            T pivot = arr[(begin + (end - begin) / 2)];
            int left = begin;
            int right = end;
            while (left <= right)
            {
                while (comparison.Invoke(arr[left], pivot) < 0)
                {
                    left++;
                }
                while (comparison.Invoke(arr[right], pivot) > 0)
                {
                    right--;
                }
                if (left <= right)
                {
                    Swap(arr, left, right);
                    left++;
                    right--;
                }
            }
            if (begin < right)
            {
                QuickSort(arr, begin, left - 1, comparison);
            }
            if (end > left)
            {
                QuickSort(arr, right + 1, end, comparison);
            }
        }

        static void Swap<T>(T[] items, int x, int y)
        {
            T temp = items[x];
            items[x] = items[y];
            items[y] = temp;
        }

        public static void Quicksort<T>(T[] elements, int start, int end, Comparison<T> comparison)
        {
            end = elements.Length <= end ? elements.Length - 1 : end;

            int i = start;
            int j = end;
            T pivot = elements[(start + end) / 2];

            while (i <= j)
            {
                while (comparison.Invoke(elements[i], pivot) < 0)
                {
                    i++;
                }

                while (comparison.Invoke(elements[j], pivot) > 0)
                {
                    j--;
                }

                if (i <= j)
                {
                    // Swap
                    T tmp = elements[i];
                    elements[i] = elements[j];
                    elements[j] = tmp;

                    i++;
                    j--;
                }
            }

            // Recursive calls
            if (start < j)
            {
                Quicksort(elements, start, j, comparison);
            }

            if (i < end)
            {
                Quicksort(elements, i, end, comparison);
            }
        }
    }
}
