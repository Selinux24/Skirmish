﻿using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Shadows
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Position texture vertex shader
    /// </summary>
    public class PositionTextureVs : IBuiltInShader<EngineVertexShader>
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMesh;
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMaterial;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionTextureVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<PositionTextureVs>("main", ShadowRenderingResources.PositionTexture_vs);
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
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                cbPerMesh,
                cbPerMaterial,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
