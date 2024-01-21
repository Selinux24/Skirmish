﻿using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area cylinder
    /// </summary>
    public class GraphAreaCylinder : GraphArea, IGraphAreaCylinder
    {
        /// <inheritdoc/>
        public Vector3 Center { get; set; }
        /// <inheritdoc/>
        public float Radius { get; set; }
        /// <inheritdoc/>
        public float Height { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaCylinder() : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaCylinder(Vector3 center, float radius, float height) : base()
        {
            Center = center;
            Radius = radius;
            Height = height;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAreaCylinder(BoundingCylinder cylinder) : base()
        {
            Center = cylinder.Center;
            Radius = cylinder.Radius;
            Height = cylinder.Height;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => AreaType {AreaType}; Center {Center} Radius {Radius} Height {Height}";
        }
    }
}
