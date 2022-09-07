using System;

namespace Engine.BuiltIn.ShadowCascade
{
    /// <summary>
    /// Shadow position-normal-color drawer
    /// </summary>
    public class BuiltInPositionNormalColor : BuiltInDrawer, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorVs>();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionNormalColor()
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
