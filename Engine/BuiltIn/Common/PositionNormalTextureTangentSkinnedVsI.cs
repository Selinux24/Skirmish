using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Common
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Skinned position normal texture tangent instanced vertex shader
    /// </summary>
    public class PositionNormalTextureTangentSkinnedVsI : IBuiltInVertexShader
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
        public PositionNormalTextureTangentSkinnedVsI(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(PositionNormalTextureTangentSkinnedVsI), "main", CommonResources.PositionNormalTextureTangentSkinnedI_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalTextureTangentSkinnedVsI()
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
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobalConstantBuffer(),
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerMaterial,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetMaterialPaletteResourceView(),
                BuiltInShaders.GetAnimationPaletteResourceView(),
            };

            dc.SetVertexShaderResourceViews(0, rv);
        }
    }
}
