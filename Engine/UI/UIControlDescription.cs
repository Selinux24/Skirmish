using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// User interface control description
    /// </summary>
    public abstract class UIControlDescription : SceneDrawableDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";
        /// <summary>
        /// Top position
        /// </summary>
        public float Top { get; set; } = 0;
        /// <summary>
        /// Left position
        /// </summary>
        public float Left { get; set; } = 0;
        /// <summary>
        /// Width
        /// </summary>
        public float Width { get; set; } = 0;
        /// <summary>
        /// Height
        /// </summary>
        public float Height { get; set; } = 0;
        /// <summary>
        /// Spacing
        /// </summary>
        public Spacing Spacing { get; set; } = UIConfiguration.Spacing;
        /// <summary>
        /// Padding
        /// </summary>
        public Padding Padding { get; set; } = UIConfiguration.Padding;
        /// <summary>
        /// Fit screen
        /// </summary>
        public bool FitParent { get; set; } = false;
        /// <summary>
        /// Anchor
        /// </summary>
        public Anchors Anchor { get; set; } = Anchors.None;
        /// <summary>
        /// Base color
        /// </summary>
        public Color4 BaseColor { get; set; } = UIConfiguration.BaseColor;
        /// <summary>
        /// Tint color
        /// </summary>
        public Color4 TintColor { get; set; } = Color4.White;
        /// <summary>
        /// Events enabled
        /// </summary>
        public bool EventsEnabled { get; set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        protected UIControlDescription() : base()
        {
            CastShadow = false;
            DeferredEnabled = false;
            DepthEnabled = false;
            BlendMode = BlendModes.Alpha;
        }
    }
}
