using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Empty shader
    /// </summary>
    public class Empty<T> : IBuiltInShader<T> where T : IEngineShader
    {
        /// <inheritdoc/>
        public T Shader { get; private set; }

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public Empty(Graphics graphics)
        {
            Graphics = graphics;
            Shader = default;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Empty()
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
                Shader?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            // Empty shader
        }
    }
}
