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
        public int Top { get; set; } = 0;
        /// <summary>
        /// Left position
        /// </summary>
        public int Left { get; set; } = 0;
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; } = 200;
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; } = 50;
        /// <summary>
        /// Fit screen
        /// </summary>
        public bool FitParent { get; set; } = false;
        /// <summary>
        /// Vertically centered
        /// </summary>
        public bool CenterVertically { get; set; } = false;
        /// <summary>
        /// Horizontally centered
        /// </summary>
        public bool CenterHorizontally { get; set; } = false;
        /// <summary>
        /// Sprite color
        /// </summary>
        public Color4 Color { get; set; } = Color4.White;

        /// <summary>
        /// Constructor
        /// </summary>
        protected UIControlDescription() : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
