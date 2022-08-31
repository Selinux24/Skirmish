using Engine.Shaders.Properties;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Skinned position texture vertex shader
    /// </summary>
    public class PositionTextureSkinnedVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMesh;
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
        public PositionTextureSkinnedVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(PositionTextureSkinnedVs), "main", ShaderShadowBasicResources.PositionTextureSkinned_vs, HelperShaders.VSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionTextureSkinnedVs()
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
                BuiltInShaders.GetGlobal(),
                BuiltInShaders.GetVSPerFrame(),
                cbPerMesh,
                cbPerMaterial,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetAnimationPalette(),
            };

            Graphics.SetVertexShaderResourceViews(0, rv);
        }
    }
}
