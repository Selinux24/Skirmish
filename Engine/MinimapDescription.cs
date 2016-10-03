using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Minimap description
    /// </summary>
    public class MinimapDescription
    {
        /// <summary>
        /// Top position
        /// </summary>
        public int Top;
        /// <summary>
        /// Left position
        /// </summary>
        public int Left;
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Terrain to draw
        /// </summary>
        public Drawable[] Drawables;
        /// <summary>
        /// Minimap render area
        /// </summary>
        public BoundingBox MinimapArea;
    }
}
