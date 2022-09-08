using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Skinned position normal texture instanced vertex shader
    /// </summary>
    public class PositionNormalTextureSkinnedVsI : IBuiltInVertexShader
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMaterial;

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
        public PositionNormalTextureSkinnedVsI(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(PositionNormalTextureSkinnedVsI), "main", ShaderDefaultBasicResources.PositionNormalTextureSkinnedI_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalTextureSkinnedVsI()
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
        /// Sets per material constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerMaterialConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerMaterial = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobalConstantBuffer(),
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerMaterial,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetMaterialPaletteResourceView(),
                BuiltInShaders.GetAnimationPaletteResourceView(),
            };

            Graphics.SetVertexShaderResourceViews(0, rv);
        }
    }
}
