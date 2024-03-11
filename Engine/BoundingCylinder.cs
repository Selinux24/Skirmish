using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Axis aligned bounding cylinder
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="center">Center position</param>
    /// <param name="radius">Radius</param>
    /// <param name="height">Height</param>
    public struct BoundingCylinder(Vector3 center, float radius, float height) : IEquatable<BoundingCylinder>
    {
        /// <summary>
        /// Constructs a BoundingCylinder that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the cylinder</param>
        /// <returns>When the method completes, contains the newly constructed bounding cylinder</returns>
        public static BoundingCylinder FromPoints(IEnumerable<Vector3> points)
        {
            //Find radius and height
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;

            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < points.Count(); i++)
            {
                var p = points.ElementAt(i);

                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;

                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;

                if (p.Z < minZ) minZ = p.Z;
                if (p.Z > maxZ) maxZ = p.Z;
            }

            float h = maxY - minY;
            float r = Vector2.Distance(new Vector2(minX, minZ), new Vector2(maxX, maxZ)) * 0.5f;

            //Find center
            Vector3 c = (new Vector3(minX, minY, minZ) + new Vector3(maxX, minY, maxZ)) * 0.5f;

            return new BoundingCylinder(c, r, h);
        }
        /// <summary>
        /// Constructs a BoundingCylinder that fully contains the given points
        /// </summary>
        /// <param name="points">The points that will be contained by the cylinder</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding cylinder</param>
        public static void FromPoints(IEnumerable<Vector3> points, out BoundingCylinder result)
        {
            result = FromPoints(points);
        }

        /// <summary>
        /// Radius
        /// </summary>
        public float Radius { get; set; } = radius;
        /// <summary>
        /// Height
        /// </summary>
        public float Height { get; set; } = height;
        /// <summary>
        /// Center
        /// </summary>
        public Vector3 Center { get; set; } = center;
        /// <summary>
        /// Base position
        /// </summary>
        public readonly Vector3 BasePosition
        {
            get
            {
                return new Vector3(Center.X, Center.Y - (Height * 0.5f), Center.Z);
            }
        }
        /// <summary>
        /// Cap position
        /// </summary>
        public readonly Vector3 CapPosition
        {
            get
            {
                return new Vector3(Center.X, Center.Y + (Height * 0.5f), Center.Z);
            }
        }

        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public readonly ContainmentType Contains(ref Vector3 point)
        {
            // Find cylinder central axis
            var p1 = BasePosition;
            var p2 = CapPosition;

            // Find closest point of point in axis
            var closest = Intersection.ClosestPointInRay(p1, p2, point, out float distance);
            if (distance > Radius)
            {
                // Outside cylinder radius
                return ContainmentType.Disjoint;
            }

            // Find distance from closest point in axis to the center
            float hh = Height * 0.5f;
            float distOnAxis = Vector3.Distance(Center, closest);
            if (distOnAxis > hh)
            {
                // Outside cap and base distances
                return ContainmentType.Disjoint;
            }

            if (MathUtil.NearEqual(distance, Radius))
            {
                // The point is into the cylinder radius, and between cap and base distances.
                return ContainmentType.Intersects;
            }

            if (MathUtil.NearEqual(distOnAxis, hh))
            {
                // The point is in the cap or base.
                return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }
        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public readonly ContainmentType Contains(Vector3 point)
        {
            return Contains(ref point);
        }

        /// <summary>
        /// Gets the cylinder vertices
        /// </summary>
        /// <param name="sliceCount">Slice count</param>
        /// <returns>Returns a point array of vertices</returns>
        public readonly IEnumerable<Vector3> GetVertices(int sliceCount)
        {
            var geom = GeometryUtil.CreateCylinder(Topology.TriangleList, Center, Radius, Height, sliceCount);

            return geom.Vertices.ToArray();
        }

        /// <inheritdoc/>
        public static bool operator ==(BoundingCylinder left, BoundingCylinder right)
        {
            return
                left.Center == right.Center &&
                MathUtil.NearEqual(left.Radius, right.Radius) &&
                MathUtil.NearEqual(left.Height, right.Height);
        }
        /// <inheritdoc/>
        public static bool operator !=(BoundingCylinder left, BoundingCylinder right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public readonly bool Equals(BoundingCylinder other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is BoundingCylinder cylinder)
            {
                return this == cylinder;
            }

            return false;
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Center, Radius, Height);
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Center: {Center}; Radius: {Radius}; Height: {Height};";
        }
    }
}
