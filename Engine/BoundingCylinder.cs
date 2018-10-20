using System;
using SharpDX;

namespace Engine
{
    /// <summary>
    /// Bounding cylinder
    /// </summary>
    public struct BoundingCylinder : IEquatable<BoundingCylinder>
    {
        /// <summary>
        /// Constructs a BoundingCylinder that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the cylinder</param>
        /// <returns>When the method completes, contains the newly constructed bounding cylinder</returns>
        public static BoundingCylinder FromPoints(Vector3[] points)
        {
            //Find radius and height
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;

            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                var c = points[i];

                if (c.X < minX) minX = c.X;
                if (c.X > maxX) maxX = c.X;

                if (c.Y < minY) minY = c.Y;
                if (c.Y > maxY) maxY = c.Y;

                if (c.Z < minZ) minZ = c.Z;
                if (c.Z > maxZ) maxZ = c.Z;
            }

            float height = maxY - minY;
            float radius = Vector2.Distance(new Vector2(minX, minZ), new Vector2(maxX, maxZ)) * 0.5f;

            //Find position
            Vector3 position = (new Vector3(minX, minY, minZ) + new Vector3(maxX, minY, maxZ)) * 0.5f;

            return new BoundingCylinder(position, radius, height);
        }
        /// <summary>
        /// Constructs a BoundingCylinder that fully contains the given points
        /// </summary>
        /// <param name="points">The points that will be contained by the cylinder</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding cylinder</param>
        public static void FromPoints(Vector3[] points, out BoundingCylinder result)
        {
            result = FromPoints(points);
        }
        /// <summary>
        /// Constructs a BoundingCylinder that is as large as the total combined area of the two specified cylinders.
        /// </summary>
        /// <param name="value1">The first cylinder to merge</param>
        /// <param name="value2">The second cylinder to merge</param>
        /// <returns>The newly constructed bounding cylinder</returns>
        public static BoundingCylinder Merge(BoundingCylinder value1, BoundingCylinder value2)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Constructs a BoundingCylinder that is as large as the total combined area of the two specified cylinders.
        /// </summary>
        /// <param name="value1">The first cylinder to merge</param>
        /// <param name="value2">The second cylinder to merge</param>
        /// <param name="result">The newly constructed bounding cylinder</param>
        public static void Merge(ref BoundingCylinder value1, ref BoundingCylinder value2, out BoundingCylinder result)
        {
            result = Merge(value1, value2);
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position { get; set; }
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
        public Vector3 Center
        {
            get
            {
                return this.Position + new Vector3(0f, this.Height * 0.5f, 0f);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        public BoundingCylinder(Vector3 position, float radius, float height)
        {
            this.Position = position;
            this.Radius = radius;
            this.Height = height;
        }

        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref Vector3 point)
        {
            throw new NotImplementedException();
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
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if left has a different value than right; otherwise, false.</returns>
        public static bool operator !=(BoundingCylinder left, BoundingCylinder right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns></returns>
        public static bool operator ==(BoundingCylinder left, BoundingCylinder right)
        {
            return
                left.Position == right.Position &&
                left.Radius == right.Radius &&
                left.Height == right.Height;
        }
        /// <summary>
        /// Determines whether the specified BoundingCylinder is equal to this instance.
        /// </summary>
        /// <param name="other">The BoundingCylinder to compare with this instance.</param>
        /// <returns>true if the specified BoundingCylinder is equal to this instance; otherwise, false</returns>
        public bool Equals(BoundingCylinder other)
        {
            return this == other;
        }
        /// <summary>
        /// Determines whether the specified System.Object is equal to this instance.
        /// </summary>
        /// <param name="obj">The System.Object to compare with this instance.</param>
        /// <returns>true if the specified System.Object is equal to this instance; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            if (obj is BoundingCylinder)
            {
                return this == (BoundingCylinder)obj;
            }

            return false;
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return this.Position.GetHashCode() ^ this.Radius.GetHashCode() ^ this.Height.GetHashCode();
        }
    }
}
