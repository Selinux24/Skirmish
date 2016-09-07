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
        public bool Instanced = false;
        /// <summary>
        /// Gets or sets the number of instances
        /// </summary>
        public int Instances = 0;
        /// <summary>
        /// Gets or sets whether the animation data must be loaded or not
        /// </summary>
        public bool LoadAnimation = false;
        /// <summary>
        /// Gets or sets the texture count
        /// </summary>
        public int TextureCount = 0;
        /// <summary>
        /// Gets or sets whether the model uses normal maps or not
        /// </summary>
        public bool LoadNormalMaps = false;
        /// <summary>
        /// Gets or sets whether the model uses dynamic buffers or not
        /// </summary>
        public bool DynamicBuffers = false;
        /// <summary>
        /// Gets or sets the bounding volume constraint for vertex generation
        /// </summary>
        public BoundingBox? Constraint = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public DrawingDataDescription()
        {

        }
    }
}
