using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A <see cref="ContourVertexi"/> is a vertex that stores 3 integer coordinates and a region ID, and is used to build <see cref="Contour"/>s.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ContourVertexi
    {
        /// <summary>
        /// Gets the leftness of a triangle formed from 3 contour vertices.
        /// </summary>
        /// <param name="a">The first vertex.</param>
        /// <param name="b">The second vertex.</param>
        /// <param name="c">The third vertex.</param>
        /// <returns>A value indicating the leftness of the triangle.</returns>
        public static bool IsLeft(ref ContourVertexi a, ref ContourVertexi b, ref ContourVertexi c)
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
        public static void Area2D(ref ContourVertexi a, ref ContourVertexi b, ref ContourVertexi c, out int area)
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
        /// Initializes a new instance of the <see cref="ContourVertexi"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <param name="region">The region ID.</param>
        public ContourVertexi(int x, int y, int z, RegionId region)
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
        public ContourVertexi(Vector3 vec, RegionId region)
        {
            this.X = (int)vec.X;
            this.Y = (int)vec.Y;
            this.Z = (int)vec.Z;
            this.RegionId = region;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ContourVertexi"/> struct as a copy.
        /// </summary>
        /// <param name="vert">The original vertex.</param>
        /// <param name="index">The index of the original vertex, which is temporarily stored in the <see cref="RegionId"/> field.</param>
        public ContourVertexi(ContourVertexi vert, int index)
        {
            this.X = vert.X;
            this.Y = vert.Y;
            this.Z = vert.Z;
            this.RegionId = new RegionId(index);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ContourVertexi"/> struct as a copy.
        /// </summary>
        /// <param name="vert">The original vertex.</param>
        /// <param name="region">The region that the vertex belongs to.</param>
        public ContourVertexi(ContourVertexi vert, RegionId region)
        {
            this.X = vert.X;
            this.Y = vert.Y;
            this.Z = vert.Z;
            this.RegionId = region;
        }
    }
}
