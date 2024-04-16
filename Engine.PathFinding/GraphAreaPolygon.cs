using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area
    /// </summary>
    public class GraphAreaPolygon : GraphArea, IGraphAreaPolygon
    {
        /// <summary>
        /// Maximum polygon vertices points
        /// </summary>
        public const int MaxPoints = 12;

        /// <inheritdoc/>
        public Vector3[] Vertices { get; set; }
        /// <inheritdoc/>
        public float MinHeight { get; set; }
        /// <inheritdoc/>
        public float MaxHeight { get; set; }

        /// <summary>
        /// Gets the polygon bounds
        /// </summary>
        /// <param name="vertices">Polygon vertices</param>
        /// <param name="hmin">Minimum height</param>
        /// <param name="hmax">Maximum height</param>
        public static BoundingBox GetPolygonBounds(Vector3[] vertices, float hmin, float hmax)
        {
            var bmin = vertices[0];
            var bmax = vertices[0];

            for (int i = 1; i < vertices.Length; ++i)
            {
                bmin = Vector3.Min(bmin, vertices[i]);
                bmax = Vector3.Max(bmax, vertices[i]);
            }

            bmin.Y = hmin;
            bmax.Y = hmax;

            return new(bmin, bmax);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaPolygon() : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaPolygon(IEnumerable<Vector3> vertices, float minHeight, float maxHeight) : base()
        {
            if ((vertices?.Count() ?? 0) < 3)
            {
                throw new ArgumentException("A polygon area must have at least three vertices.", nameof(vertices));
            }

            Vertices = vertices.ToArray();
            MinHeight = minHeight;
            MaxHeight = maxHeight;
        }

        /// <inheritdoc/>
        public override BoundingBox GetBounds()
        {
            return GetPolygonBounds(Vertices, MinHeight, MaxHeight);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => AreaType {GetAreaType()}; MinHeight {MinHeight} MaxHeight {MaxHeight} -> {(Vertices != null ? string.Join(" ", Vertices) : "")}";
        }
    }
}
