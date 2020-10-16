using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Cursor description
    /// </summary>
    public class UICursorDescription : SpriteDescription
    {
        /// <summary>
        /// Gets the default cursor description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="centered">Pointer positon is in the texture center</param>
        public static UICursorDescription Default(string texture, float width, float height, bool centered = false)
        {
            return new UICursorDescription()
            {
                Textures = new[] { texture },
                Width = width,
                Height = height,
                Centered = centered,
                BaseColor = Color4.White,
            };
        }
        /// <summary>
        /// Gets the default cursor description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="delta">Position delta from texture to pointer position</param>
        public static UICursorDescription Default(string texture, float width, float height, Vector2 delta)
        {
            return new UICursorDescription()
            {
                Textures = new[] { texture },
                Width = width,
                Height = height,
                Delta = delta,
                BaseColor = Color4.White,
            };
        }

        /// <summary>
        /// Delta to position
        /// </summary>
        public Vector2 Delta { get; set; } = Vector2.Zero;
        /// <summary>
        /// Center cursor sprite
        /// </summary>
        public bool Centered { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public UICursorDescription() : base()
        {

        }
    }
}
