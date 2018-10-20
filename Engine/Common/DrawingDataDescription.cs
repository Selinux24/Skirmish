using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Drawing data description
    /// </summary>
    public class DrawingDataDescription
    {
        /// <summary>
        /// Gets or sets whether the data is instanced
        /// </summary>
        public bool Instanced { get; set; } = false;
        /// <summary>
        /// Gets or sets the number of instances
        /// </summary>
        public int Instances { get; set; } = 0;
        /// <summary>
        /// Gets or sets whether the animation data must be loaded or not
        /// </summary>
        public bool LoadAnimation { get; set; } = false;
        /// <summary>
        /// Gets or sets the texture count
        /// </summary>
        public int TextureCount { get; set; } = 0;
        /// <summary>
        /// Gets or sets whether the model uses normal maps or not
        /// </summary>
        public bool LoadNormalMaps { get; set; } = false;
        /// <summary>
        /// Gets or sets whether the model uses dynamic buffers or not
        /// </summary>
        public bool DynamicBuffers { get; set; } = false;
        /// <summary>
        /// Gets or sets the bounding volume constraint for vertex generation
        /// </summary>
        public BoundingBox? Constraint { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public DrawingDataDescription()
        {

        }
    }
}
