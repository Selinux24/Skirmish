using SharpDX;

namespace Engine
{
    /// <summary>
    /// Lens flare description
    /// </summary>
    public class LensFlareDescription : SceneObjectDescription
    {
        /// <summary>
        /// Flare description
        /// </summary>
        public class Flare
        {
            /// <summary>
            /// Relative position
            /// </summary>
            public float Position { get; set; }
            /// <summary>
            /// Relative scale
            /// </summary>
            public float Scale { get; set; }
            /// <summary>
            /// Color
            /// </summary>
            public Color Color { get; set; }
            /// <summary>
            /// Texture
            /// </summary>
            public string Texture { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="position">Relative position</param>
            /// <param name="scale">Relative scale</param>
            /// <param name="color">Color</param>
            /// <param name="texture">Texture name</param>
            public Flare(float position, float scale, Color color, string texture)
            {
                this.Position = position;
                this.Scale = scale;
                this.Color = color;
                this.Texture = texture;
            }
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; }
        /// <summary>
        /// Glow texture
        /// </summary>
        public string GlowTexture { get; set; }
        /// <summary>
        /// Flare collection
        /// </summary>
        public Flare[] Flares { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LensFlareDescription()
            : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
