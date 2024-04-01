using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Rasterizer settings
    /// </summary>
    public struct RasterizerSettings
    {
        /// <summary>
        /// Height bits
        /// </summary>
        const int HeightBits = 13;
        /// <summary>
        /// Defines the maximum value for smin and smax.
        /// </summary>
        public const int MaxHeight = (1 << HeightBits) - 1;

        /// <summary>
        /// Cell size value
        /// </summary>
        private float cellSize;
        /// <summary>
        /// Cell height value
        /// </summary>
        private float cellHeight;

        /// <summary>
        /// Walkable slope angle
        /// </summary>
        public float WalkableSlopeAngle { get; set; }
        /// <summary>
        /// Walkable climb
        /// </summary>
        public int WalkableClimb { get; set; }
        /// <summary>
        /// Bounds
        /// </summary>
        public BoundingBox Bounds { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize { readonly get { return cellSize; } set { cellSize = value; InverseCellSize = 1f / value; } }
        /// <summary>
        /// Inverse cell size
        /// </summary>
        public float InverseCellSize { get; private set; }
        /// <summary>
        /// Cell height
        /// </summary>
        public float CellHeight { readonly get { return cellHeight; } set { cellHeight = value; InverseCellHeight = 1f / value; } }
        /// <summary>
        /// Inverse cell height
        /// </summary>
        public float InverseCellHeight { get; private set; }
    }
}
