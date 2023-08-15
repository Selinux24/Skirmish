using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position vertex shader
    /// </summary>
    public class PositionVs : IBuiltInShader<EngineVertexShader>
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMesh;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader($"{nameof(Shadows)}_{nameof(PositionVs)}", "main", ShadowRenderingResources.Position_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionVs()
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

        /// <summary>
        /// Sets per mesh constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerMeshConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerMesh = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                cbPerMesh,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
