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
        public int Left;
        /// <summary>
        /// Top position
        /// </summary>
        public int Top;
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Base color
        /// </summary>
        public Color BaseColor;
        /// <summary>
        /// Progress color
        /// </summary>
        public Color ProgressColor;
        /// <summary>
        /// Button text
        /// </summary>
        public string Text;
        /// <summary>
        /// Text description
        /// </summary>
        public TextDrawerDescription TextDescription;

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
