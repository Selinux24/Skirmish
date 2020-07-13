
namespace Engine
{
    using Engine.Content;

    /// <summary>
    /// Ground description
    /// </summary>
    public class GroundDescription : SceneObjectDescription
    {
        /// <summary>
        /// Quadtree description
        /// </summary>
        public class QuadtreeDescription
        {
            /// <summary>
            /// Maximum depth
            /// </summary>
            public int MaximumDepth { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public QuadtreeDescription()
            {
                this.MaximumDepth = 3;
            }
        }

        /// <summary>
        /// Content
        /// </summary>
        public ContentDescription Content { get; set; } = new ContentDescription();
        /// <summary>
        /// Quadtree
        /// </summary>
        public QuadtreeDescription Quadtree { get; set; } = new QuadtreeDescription();
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropic { get; set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public GroundDescription()
            : base()
        {
            this.CastShadow = true;
        }
    }
}
