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
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static SpriteDescription Default(float width = 0, float height = 0)
        {
            return new SpriteDescription()
            {
                Width = width,
                Height = height,
            };
        }
        /// <summary>
        /// Gets a sprite description
        /// </summary>
        /// <param name="baseColor">Base color</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static SpriteDescription Default(Color4 baseColor, float width = 0, float height = 0)
        {
            var blendMode = baseColor.Alpha >= 1f ? BlendModes.Default : BlendModes.DefaultTransparent;

            return new SpriteDescription
            {
                BaseColor = baseColor,
                BlendMode = blendMode,
                Width = width,
                Height = height,
            };
        }
        /// <summary>
        /// Gets a sprite description
        /// </summary>
        /// <param name="fileName">Texture file name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static SpriteDescription Default(string fileName, float width = 0, float height = 0)
        {
            return new SpriteDescription
            {
                Textures = new[] { fileName },
                BaseColor = Color4.White,
                Width = width,
                Height = height,
            };
        }
        /// <summary>
        /// Gets a background sprite description
        /// </summary>
        public static SpriteDescription Background()
        {
            var desc = Default();
            desc.FitParent = true;
            return desc;
        }
        /// <summary>
        /// Gets a background sprite description
        /// </summary>
        /// <param name="baseColor">Base color</param>
        public static SpriteDescription Background(Color4 baseColor)
        {
            var desc = Default(baseColor);
            desc.FitParent = true;
            return desc;
        }
        /// <summary>
        /// Gets a background sprite description
        /// </summary>
        /// <param name="fileName">Texture file name</param>
        public static SpriteDescription Background(string fileName)
        {
            var desc = Default(fileName);
            desc.FitParent = true;
            return desc;
        }

        /// <summary>
        /// First color
        /// </summary>
        public Color4 Color1 { get; set; }
        /// <summary>
        /// Second color
        /// </summary>
        public Color4 Color2 { get; set; }
        /// <summary>
        /// Third color
        /// </summary>
        public Color4 Color3 { get; set; }
        /// <summary>
        /// Fourth color
        /// </summary>
        public Color4 Color4 { get; set; }
        /// <summary>
        /// First percentage
        /// </summary>
        public float Percentage1 { get; set; }
        /// <summary>
        /// Second percentage
        /// </summary>
        public float Percentage2 { get; set; }
        /// <summary>
        /// Third percentage
        /// </summary>
        public float Percentage3 { get; set; }
        /// <summary>
        /// Draw direction
        /// </summary>
        public SpriteDrawDirections DrawDirection { get; set; } = SpriteDrawDirections.Horizontal;
        /// <summary>
        /// Sprite textures
        /// </summary>
        public string[] Textures { get; set; }
        /// <summary>
        /// Initial texture index
        /// </summary>
        public uint TextureIndex { get; set; } = 0;
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
            EventsEnabled = false;
        }
    }
}
