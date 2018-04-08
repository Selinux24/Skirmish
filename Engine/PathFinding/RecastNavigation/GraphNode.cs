using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    public class GraphNode : IGraphNode
    {
        public static GraphNode[] Build(NavMesh mesh)
        {
            List<GraphNode> nodes = new List<GraphNode>();

            for (int i = 0; i < mesh.MaxTiles; ++i)
            {
                var tile = mesh.Tiles[i];
                if (tile.header.magic != Detour.DT_NAVMESH_MAGIC) continue;

                for (int t = 0; t < tile.header.polyCount; t++)
                {
                    var p = tile.polys[t];
                    if (p.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION) continue;

                    var bse = mesh.GetPolyRefBase(tile);

                    int tileNum = mesh.DecodePolyIdTile(bse);
                    var tileColor = Helper.IntToCol(tileNum, 128);

                    var pd = tile.detailMeshes[t];

                    List<Triangle> tris = new List<Triangle>();

                    for (int j = 0; j < pd.triCount; ++j)
                    {
                        var dt = tile.detailTris[(pd.triBase + j)];
                        Vector3[] triVerts = new Vector3[3];
                        for (int k = 0; k < 3; ++k)
                        {
                            if (dt[k] < p.vertCount)
                            {
                                triVerts[k] = tile.verts[p.verts[dt[k]]];
                            }
                            else
                            {
                                triVerts[k] = tile.detailVerts[(pd.vertBase + dt[k] - p.vertCount)];
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

        public Triangle[] Triangles;

        public Vector3 Center
        {
            get
            {
                Vector3 center = Vector3.Zero;

                foreach (var tri in Triangles)
                {
                    center += tri.Center;
                }

                return center / Math.Max(1, Triangles.Length);
            }
        }

        public Color4 Color { get; set; }

        public float TotalCost { get; set; }

        public bool Contains(Vector3 point, out float distance)
        {
            distance = float.MaxValue;
            foreach (var tri in Triangles)
            {
                if (Intersection.PointInPoly(point, tri.GetVertices()))
                {
                    float d = Intersection.PointToTriangle(point, tri.Point1, tri.Point2, tri.Point3);
                    if (d == 0)
                    {
                        distance = 0;
                        return true;
                    }

                    distance = Math.Min(distance, d);
                }
            }

            return false;
        }

        public Vector3[] GetPoints()
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
