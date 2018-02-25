using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Engine.PathFinding.NavMesh2
{
    public class InputGeometry
    {
        public static Triangle[] DebugTris()
        {
            List<Vector3> points = new List<Vector3>();
            List<Int3> indices = new List<Int3>();

            using (StreamReader rd = new StreamReader(@"./PathFinding/Navmesh2/DEBUGTris.obj"))
            {
                while (!rd.EndOfStream)
                {
                    string strLine = rd.ReadLine();
                    if (!string.IsNullOrWhiteSpace(strLine))
                    {
                        if (strLine.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                        {
                            var numbers = strLine.Split(" ".ToArray(), StringSplitOptions.None);

                            points.Add(new Vector3(
                                float.Parse(numbers[1], NumberStyles.Float, CultureInfo.InvariantCulture),
                                float.Parse(numbers[2], NumberStyles.Float, CultureInfo.InvariantCulture),
                                float.Parse(numbers[3], NumberStyles.Float, CultureInfo.InvariantCulture)));
                        }

                        if (strLine.StartsWith("f", StringComparison.OrdinalIgnoreCase))
                        {
                            var numbers = strLine.Split(" ".ToArray(), StringSplitOptions.None);

                            indices.Add(new Int3(
                                int.Parse(numbers[1], NumberStyles.Integer, CultureInfo.InvariantCulture),
                                int.Parse(numbers[2], NumberStyles.Integer, CultureInfo.InvariantCulture),
                                int.Parse(numbers[3], NumberStyles.Integer, CultureInfo.InvariantCulture)));
                        }
                    }
                }
            }

            Triangle[] res = new Triangle[indices.Count];

            for (int i = 0; i < indices.Count; i++)
            {
                res[i] = new Triangle(
                    points[indices[i].X - 1],
                    points[indices[i].Y - 1],
                    points[indices[i].Z - 1]);
            }

            return res;
        }

        public const int MaxVolumes = 256;
        public const int MaxOffmeshConnections = 256;

        private BoundsItemComparerX xComparer = new BoundsItemComparerX();
        private BoundsItemComparerY yComparer = new BoundsItemComparerY();

        private ChunkyTriMesh m_chunkyMesh;
        private ConvexVolume[] m_volumes;
        private int m_volumeCount;

        private float[] m_offMeshConVerts;
        private float[] m_offMeshConRads;
        private byte[] m_offMeshConDirs;
        private SamplePolyAreas[] m_offMeshConAreas;
        private SamplePolyFlags[] m_offMeshConFlags;
        private uint[] m_offMeshConId;
        private int m_offMeshConCount;

        public readonly BoundingBox BoundingBox;

        public InputGeometry()
        {
            m_volumes = new ConvexVolume[MaxVolumes];
            m_volumeCount = 0;

            m_offMeshConVerts = new float[MaxOffmeshConnections * 3 * 2];
            m_offMeshConRads = new float[MaxOffmeshConnections];
            m_offMeshConDirs = new byte[MaxOffmeshConnections];
            m_offMeshConAreas = new SamplePolyAreas[MaxOffmeshConnections];
            m_offMeshConFlags = new SamplePolyFlags[MaxOffmeshConnections];
            m_offMeshConId = new uint[MaxOffmeshConnections];
            m_offMeshConCount = 0;
        }
        public InputGeometry(IEnumerable<Triangle> triangles) : this()
        {
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
        public int GetOffMeshConnectionCount()
        {
            return m_offMeshConCount;
        }
        public uint[] GetOffMeshConnectionId()
        {
            return m_offMeshConId;
        }
        public SamplePolyFlags[] GetOffMeshConnectionFlags()
        {
            return m_offMeshConFlags;
        }
        public SamplePolyAreas[] GetOffMeshConnectionAreas()
        {
            return m_offMeshConAreas;
        }
        public byte[] GetOffMeshConnectionDirs()
        {
            return m_offMeshConDirs;
        }
        public float[] GetOffMeshConnectionRads()
        {
            return m_offMeshConRads;
        }
        public float[] GetOffMeshConnectionVerts()
        {
            return m_offMeshConVerts;
        }

        private bool CreateChunkyTriMesh(Triangle[] tris, int trisPerChunk, out ChunkyTriMesh cm)
        {
            cm = new ChunkyTriMesh();

            int ntris = tris.Count();
            int nchunks = (ntris + trisPerChunk - 1) / trisPerChunk;

            cm.triangles = tris;

            cm.nodes = new ChunkyTriMeshNode[nchunks * 4];

            cm.tris = new int[ntris];
            cm.ntris = ntris;

            // Build tree
            BoundsItem[] items = new BoundsItem[ntris];

            for (int i = 0; i < ntris; i++)
            {
                var t = tris[i];

                // Calc triangle XZ bounds.
                var bbox = BoundingBox.FromPoints(t.GetVertices());

                items[i].i = i;
                items[i].bmin = new Vector2(bbox.Minimum.X, bbox.Minimum.Z);
                items[i].bmax = new Vector2(bbox.Maximum.X, bbox.Maximum.Z);
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

            int iNode = curNode++;

            Vector2 bmin;
            Vector2 bmax;
            CalcExtends(items, imin, imax, out bmin, out bmax);

            nodes[iNode].bmin = bmin;
            nodes[iNode].bmax = bmax;

            if (inum <= trisPerChunk)
            {
                // Leaf

                // Copy triangles.
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

                int axis = LongestAxis(bmax.X - bmin.X, bmax.Y - bmin.Y);
                if (axis == 0)
                {
                    // Sort along x-axis
                    Array.Sort(items, imin, inum, xComparer);
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort(items, imin, inum, yComparer);
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
    }
}
