using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
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

            ChunkyTriMeshNode node = new ChunkyTriMeshNode();

            if (inum <= trisPerChunk)
            {
                // Leaf
                CalcExtends(items, imin, imax, out node.bmin, out node.bmax);

                // Copy triangles.
                node.i = curTri;
                node.n = inum;

                for (int i = imin; i < imax; ++i)
                {
                    outTris[curTri] = items[i].i;
                    curTri++;
                }
            }
            else
            {
                // Split
                CalcExtends(items, imin, imax, out node.bmin, out node.bmax);

                int axis = LongestAxis(node.bmax.X - node.bmin.X, node.bmax.Y - node.bmin.Y);
                if (axis == 0)
                {
                    // Sort along x-axis
                    Array.Sort(items, (a, b) =>
                    {
                        if (a.bmin.X < b.bmin.X) return -1;
                        if (a.bmin.X > b.bmin.X) return 1;
                        return 0;
                    });
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort(items, (a, b) =>
                    {
                        if (a.bmin.Y < b.bmin.Y) return -1;
                        if (a.bmin.Y > b.bmin.Y) return 1;
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
                node.i = -iescape;
            }

            nodes[curNode++] = node;
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
    }
}
