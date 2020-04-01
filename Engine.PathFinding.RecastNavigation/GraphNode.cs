using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Graph node
    /// </summary>
    public class GraphNode : IGraphNode
    {
        /// <summary>
        /// Gets a graph node list from a navigation mesh
        /// </summary>
        /// <param name="mesh">Navigation mesh</param>
        /// <returns>Returns graph node</returns>
        public static GraphNode[] Build(NavMesh mesh)
        {
            List<GraphNode> nodes = new List<GraphNode>();

            for (int i = 0; i < mesh.MaxTiles; ++i)
            {
                var tile = mesh.Tiles[i];
                if (tile.Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
                {
                    continue;
                }

                for (int t = 0; t < tile.Header.PolyCount; t++)
                {
                    var p = tile.Polys[t];
                    if (p.Type == PolyTypes.OffmeshConnection)
                    {
                        continue;
                    }

                    var bse = mesh.GetTileRef(tile);

                    int tileNum = mesh.DecodePolyIdTile(bse);
                    var tileColor = IntToCol(tileNum, 128);

                    var pd = tile.DetailMeshes[t];

                    List<Triangle> tris = new List<Triangle>();

                    for (int j = 0; j < pd.TriCount; ++j)
                    {
                        var dt = tile.DetailTris[(pd.TriBase + j)];
                        Vector3[] triVerts = new Vector3[3];
                        for (int k = 0; k < 3; ++k)
                        {
                            if (dt[k] < p.VertCount)
                            {
                                triVerts[k] = tile.Verts[p.Verts[dt[k]]];
                            }
                            else
                            {
                                triVerts[k] = tile.DetailVerts[(pd.VertBase + dt[k] - p.VertCount)];
                            }
                        }

                        tris.Add(new Triangle(triVerts[0], triVerts[1], triVerts[2]));
                    }

                    nodes.Add(new GraphNode()
                    {
                        Triangles = tris.ToArray(),
                        TotalCost = 1,
                        Color = tileColor,
                    });
                }
            }

            return nodes.ToArray();
        }
        /// <summary>
        /// Bitwise secret wisdoms
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Bit(int a, int b)
        {
            return (a & (1 << b)) >> b;
        }
        /// <summary>
        /// Converts an integer value to Color4
        /// </summary>
        /// <param name="value">Integer value</param>
        /// <param name="alpha">Alpha value from 0 to 255</param>
        /// <returns>Returns the Color4 value</returns>
        public static Color4 IntToCol(int value, int alpha)
        {
            int r = Bit(value, 0) + Bit(value, 3) * 2 + 1;
            int g = Bit(value, 1) + Bit(value, 4) * 2 + 1;
            int b = Bit(value, 2) + Bit(value, 5) * 2 + 1;

            return new Color4(
                1 - r * 63.0f / 255.0f,
                1 - g * 63.0f / 255.0f,
                1 - b * 63.0f / 255.0f,
                alpha / 255.0f);
        }

        /// <summary>
        /// Node triangle list
        /// </summary>
        public IEnumerable<Triangle> Triangles { get; private set; }
        /// <summary>
        /// Center point
        /// </summary>
        public Vector3 Center
        {
            get
            {
                Vector3 center = Vector3.Zero;

                foreach (var tri in Triangles)
                {
                    center += tri.Center;
                }

                return center / Math.Max(1, Triangles.Count());
            }
        }
        /// <summary>
        /// Node color
        /// </summary>
        public Color4 Color { get; set; }
        /// <summary>
        /// Total cost
        /// </summary>
        public float TotalCost { get; set; }

        /// <summary>
        /// Gets if the node contains the specified node
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <param name="distance">Resulting distance to point</param>
        /// <returns>Returns true if the current node contains the specified point</returns>
        public bool Contains(Vector3 point, out float distance)
        {
            distance = float.MaxValue;

            foreach (var tri in Triangles)
            {
                if (Intersection.PointInTriangle(point, tri.Point1, tri.Point2, tri.Point3, out float d))
                {
                    distance = d;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets node points (triangle list)
        /// </summary>
        /// <returns>Returns the node point list</returns>
        public IEnumerable<Vector3> GetPoints()
        {
            List<Vector3> vList = new List<Vector3>();

            foreach (var tri in Triangles)
            {
                vList.AddRange(tri.GetVertices());
            }

            return vList.ToArray();
        }
    }
}
