namespace Engine.Common
{
    /// <summary>
    /// Model base description
    /// </summary>
    public abstract class ModelBaseDescription : SceneObjectDescription
    {
        /// <summary>
        /// Optimize geometry
        /// </summary>
        public bool Optimize = true;
        /// <summary>
        /// Instancing model
        /// </summary>
        public bool Instanced { get; protected set; }
        /// <summary>
        /// Instances
        /// </summary>
        public int Instances = 1;
        /// <summary>
        /// Load animation
        /// </summary>
        public bool LoadAnimation = true;
        /// <summary>
        /// Load normal maps
        /// </summary>
        public bool LoadNormalMaps = true;
        /// <summary>
        /// Dynamic buffers
        /// </summary>
        public bool Dynamic = false;
        /// <summary>
        /// Content info
        /// </summary>
        public ContentDescription Content = new ContentDescription();

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelBaseDescription()
            : base()
        {
            this.Instanced = false;
        }
    }
}
