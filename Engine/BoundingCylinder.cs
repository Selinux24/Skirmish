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
    public struct BoundingCylinder : IEquatable<BoundingCylinder>
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
                var c = points.ElementAt(i);

                if (c.X < minX) minX = c.X;
                if (c.X > maxX) maxX = c.X;

                if (c.Y < minY) minY = c.Y;
                if (c.Y > maxY) maxY = c.Y;

                if (c.Z < minZ) minZ = c.Z;
                if (c.Z > maxZ) maxZ = c.Z;
            }

            float height = maxY - minY;
            float radius = Vector2.Distance(new Vector2(minX, minZ), new Vector2(maxX, maxZ)) * 0.5f;

            //Find center
            Vector3 center = (new Vector3(minX, minY, minZ) + new Vector3(maxX, minY, maxZ)) * 0.5f;

            return new BoundingCylinder(center, radius, height);
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
        public float Radius { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public float Height { get; set; }
        /// <summary>
        /// Center
        /// </summary>
        public Vector3 Center { get; set; }
        /// <summary>
        /// Base position
        /// </summary>
        public Vector3 BasePosition
        {
            get
            {
                return new Vector3(Center.X, Center.Y - (Height * 0.5f), Center.Z);
            }
        }
        /// <summary>
        /// Cap position
        /// </summary>
        public Vector3 CapPosition
        {
            get
            {
                return new Vector3(Center.X, Center.Y + (Height * 0.5f), Center.Z);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">Center position</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        public BoundingCylinder(Vector3 center, float radius, float height)
        {
            Center = center;
            Radius = radius;
            Height = height;
        }

        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref Vector3 point)
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

            if (distance == Radius)
            {
                // The point is into the cylinder radius, and between cap and base distances.
                return ContainmentType.Intersects;
            }

            if (distOnAxis == hh)
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
        public ContainmentType Contains(Vector3 point)
        {
            return Contains(ref point);
        }

        /// <summary>
        /// Gets the cylinder vertices
        /// </summary>
        /// <param name="stackCount">Stack count</param>
        /// <returns>Returns a point array of vertices</returns>
        public IEnumerable<Vector3> GetVertices(int stackCount)
        {
            var geom = GeometryUtil.CreateCylinder(Center, Radius, Height, stackCount);

            return geom.Vertices.ToArray();
        }

        /// <inheritdoc/>
        public static bool operator ==(BoundingCylinder left, BoundingCylinder right)
        {
            return
                left.Center == right.Center &&
                left.Radius == right.Radius &&
                left.Height == right.Height;
        }
        /// <inheritdoc/>
        public static bool operator !=(BoundingCylinder left, BoundingCylinder right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public bool Equals(BoundingCylinder other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is BoundingCylinder cylinder)
            {
                return this == cylinder;
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Center, Radius, Height);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Center: {Center}; Radius: {Radius}; Height: {Height};";
        }
    }
}
