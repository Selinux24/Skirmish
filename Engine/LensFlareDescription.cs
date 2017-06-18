using SharpDX;

namespace Engine
{
    using Engine.Common;

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
            public float Position;
            /// <summary>
            /// Relative scale
            /// </summary>
            public float Scale;
            /// <summary>
            /// Color
            /// </summary>
            public Color Color;
            /// <summary>
            /// Texture
            /// </summary>
            public string Texture;

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
        public string ContentPath;
        /// <summary>
        /// Glow texture
        /// </summary>
        public string GlowTexture;
        /// <summary>
        /// Flare collection
        /// </summary>
        public Flare[] Flares;

        /// <summary>
        /// Constructor
        /// </summary>
        public LensFlareDescription()
            : base()
        {
            this.Static = false;
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
