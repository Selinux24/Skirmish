namespace Engine
{
    using Engine.Common;

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
        /// Quadtree
        /// </summary>
        public QuadtreeDescription Quadtree = new QuadtreeDescription();

        /// <summary>
        /// Delay internal generation
        /// </summary>
        public bool DelayGeneration = false;

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
}
