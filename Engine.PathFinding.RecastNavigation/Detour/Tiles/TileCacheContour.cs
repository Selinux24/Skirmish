using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache contour
    /// </summary>
    public struct TileCacheContour
    {
        /// <summary>
        /// Stored direction mask
        /// </summary>
        public const int DT_DIR_MASK = 0xf8;

        /// <summary>
        /// Vertex list
        /// </summary>
        private ContourVertex[] vertices;
        /// <summary>
        /// Number of vertices
        /// </summary>
        private int nvertices;

        /// <summary>
        /// Region id
        /// </summary>
        public int RegionId { get; set; }
        /// <summary>
        /// Area type
        /// </summary>
        public AreaTypes Area { get; set; }

        /// <summary>
        /// Gets whether the contour has vertices
        /// </summary>
        public readonly bool HasVertices()
        {
            return nvertices > 0;
        }
        /// <summary>
        /// Gets the vertex count
        /// </summary>
        public readonly int GetVertexCount()
        {
            return nvertices;
        }
        /// <summary>
        /// Gets the vertex at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly ContourVertex GetVertex(int index)
        {
            return vertices[index];
        }

        /// <summary>
        /// Iterates over contour vertices
        /// </summary>
        /// <returns>Returns the vertex index and the vertex data</returns>
        public readonly IEnumerable<(int i, ContourVertex v)> IterateVertices()
        {
            for (int i = 0; i < nvertices; i++)
            {
                yield return (i, vertices[i]);
            }
        }
        /// <summary>
        /// Iterates over contour segments
        /// </summary>
        /// <returns>Returns a list if segments</returns>
        public readonly IEnumerable<(ContourVertex va, ContourVertex vb)> IterateSegments()
        {
            if (nvertices < 2)
            {
                yield break;
            }

            for (int j = 0, k = nvertices - 1; j < nvertices; k = j++)
            {
                var va = vertices[k];
                var vb = vertices[j];

                yield return (va, vb);
            }
        }
        /// <summary>
        /// Iterates over contour triangles
        /// </summary>
        public readonly IEnumerable<(int i, Int3 a, Int3 b, Int3 c)> IterateTriangles()
        {
            if (nvertices < 3)
            {
                yield break;
            }

            var verts = ContourVertex.ToInt3List(vertices);

            for (int i = 0; i < nvertices; i++)
            {
                var a = verts[i];
                var b = verts[ArrayUtils.Next(i, nvertices)];
                var c = verts[ArrayUtils.Prev(i, nvertices)];

                yield return (i, a, b, c);
            }
        }

        /// <summary>
        /// Triangulates the polygon's contour
        /// </summary>
        /// <returns>Returns the polygon indices, polygon triangles and number of triangles</returns>
        public readonly Int3[] Triangulate()
        {
            var verts = ContourVertex.ToInt3List(vertices);

            // Triangulate contour
            var (triRes, tris) = TriangulationHelper.Triangulate(verts);
            if (!triRes || tris.Length <= 0)
            {
                Logger.WriteWarning(nameof(TileCacheContourSet), $"Polygon contour triangulation error: Reg {RegionId}");
            }

            return tris;
        }

        /// <summary>
        /// Gets the contour center
        /// </summary>
        /// <param name="cont">Contour</param>
        /// <param name="orig">Origin</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        public readonly Vector3 GetContourCenter(Vector3 orig, float cs, float ch)
        {
            if (nvertices <= 0)
            {
                return Vector3.Zero;
            }

            Int3 center = Int3.Zero;
            for (int i = 0; i < nvertices; i++)
            {
                var v = vertices[i];
                center += v.Position;
            }

            Vector3 res = new(center.X, center.Y, center.Z);
            float s = 1.0f / nvertices;
            res *= s * cs;

            res.X += orig.X;
            res.Y += orig.Y * ch;
            res.Z += orig.Z;

            return res;
        }

        /// <summary>
        /// Stores the specified vertices in the contour
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="nverts">Number of vertices in the list</param>
        /// <param name="tcl">Tile cache layer</param>
        /// <param name="walkableClimb">Walkable climb value</param>
        public void StoreVerts(VertexWithNeigbour[] verts, int nverts, TileCacheLayer tcl, int walkableClimb)
        {
            nvertices = nverts;
            if (nvertices <= 0)
            {
                return;
            }

            vertices = new ContourVertex[nverts];

            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var v = verts[j];
                var vn = verts[i];
                int nei = vn.Nei; // The neighbour reg is stored at segment vertex of a segment. 
                int lh = tcl.GetCornerHeight(v, walkableClimb, out bool shouldRemove);

                // Store portal direction and remove status to the fourth component.
                int flag = Edge.DT_PORTAL_FLAG;
                if (nei != Edge.DT_PORTAL_FLAG && nei >= DT_DIR_MASK)
                {
                    flag = nei - DT_DIR_MASK;
                }
                if (shouldRemove)
                {
                    flag |= Edge.DT_BORDER_VERTEX;
                }

                vertices[j] = new()
                {
                    X = v.X,
                    Y = lh,
                    Z = v.Z,
                    Flag = flag,
                };
            }
        }
    }
}
