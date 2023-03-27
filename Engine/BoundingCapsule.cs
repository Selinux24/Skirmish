﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Axis aligned bounding capsule
    /// </summary>
    public struct BoundingCapsule : IEquatable<BoundingCapsule>
    {
        /// <summary>
        /// Constructs a BoundingCapsule that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the capsule</param>
        /// <returns>When the method completes, contains the newly constructed bounding capsule</returns>
        public static BoundingCapsule FromPoints(IEnumerable<Vector3> points)
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

            return new BoundingCapsule(center, radius, height);
        }
        /// <summary>
        /// Constructs a BoundingCapsule that fully contains the given points
        /// </summary>
        /// <param name="points">The points that will be contained by the capsule</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding capsule</param>
        public static void FromPoints(IEnumerable<Vector3> points, out BoundingCapsule result)
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
        public BoundingCapsule(Vector3 center, float radius, float height)
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
            // Find capsule points
            var p1 = BasePosition;
            var p2 = CapPosition;

            // Find closest point of point in axis
            Intersection.ClosestPointInSegment(p1, p2, point, out float distance);
            if (distance > Radius)
            {
                // Outside capsule radius
                return ContainmentType.Disjoint;
            }

            if (MathUtil.NearEqual(distance, Radius))
            {
                // In the capsule
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
        /// Gets the capsule segment vertices
        /// </summary>
        /// <returns>Returns a two point array of vertices, representing the capsule axis segment</returns>
        public IEnumerable<Vector3> GetVertices(int sliceCount, int stackCount)
        {
            var geom = GeometryUtil.CreateCapsule(Topology.TriangleList, Center, Radius, Height, sliceCount, stackCount);

            return geom.Vertices.ToArray();
        }

        /// <inheritdoc/>
        public static bool operator ==(BoundingCapsule left, BoundingCapsule right)
        {
            return
                left.Center == right.Center &&
                left.Radius == right.Radius &&
                left.Height == right.Height;
        }
        /// <inheritdoc/>
        public static bool operator !=(BoundingCapsule left, BoundingCapsule right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public bool Equals(BoundingCapsule other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is BoundingCapsule capsule)
            {
                return this == capsule;
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
            return $"Base: {BasePosition}; Cap: {CapPosition}; Radius: {Radius};";
        }
    }
}
