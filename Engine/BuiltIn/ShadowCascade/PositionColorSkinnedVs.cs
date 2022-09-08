using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.ShadowCascade
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Skinned position color vertex shader
    /// </summary>
    public class PositionColorSkinnedVs : IBuiltInVertexShader
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
        public PositionColorSkinnedVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(PositionColorSkinnedVs), "main", ShaderShadowCascadeResources.PositionColorSkinned_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionColorSkinnedVs()
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
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobalConstantBuffer(),
                cbPerMesh,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetAnimationPaletteResourceView(),
            };

            Graphics.SetVertexShaderResourceViews(0, rv);
        }
    }
}
