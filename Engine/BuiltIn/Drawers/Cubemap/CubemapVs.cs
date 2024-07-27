﻿using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Cubemap
{
    /// <summary>
    /// Cubemap vertex shader
    /// </summary>
    public class CubemapVs : IShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CubemapVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<CubemapVs>("main", ForwardRenderingResources.Cubemap_vs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
