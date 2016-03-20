using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Geometry
{
    using Engine.Common;

    /// <summary>
    /// A set of contours around the regions of a <see cref="CompactHeightfield"/>, used as the edges of a
    /// <see cref="PolyMesh"/>.
    /// </summary>
    public class ContourSet : ICollection<Contour>
    {
        private List<Contour> contours;

        /// <summary>
        /// Gets the world-space bounding box of the set.
        /// </summary>
        public BoundingBox Bounds { get; private set; }
        /// <summary>
        /// Gets the width of the set, not including the border size specified in <see cref="CompactHeightfield"/>.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets the height of the set, not including the border size specified in <see cref="CompactHeightfield"/>.
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the <see cref="ContourSet"/> is read-only.
        /// </summary>
        bool ICollection<Contour>.IsReadOnly
        {
            get { return true; }
        }
        /// <summary>
        /// Gets the number of <see cref="Contour"/>s in the set.
        /// </summary>
        public int Count
        {
            get
            {
                return contours.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContourSet"/> class.
        /// </summary>
        /// <param name="contours">A collection of <see cref="Contour"/>s.</param>
        /// <param name="bounds">The bounding box that contains all of the <see cref="Contour"/>s.</param>
        /// <param name="width">The width, in voxel units, of the world.</param>
        /// <param name="height">The height, in voxel units, of the world.</param>
        public ContourSet(IEnumerable<Contour> contours, BoundingBox bounds, int width, int height)
        {
            this.contours = contours.ToList();
            this.Bounds = bounds;
            this.Width = width;
            this.Height = height;

            //prevent null contours from ever being added to the set.
            this.contours.RemoveAll(c => c.IsNull);
        }

        /// <summary>
        /// Calculates the maximum number of vertices, triangles, and vertices per contour in the
        /// set of contours.
        /// </summary>
        /// <param name="maxVertices">The maximum number of vertices possible from this contour set.</param>
        /// <param name="maxTris">The maximum number of triangles possible from this contour set.</param>
        /// <param name="maxVertsPerContour">The maximum number of vertices per contour within the set.</param>
        public void GetVertexLimits(out int maxVertices, out int maxTris, out int maxVertsPerContour)
        {
            //TODO refactor name of function?
            maxVertices = 0;
            maxTris = 0;
            maxVertsPerContour = 0;

            foreach (var c in contours)
            {
                int vertCount = c.Vertices.Length;

                maxVertices += vertCount;
                maxTris += vertCount - 2;
                maxVertsPerContour = Math.Max(maxVertsPerContour, vertCount);
            }
        }
        /// <summary>
        /// Add a new contour to the set
        /// </summary>
        /// <param name="item">The contour to add</param>
        public void Add(Contour item)
        {
            if (item.IsNull)
                throw new ArgumentException("Contour is null (less than 3 vertices)");

            contours.Add(item);
        }
        /// <summary>
        /// Clear the set of contours.
        /// </summary>
        public void Clear()
        {
            contours.Clear();
        }
        /// <summary>
        /// Checks if a specified <see cref="ContourSet"/> is contained in the <see cref="ContourSet"/>.
        /// </summary>
        /// <param name="item">A contour.</param>
        /// <returns>A value indicating whether the set contains the specified contour.</returns>
        public bool Contains(Contour item)
        {
            return contours.Contains(item);
        }
        /// <summary>
        /// Copies the <see cref="Contour"/>s in the set to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Contour[] array, int arrayIndex)
        {
            contours.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Returns an enumerator that iterates through the entire <see cref="ContourSet"/>.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<Contour> GetEnumerator()
        {
            return contours.GetEnumerator();
        }
        /// <summary>
        /// (Not implemented) Remove a contour from the set
        /// </summary>
        /// <param name="item">The contour to remove</param>
        /// <returns>throw InvalidOperatorException</returns>
        bool ICollection<Contour>.Remove(Contour item)
        {
            throw new InvalidOperationException();
        }
        /// <summary>
        /// Gets an enumerator that iterates through the set
        /// </summary>
        /// <returns>The enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    /// <summary>
    /// A contour is formed from a region.
    /// </summary>
    public class Contour
    {
        /// <summary>
        /// Finds the closest indices between two contours. Useful for merging contours.
        /// </summary>
        /// <param name="a">A contour.</param>
        /// <param name="b">Another contour.</param>
        /// <param name="indexA">The nearest index on contour A.</param>
        /// <param name="indexB">The nearest index on contour B.</param>
        private static void GetClosestIndices(Contour a, Contour b, out int indexA, out int indexB)
        {
            int closestDistance = int.MaxValue;
            int lengthA = a.Vertices.Length;
            int lengthB = b.Vertices.Length;

            indexA = -1;
            indexB = -1;

            for (int i = 0; i < lengthA; i++)
            {
                int vertA = i;
                int vertANext = (i + 1) % lengthA;
                int vertAPrev = (i + lengthA - 1) % lengthA;

                for (int j = 0; j < lengthB; j++)
                {
                    int vertB = j;

                    //vertB must be infront of vertA
                    if (ContourVertex.IsLeft(ref a.Vertices[vertAPrev], ref a.Vertices[vertA], ref b.Vertices[vertB]) &&
                        ContourVertex.IsLeft(ref a.Vertices[vertA], ref a.Vertices[vertANext], ref b.Vertices[vertB]))
                    {
                        int dx = b.Vertices[vertB].X - a.Vertices[vertA].X;
                        int dz = b.Vertices[vertB].Z - a.Vertices[vertA].Z;
                        int tempDist = dx * dx + dz * dz;
                        if (tempDist < closestDistance)
                        {
                            indexA = i;
                            indexB = j;
                            closestDistance = tempDist;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Simplify the contours by reducing the number of edges
        /// </summary>
        /// <param name="rawVerts">Initial vertices</param>
        /// <param name="simplified">New and simplified vertices</param>
        /// <param name="maxError">Maximum error allowed</param>
        /// <param name="maxEdgeLen">The maximum edge length allowed</param>
        /// <param name="buildFlags">Flags determines how to split the long edges</param>
        public static void Simplify(List<ContourVertex> rawVerts, List<ContourVertex> simplified, float maxError, int maxEdgeLen, ContourBuildFlags buildFlags)
        {
            bool tesselateWallEdges = (buildFlags & ContourBuildFlags.TessellateWallEdges) == ContourBuildFlags.TessellateWallEdges;
            bool tesselateAreaEdges = (buildFlags & ContourBuildFlags.TessellateAreaEdges) == ContourBuildFlags.TessellateAreaEdges;

            //add initial points
            bool hasConnections = false;
            for (int i = 0; i < rawVerts.Count; i++)
            {
                if (rawVerts[i].RegionId.Id != 0)
                {
                    hasConnections = true;
                    break;
                }
            }

            if (hasConnections)
            {
                //contour has some portals to other regions
                //add new point to every location where region changes
                for (int i = 0, end = rawVerts.Count; i < end; i++)
                {
                    int ii = (i + 1) % end;
                    bool differentRegions = rawVerts[i].RegionId.Id != rawVerts[ii].RegionId.Id;
                    bool areaBorders = RegionId.HasFlags(rawVerts[i].RegionId, RegionFlags.AreaBorder) != RegionId.HasFlags(rawVerts[ii].RegionId, RegionFlags.AreaBorder);

                    if (differentRegions || areaBorders)
                    {
                        simplified.Add(new ContourVertex(rawVerts[i], i));
                    }
                }
            }

            //add some points if thhere are no connections
            if (simplified.Count == 0)
            {
                //find lower-left and upper-right vertices of contour
                int lowerLeftX = rawVerts[0].X;
                int lowerLeftY = rawVerts[0].Y;
                int lowerLeftZ = rawVerts[0].Z;
                RegionId lowerLeftI = RegionId.Null;

                int upperRightX = rawVerts[0].X;
                int upperRightY = rawVerts[0].Y;
                int upperRightZ = rawVerts[0].Z;
                RegionId upperRightI = RegionId.Null;

                //iterate through points
                for (int i = 0; i < rawVerts.Count; i++)
                {
                    int x = rawVerts[i].X;
                    int y = rawVerts[i].Y;
                    int z = rawVerts[i].Z;

                    if (x < lowerLeftX || (x == lowerLeftX && z < lowerLeftZ))
                    {
                        lowerLeftX = x;
                        lowerLeftY = y;
                        lowerLeftZ = z;
                        lowerLeftI = new RegionId(i);
                    }

                    if (x > upperRightX || (x == upperRightX && z > upperRightZ))
                    {
                        upperRightX = x;
                        upperRightY = y;
                        upperRightZ = z;
                        upperRightI = new RegionId(i);
                    }
                }

                //save the points
                simplified.Add(new ContourVertex(lowerLeftX, lowerLeftY, lowerLeftZ, lowerLeftI));
                simplified.Add(new ContourVertex(upperRightX, upperRightY, upperRightZ, upperRightI));
            }

            //add points until all points are within error tolerance of simplified slope
            int numPoints = rawVerts.Count;
            for (int i = 0; i < simplified.Count; )
            {
                int ii = (i + 1) % simplified.Count;

                //obtain (x, z) coordinates, along with region id
                int ax = simplified[i].X;
                int az = simplified[i].Z;
                int ai = (int)simplified[i].RegionId;

                int bx = simplified[ii].X;
                int bz = simplified[ii].Z;
                int bi = (int)simplified[ii].RegionId;

                float maxDeviation = 0;
                int maxi = -1;
                int ci, countIncrement, endi;

                //traverse segment in lexilogical order (try to go from smallest to largest coordinates?)
                if (bx > ax || (bx == ax && bz > az))
                {
                    countIncrement = 1;
                    ci = (int)(ai + countIncrement) % numPoints;
                    endi = (int)bi;
                }
                else
                {
                    countIncrement = numPoints - 1;
                    ci = (int)(bi + countIncrement) % numPoints;
                    endi = (int)ai;
                }

                //tessellate only outer edges or edges between areas
                if (rawVerts[ci].RegionId.Id == 0 || RegionId.HasFlags(rawVerts[ci].RegionId, RegionFlags.AreaBorder))
                {
                    //find the maximum deviation
                    while (ci != endi)
                    {
                        float deviation = GeometryUtil.PointToSegment2DSquared(rawVerts[ci].X, rawVerts[ci].Z, ax, az, bx, bz);

                        if (deviation > maxDeviation)
                        {
                            maxDeviation = deviation;
                            maxi = ci;
                        }

                        ci = (ci + countIncrement) % numPoints;
                    }
                }

                //If max deviation is larger than accepted error, add new point
                if (maxi != -1 && maxDeviation > (maxError * maxError))
                {
                    simplified.Insert(i + 1, new ContourVertex(rawVerts[maxi], maxi));
                }
                else
                {
                    i++;
                }
            }

            //split too long edges
            if (maxEdgeLen > 0 && (tesselateAreaEdges || tesselateWallEdges))
            {
                for (int i = 0; i < simplified.Count; )
                {
                    int ii = (i + 1) % simplified.Count;

                    //get (x, z) coordinates along with region id
                    int ax = simplified[i].X;
                    int az = simplified[i].Z;
                    int ai = (int)simplified[i].RegionId;

                    int bx = simplified[ii].X;
                    int bz = simplified[ii].Z;
                    int bi = (int)simplified[ii].RegionId;

                    //find maximum deviation from segment
                    int maxi = -1;
                    int ci = (int)(ai + 1) % numPoints;

                    //tessellate only outer edges or edges between areas
                    bool tess = false;

                    //wall edges
                    if (tesselateWallEdges && rawVerts[ci].RegionId.Id == 0)
                        tess = true;

                    //edges between areas
                    if (tesselateAreaEdges && RegionId.HasFlags(rawVerts[ci].RegionId, RegionFlags.AreaBorder))
                        tess = true;

                    if (tess)
                    {
                        int dx = bx - ax;
                        int dz = bz - az;
                        if (dx * dx + dz * dz > maxEdgeLen * maxEdgeLen)
                        {
                            //round based on lexilogical direction (smallest to largest cooridinates, first by x.
                            //if x coordinates are equal, then compare z coordinates)
                            int n = bi < ai ? (bi + numPoints - ai) : (bi - ai);

                            if (n > 1)
                            {
                                if (bx > ax || (bx == ax && bz > az))
                                    maxi = (int)(ai + n / 2) % numPoints;
                                else
                                    maxi = (int)(ai + (n + 1) / 2) % numPoints;
                            }
                        }
                    }

                    //add new point
                    if (maxi != -1)
                    {
                        simplified.Insert(i + 1, new ContourVertex(rawVerts[maxi], maxi));
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            for (int i = 0; i < simplified.Count; i++)
            {
                ContourVertex sv = simplified[i];

                //take edge vertex flag from current raw point and neighbor region from next raw point
                int ai = ((int)sv.RegionId + 1) % numPoints;
                RegionId bi = sv.RegionId;

                //save new region id
                sv.RegionId = RegionId.FromRawBits(((int)rawVerts[ai].RegionId & (RegionId.MaskId | (int)RegionFlags.AreaBorder)) | ((int)rawVerts[(int)bi].RegionId & (int)RegionFlags.VertexBorder));

                simplified[i] = sv;
            }
        }
        /// <summary>
        /// Removes degenerate segments from a simplified contour.
        /// </summary>
        /// <param name="simplified">The simplified contour.</param>
        public static void RemoveDegenerateSegments(List<ContourVertex> simplified)
        {
            //remove adjacent vertices which are equal on the xz-plane
            for (int i = 0; i < simplified.Count; i++)
            {
                int ni = i + 1;
                if (ni >= simplified.Count)
                    ni = 0;

                if (simplified[i].X == simplified[ni].X &&
                    simplified[i].Z == simplified[ni].Z)
                {
                    //remove degenerate segment
                    simplified.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Gets the simplified vertices of the contour.
        /// </summary>
        public ContourVertex[] Vertices { get; private set; }
        /// <summary>
        /// Gets the area ID of the contour.
        /// </summary>
        public Area Area { get; private set; }
        /// <summary>
        /// Gets the region ID of the contour.
        /// </summary>
        public RegionId RegionId { get; private set; }
        /// <summary>
        /// Gets the 2D area of the contour. A positive area means the contour is going forwards, a negative
        /// area maens it is going backwards.
        /// </summary>
        public int Area2D
        {
            get
            {
                int area = 0;
                for (int i = 0, j = Vertices.Length - 1; i < Vertices.Length; j = i++)
                {
                    ContourVertex vi = Vertices[i], vj = Vertices[j];
                    area += vi.X * vj.Z - vj.X * vi.Z;
                }

                return (area + 1) / 2;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the contour is "null" (has less than 3 vertices).
        /// </summary>
        public bool IsNull
        {
            get
            {
                if (Vertices == null || Vertices.Length < 3)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contour"/> class.
        /// </summary>
        /// <param name="verts">The raw vertices of the contour.</param>
        /// <param name="region">The region ID of the contour.</param>
        /// <param name="area">The area ID of the contour.</param>
        /// <param name="borderSize">The size of the border.</param>
        public Contour(List<ContourVertex> verts, RegionId region, Area area, int borderSize)
        {
            this.Vertices = verts.ToArray();
            this.RegionId = region;
            this.Area = area;

            //remove offset
            if (borderSize > 0)
            {
                for (int j = 0; j < Vertices.Length; j++)
                {
                    Vertices[j].X -= borderSize;
                    Vertices[j].Z -= borderSize;
                }
            }
        }

        /// <summary>
        /// Merges another contour into this instance.
        /// </summary>
        /// <param name="contour">The contour to merge.</param>
        public void MergeWith(Contour contour)
        {
            int lengthA = Vertices.Length;
            int lengthB = contour.Vertices.Length;

            int ia, ib;
            GetClosestIndices(this, contour, out ia, out ib);

            //create a list with the capacity set to the max number of possible verts to avoid expanding the list.
            var newVerts = new List<ContourVertex>(Vertices.Length + contour.Vertices.Length + 2);

            //copy contour A
            for (int i = 0; i <= lengthA; i++)
                newVerts.Add(Vertices[(ia + i) % lengthA]);

            //add contour B (other contour) to contour A (this contour)
            for (int i = 0; i <= lengthB; i++)
                newVerts.Add(contour.Vertices[(ib + i) % lengthB]);

            Vertices = newVerts.ToArray();

            //delete the other contour
            contour.Vertices = null;
        }
    }
    /// <summary>
    /// A set of flags that control the way contours are built.
    /// </summary>
    [Flags]
    public enum ContourBuildFlags
    {
        /// <summary>Build normally.</summary>
        None = 0,
        /// <summary>Tessellate solid edges during contour simplification.</summary>
        TessellateWallEdges = 0x01,
        /// <summary>Tessellate edges between areas during contour simplification.</summary>
        TessellateAreaEdges = 0x02
    }
    /// <summary>
    /// A <see cref="ContourVertex"/> is a vertex that stores 3 integer coordinates and a region ID, and is used to build <see cref="Contour"/>s.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ContourVertex
    {
        /// <summary>
        /// Gets the leftness of a triangle formed from 3 contour vertices.
        /// </summary>
        /// <param name="a">The first vertex.</param>
        /// <param name="b">The second vertex.</param>
        /// <param name="c">The third vertex.</param>
        /// <returns>A value indicating the leftness of the triangle.</returns>
        public static bool IsLeft(ref ContourVertex a, ref ContourVertex b, ref ContourVertex c)
        {
            int area;
            Area2D(ref a, ref b, ref c, out area);
            return area < 0;
        }
        /// <summary>
        /// Gets the 2D area of the triangle ABC.
        /// </summary>
        /// <param name="a">Point A of triangle ABC.</param>
        /// <param name="b">Point B of triangle ABC.</param>
        /// <param name="c">Point C of triangle ABC.</param>
        /// <param name="area">The 2D area of the triangle.</param>
        public static void Area2D(ref ContourVertex a, ref ContourVertex b, ref ContourVertex c, out int area)
        {
            area = (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z);
        }

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public int X;
        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public int Y;
        /// <summary>
        /// The Z coordinate.
        /// </summary>
        public int Z;
        /// <summary>
        /// The region that the vertex belongs to.
        /// </summary>
        public RegionId RegionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContourVertex"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <param name="region">The region ID.</param>
        public ContourVertex(int x, int y, int z, RegionId region)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.RegionId = region;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SharpNav.ContourVertex"/> struct.
        /// </summary>
        /// <param name="vec">The array of X,Y,Z coordinates.</param>
        /// <param name="region">The Region ID.</param>
        public ContourVertex(Vector3 vec, RegionId region)
        {
            this.X = (int)vec.X;
            this.Y = (int)vec.Y;
            this.Z = (int)vec.Z;
            this.RegionId = region;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ContourVertex"/> struct as a copy.
        /// </summary>
        /// <param name="vert">The original vertex.</param>
        /// <param name="index">The index of the original vertex, which is temporarily stored in the <see cref="RegionId"/> field.</param>
        public ContourVertex(ContourVertex vert, int index)
        {
            this.X = vert.X;
            this.Y = vert.Y;
            this.Z = vert.Z;
            this.RegionId = new RegionId(index);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ContourVertex"/> struct as a copy.
        /// </summary>
        /// <param name="vert">The original vertex.</param>
        /// <param name="region">The region that the vertex belongs to.</param>
        public ContourVertex(ContourVertex vert, RegionId region)
        {
            this.X = vert.X;
            this.Y = vert.Y;
            this.Z = vert.Z;
            this.RegionId = region;
        }
    }
}
