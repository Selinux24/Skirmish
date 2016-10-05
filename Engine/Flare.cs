using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Flare
    /// </summary>
    public class Flare : IDisposable
    {
        /// <summary>
        /// Sripte
        /// </summary>
        public Sprite FlareSprite;
        /// <summary>
        /// Relative Position
        /// </summary>
        public float Position;
        /// <summary>
        /// Relative Scale
        /// </summary>
        public float Scale;
        /// <summary>
        /// Color
        /// </summary>
        public Color Color;

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.FlareSprite);
        }
    }
}
