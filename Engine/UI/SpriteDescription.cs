using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Sprite description
    /// </summary>
    public class SpriteDescription : UIControlDescription
    {
        /// <summary>
        /// Gets a sprite description
        /// </summary>
        /// <param name="baseColor">Base color</param>
        public static SpriteDescription Default(Color4 baseColor)
        {
            var blendMode = baseColor.Alpha >= 1f ? BlendModes.Default : BlendModes.DefaultTransparent;

            return new SpriteDescription
            {
                TintColor = baseColor,
                BlendMode = blendMode,
            };
        }
        /// <summary>
        /// Gets a sprite description
        /// </summary>
        /// <param name="fileName">Texture file name</param>
        public static SpriteDescription FromFile(string fileName)
        {
            return new SpriteDescription
            {
                Textures = new[] { fileName }
            };
        }

        /// <summary>
        /// Sprite textures
        /// </summary>
        public string[] Textures { get; set; }
        /// <summary>
        /// Initial texture index
        /// </summary>
        public int TextureIndex { get; set; } = 0;
        /// <summary>
        /// UV map
        /// </summary>
        public Vector4? UVMap { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteDescription()
            : base()
        {

        }
    }
}
