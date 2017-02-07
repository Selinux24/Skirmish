namespace Engine
{
    using Engine.Common;
    using Engine.PathFinding;

    /// <summary>
    /// Ground description
    /// </summary>
    public class GroundDescription : DrawableDescription
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
        /// Path finder grid description
        /// </summary>
        public class PathFinderDescription
        {
            /// <summary>
            /// Graph type
            /// </summary>
            public PathFinderSettings Settings = null;
        }

        /// <summary>
        /// Quadtree
        /// </summary>
        public QuadtreeDescription Quadtree = new QuadtreeDescription();
        /// <summary>
        /// Path finder
        /// </summary>
        public PathFinderDescription PathFinder = null;

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
