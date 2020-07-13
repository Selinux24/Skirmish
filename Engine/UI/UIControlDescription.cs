using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// User interface control description
    /// </summary>
    public abstract class UIControlDescription : SceneObjectDescription
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
        /// Fit screen
        /// </summary>
        public bool FitParent { get; set; } = false;
        /// <summary>
        /// Vertically centered
        /// </summary>
        public CenterTargets CenterVertically { get; set; } = CenterTargets.None;
        /// <summary>
        /// Horizontally centered
        /// </summary>
        public CenterTargets CenterHorizontally { get; set; } = CenterTargets.None;
        /// <summary>
        /// Sprite color
        /// </summary>
        public Color4 Color { get; set; } = Color4.White;
        /// <summary>
        /// Events enabled
        /// </summary>
        public bool EventsEnabled { get; set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        protected UIControlDescription() : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.BlendMode = BlendModes.Alpha;
        }
    }
}
