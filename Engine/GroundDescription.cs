
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
        public ContentDescription Content = new ContentDescription();
        /// <summary>
        /// Quadtree
        /// </summary>
        public QuadtreeDescription Quadtree = new QuadtreeDescription();

        /// <summary>
        /// Constructor
        /// </summary>
        public GroundDescription()
            : base()
        {
            this.Static = true;
            this.CastShadow = true;
            this.DeferredEnabled = true;
            this.DepthEnabled = true;
            this.AlphaEnabled = false;
        }
    }

    public class ContentDescription
    {
        public string ContentFolder;

        public string ModelContentFilename;

        public ModelContent ModelContent;

        public ModelContentDescription ModelContentDescription;

        public HeightmapDescription HeightmapDescription;
    }
}
