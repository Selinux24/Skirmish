
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
        /// Gets a ground description from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public static GroundDescription FromFile(string fileName)
        {
            return new GroundDescription()
            {
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ModelContentFilename = fileName,
                }
            };
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
