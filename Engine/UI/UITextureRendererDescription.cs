using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Minimap description
    /// </summary>
    public class UITextureRendererDescription : UIControlDescription
    {
        /// <summary>
        /// Gets a texture renderer description
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static UITextureRendererDescription Default(float width = 0, float height = 0)
        {
            return new()
            {
                Channel = ColorChannels.NoAlpha,
                Width = width,
                Height = height,
                BaseColor = Color4.White,
            };
        }
        /// <summary>
        /// Gets a texture renderer description
        /// </summary>
        /// <param name="fileName">Texture file name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static UITextureRendererDescription Default(string fileName, float width = 0, float height = 0)
        {
            return new()
            {
                Textures = [fileName],
                Channel = ColorChannels.NoAlpha,
                Width = width,
                Height = height,
                BaseColor = Color4.White,
            };
        }
        /// <summary>
        /// Gets a texture renderer description
        /// </summary>
        /// <param name="bounds">Control bounds</param>
        public static UITextureRendererDescription Default(RectangleF bounds)
        {
            return new()
            {
                Channel = ColorChannels.NoAlpha,
                Left = bounds.Left,
                Top = bounds.Top,
                Width = bounds.Width,
                Height = bounds.Height,
                BaseColor = Color4.White,
            };
        }
        /// <summary>
        /// Gets a texture renderer description
        /// </summary>
        /// <param name="fileName">Texture file name</param>
        /// <param name="bounds">Control bounds</param>
        public static UITextureRendererDescription Default(string fileName, RectangleF bounds)
        {
            return new()
            {
                Textures = [fileName],
                Channel = ColorChannels.NoAlpha,
                Left = bounds.Left,
                Top = bounds.Top,
                Width = bounds.Width,
                Height = bounds.Height,
                BaseColor = Color4.White,
            };
        }
        /// <summary>
        /// Gets a texture renderer description
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static UITextureRendererDescription Default(float left, float top, float width, float height)
        {
            return new()
            {
                Channel = ColorChannels.NoAlpha,
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                BaseColor = Color4.White,
            };
        }
        /// <summary>
        /// Gets a texture renderer description
        /// </summary>
        /// <param name="fileName">Texture file name</param>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static UITextureRendererDescription Default(string fileName, float left, float top, float width, float height)
        {
            return new()
            {
                Textures = [fileName],
                Channel = ColorChannels.NoAlpha,
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                BaseColor = Color4.White,
            };
        }

        /// <summary>
        /// Sprite textures
        /// </summary>
        public string[] Textures { get; set; }
        /// <summary>
        /// Initial texture index
        /// </summary>
        public uint TextureIndex { get; set; } = 0;
        /// <summary>
        /// Channel color
        /// </summary>
        public ColorChannels Channel { get; set; } = ColorChannels.Default;

        /// <summary>
        /// Constructor
        /// </summary>
        public UITextureRendererDescription()
            : base()
        {
            EventsEnabled = false;
        }
    }
}
