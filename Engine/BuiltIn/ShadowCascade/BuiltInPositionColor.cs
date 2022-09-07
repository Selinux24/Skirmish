using System;

namespace Engine.BuiltIn.ShadowCascade
{
    /// <summary>
    /// Shadow position-color drawer
    /// </summary>
    public class BuiltInPositionColor : BuiltInDrawer, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionColorVs>();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionColor()
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

            }
        }
    }
}
