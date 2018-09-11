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
        /// Destructor
        /// </summary>
        ~Flare()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (FlareSprite != null)
                {
                    FlareSprite.Dispose();
                    FlareSprite = null;
                }
            }
        }
    }
}
