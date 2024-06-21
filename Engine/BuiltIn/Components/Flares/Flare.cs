using Engine.UI;
using SharpDX;
using System;

namespace Engine.BuiltIn.Components.Flares
{
    /// <summary>
    /// Flare
    /// </summary>
    public class Flare : IDisposable
    {
        /// <summary>
        /// Sripte
        /// </summary>
        public Sprite FlareSprite { get; set; }
        /// <summary>
        /// Distance from light source along light ray
        /// </summary>
        public float Distance { get; set; }
        /// <summary>
        /// Relative Scale
        /// </summary>
        public float Scale { get; set; }
        /// <summary>
        /// Color
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Flare()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
                FlareSprite?.Dispose();
                FlareSprite = null;
            }
        }
    }
}
