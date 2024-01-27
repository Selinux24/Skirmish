using SharpDX;

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
        /// Gets the cylinder bounds
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="r">Radius</param>
        /// <param name="h">Height</param>
        public static BoundingBox GetCylinderBounds(Vector3 pos, float r, float h)
        {
            float minX = pos.X - r;
            float minY = pos.Y;
            float minZ = pos.Z - r;
            float maxX = pos.X + r;
            float maxY = pos.Y + h;
            float maxZ = pos.Z + r;

            return new(new(minX, minY, minZ), new(maxX, maxY, maxZ));
        }

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
        public override BoundingBox GetBounds()
        {
            return GetCylinderBounds(Center, Radius, Height);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => AreaType {AreaType}; Center {Center} Radius {Radius} Height {Height}";
        }
    }
}
