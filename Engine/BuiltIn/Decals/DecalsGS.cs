using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Decals
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Decals geometry shader
    /// </summary>
    public class DecalsGS : IBuiltInShader<EngineGeometryShader>
    {
        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <inheritdoc/>
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public DecalsGS(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileGeometryShader(nameof(DecalsGS), "main", ForwardRenderingResources.Decal_gs, HelperShaders.GSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~DecalsGS()
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
                Shader = null;
            }
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
            };

            dc.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
