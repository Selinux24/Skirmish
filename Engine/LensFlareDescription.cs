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
            /// Distance from light source along light ray
            /// </summary>
            public float Distance { get; set; }
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
            /// <param name="distance">Distance from light source along light ray</param>
            /// <param name="scale">Relative scale</param>
            /// <param name="color">Color</param>
            /// <param name="texture">Texture name</param>
            public Flare(float distance, float scale, Color color, string texture)
            {
                Distance = distance;
                Scale = scale;
                Color = color;
                Texture = texture;
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
            DeferredEnabled = false;
            DepthEnabled = false;
            BlendMode = BlendModes.Alpha | BlendModes.Additive;
        }
    }
}
