using SharpDX;

namespace Engine
{
    /// <summary>
    /// Sprite progress bar description
    /// </summary>
    public class SpriteProgressBarDescription : SceneObjectDescription
    {
        /// <summary>
        /// Left position
        /// </summary>
        public int Left { get; set; }
        /// <summary>
        /// Top position
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Base color
        /// </summary>
        public Color BaseColor { get; set; }
        /// <summary>
        /// Progress color
        /// </summary>
        public Color ProgressColor { get; set; }
        /// <summary>
        /// Button text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Text description
        /// </summary>
        public TextDrawerDescription TextDescription { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteProgressBarDescription()
            : base()
        {
            this.Static = true;
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
