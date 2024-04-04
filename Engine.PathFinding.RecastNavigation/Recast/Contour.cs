using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Represents a simple, non-overlapping contour in field space.
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="regionId">Region id</param>
    /// <param name="area">Area type</param>
    /// <param name="rawVertices">Raw vertices collection</param>
    /// <param name="vertices">Vertices collection</param>
    public class Contour(int regionId, AreaTypes area, ContourVertex[] rawVertices, ContourVertex[] vertices)
    {
        /// <summary>
        /// Portal flag mask
        /// </summary>
        public const int RC_PORTAL_FLAG = 0x0f;
        /// <summary>
        /// Border vertex flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// a tile border. If a contour vertex's region ID has this bit set, the 
        /// vertex will later be removed in order to match the segments and vertices 
        /// at tile boundaries.
        /// (Used during the build process.)
        /// </summary>
        public const int RC_BORDER_VERTEX = 0x10000;
        /// <summary>
        /// Area border flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// the border of an area.
        /// (Used during the region and contour build process.)
        /// </summary>
        public const int RC_AREA_BORDER = 0x20000;
        /// <summary>
        /// Applied to the region id field of contour vertices in order to extract the region id.
        /// The region id field of a vertex may have several flags applied to it.  So the
        /// fields value can't be used directly.
        /// </summary>
        public const int RC_CONTOUR_REG_MASK = 0xffff;

        /// <summary>
        /// Simplified contour vertex and connection data. [Size: 4 * #nverts]
        /// </summary>
        private ContourVertex[] vertices = vertices;
        /// <summary>
        /// The number of vertices in the simplified contour. 
        /// </summary>
        private int nvertices = vertices.Length;
        /// <summary>
        /// Raw contour vertex and connection data. [Size: 4 * #nrverts]
        /// </summary>
        private readonly ContourVertex[] rawVertices = rawVertices;
        /// <summary>
        /// The number of vertices in the raw contour. 
        /// </summary>
        private readonly int nrawVertices = rawVertices.Length;

        /// <summary>
        /// Gets whether the vertex has the <see cref="RC_BORDER_VERTEX"/> flag
        /// </summary>
        public static bool IsBorderVertex(int flag)
        {
            return (flag & RC_BORDER_VERTEX) != 0;
        }
        /// <summary>
        /// Gets whether the vertex has the <see cref="RC_AREA_BORDER"/> flag
        /// </summary>
        public static bool IsAreaBorder(int flag)
        {
            return (flag & RC_AREA_BORDER) != 0;
        }
        /// <summary>
        /// Gets whether the vertex has the <see cref="RC_CONTOUR_REG_MASK"/> flag
        /// </summary>
        public static bool IsRegion(int flag)
        {
            return (flag & RC_CONTOUR_REG_MASK) != 0;
        }

        /// <summary>
        /// The region id of the contour.
        /// </summary>
        public int RegionId { get; set; } = regionId;
        /// <summary>
        /// The area id of the contour.
        /// </summary>
        public AreaTypes Area { get; set; } = area;

        /// <summary>
        /// Gets whether the contour has vertices
        /// </summary>
        public bool HasVertices()
        {
            return nvertices > 0;
        }
        /// <summary>
        /// Gets the vertex count
        /// </summary>
        public int GetVertexCount()
        {
            return nvertices;
        }
        /// <summary>
        /// Gets the vertex at index
        /// </summary>
        /// <param name="index">Index</param>
        public ContourVertex GetVertex(int index)
        {
            return vertices[index];
        }

        /// <summary>
        /// Gets whether the contour has raw vertices
        /// </summary>
        public bool HasRawVertices()
        {
            return nrawVertices > 0;
        }
        /// <summary>
        /// Gets the raw vertex count
        /// </summary>
        public int GetRawVertexCount()
        {
            return nrawVertices;
        }
        /// <summary>
        /// Gets the raw vertex at index
        /// </summary>
        /// <param name="index">Index</param>
        public ContourVertex GetRawVertex(int index)
        {
            return rawVertices[index];
        }

        /// <summary>
        /// Iterates over contour vertices
        /// </summary>
        /// <returns>Returns the vertex index and the vertex data</returns>
        public IEnumerable<(int i, ContourVertex v)> IterateVertices()
        {
            for (int i = 0; i < nvertices; i++)
            {
                yield return (i, vertices[i]);
            }
        }
        /// <summary>
        /// Iterates over contour raw vertices
        /// </summary>
        /// <returns>Returns the raw vertex index and the raw vertex data</returns>
        public IEnumerable<(int i, ContourVertex v)> IterateRawVertices()
        {
            for (int i = 0; i < nrawVertices; i++)
            {
                yield return (i, rawVertices[i]);
            }
        }
        /// <summary>
        /// Iterates over contour segments
        /// </summary>
        /// <returns>Returns a list if segments</returns>
        public IEnumerable<(ContourVertex va, ContourVertex vb)> IterateSegments()
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
        public IEnumerable<(int i, Int3 a, Int3 b, Int3 c)> IterateTriangles()
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
        /// <returns>Returns the triangulated indices and vertices list</returns>
        public Int3[] Triangulate()
        {
            var verts = ContourVertex.ToInt3List(vertices);

            // Triangulate contour
            var (triRes, tris) = TriangulationHelper.Triangulate(verts);
            if (!triRes || tris.Length <= 0)
            {
                // Bad triangulation, should not happen.
                Logger.WriteWarning(nameof(Contour), $"rcBuildPolyMesh: Bad triangulation Contour {RegionId}.");
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
        public Vector3 GetContourCenter(Vector3 orig, float cs, float ch)
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
        /// Merges a contour into another, and clears it
        /// </summary>
        /// <param name="ca">Merged contour</param>
        /// <param name="cb">Cleared contour</param>
        /// <param name="ia">Merged merge index</param>
        /// <param name="ib">Cleared merge index</param>
        public static void Merge(Contour ca, Contour cb, int ia, int ib)
        {
            int maxVerts = ca.nvertices + cb.nvertices + 2;
            ContourVertex[] verts = new ContourVertex[maxVerts];

            int nv = 0;

            // Copy contour A.
            for (int i = 0; i <= ca.nvertices; ++i)
            {
                verts[nv++] = ca.vertices[((ia + i) % ca.nvertices)];
            }

            // Copy contour B
            for (int i = 0; i <= cb.nvertices; ++i)
            {
                verts[nv++] = cb.vertices[((ib + i) % cb.nvertices)];
            }

            ca.vertices = verts;
            ca.nvertices = nv;

            cb.vertices = null;
            cb.nvertices = 0;
        }

        /// <summary>
        /// Finds the left most vertex in the contour
        /// </summary>
        /// <param name="minx">Resulting minimum x value</param>
        /// <param name="minz">Resulting minimum z value</param>
        /// <param name="leftmost">Resulting left most index</param>
        public void FindLeftMostVertex(out int minx, out int minz, out int leftmost)
        {
            minx = vertices[0].X;
            minz = vertices[0].Z;
            leftmost = 0;
            for (int i = 1; i < nvertices; i++)
            {
                int x = vertices[i].X;
                int z = vertices[i].Z;
                if (x < minx || (x == minx && z < minz))
                {
                    minx = x;
                    minz = z;
                    leftmost = i;
                }
            }
        }
        /// <summary>
        /// Calculates the area of the polygon's contour in the xz plane
        /// </summary>
        public int CalcAreaOfPolygon2D()
        {
            int area = 0;

            for (int i = 0, j = nvertices - 1; i < nvertices; j = i++)
            {
                var vi = vertices[i];
                var vj = vertices[j];
                area += vi.X * vj.Z - vj.X * vi.Z;
            }

            return (area + 1) / 2;
        }
        /// <summary>
        /// Gets whether the specified segments intersects
        /// </summary>
        /// <param name="d0">First segment</param>
        /// <param name="d1">Second segment</param>
        /// <param name="i">Incident vertex index</param>
        /// <returns></returns>
        public bool IntersectSegCountour(ContourVertex d0, ContourVertex d1, int i)
        {
            // For each edge (k,k+1) of P
            for (int k = 0; k < nvertices; k++)
            {
                int k1 = ArrayUtils.Next(k, nvertices);

                // Skip edges incident to i.
                if (i == k || i == k1)
                {
                    continue;
                }

                var p0 = vertices[k];
                var p1 = vertices[k1];
                if (d0 == p0 || d1 == p0 || d0 == p1 || d1 == p1)
                {
                    continue;
                }

                if (TriangulationHelper.Intersect2D(d0.Coords, d1.Coords, p0.Coords, p1.Coords))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the specified border size from the vertices and rawvertices collections
        /// </summary>
        /// <param name="borderSize">Border size</param>
        public void RemoveBorderSize(int borderSize)
        {
            if (borderSize <= 0)
            {
                return;
            }

            // If the heightfield was build with bordersize, remove the offset.
            for (int j = 0; j < nvertices; ++j)
            {
                var v = vertices[j];
                v.X -= borderSize;
                v.Z -= borderSize;
                vertices[j] = v;
            }

            // If the heightfield was build with bordersize, remove the offset.
            for (int j = 0; j < nrawVertices; ++j)
            {
                var v = rawVertices[j];
                v.X -= borderSize;
                v.Z -= borderSize;
                rawVertices[j] = v;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Region Id: {RegionId}; Area: {Area}; Simplified Verts: {nvertices}; Raw Verts: {nrawVertices};";
        }
    };
}
