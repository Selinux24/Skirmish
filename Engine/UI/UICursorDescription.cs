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
            return Default(texture, width, height, centered, Vector2.Zero, Color.White);
        }
        /// <summary>
        /// Gets the default cursor description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="centered">Pointer positon is in the texture center</param>
        /// <param name="delta">Position delta from texture to pointer position</param>
        public static UICursorDescription Default(string texture, float width, float height, bool centered, Vector2 delta)
        {
            return Default(texture, width, height, centered, delta, Color.White);
        }
        /// <summary>
        /// Gets the default cursor description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="centered">Pointer positon is in the texture center</param>
        /// <param name="baseColor">Base color</param>
        public static UICursorDescription Default(string texture, float width, float height, bool centered, Color4 baseColor)
        {
            return Default(texture, width, height, centered, Vector2.Zero, baseColor);
        }
        /// <summary>
        /// Gets the default cursor description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="centered">Pointer positon is in the texture center</param>
        /// <param name="delta">Position delta from texture to pointer position</param>
        /// <param name="baseColor">Base color</param>
        public static UICursorDescription Default(string texture, float width, float height, bool centered, Vector2 delta, Color4 baseColor)
        {
            return new()
            {
                Textures = [texture],
                Width = width,
                Height = height,
                Delta = delta,
                Centered = centered,
                BaseColor = baseColor,
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
