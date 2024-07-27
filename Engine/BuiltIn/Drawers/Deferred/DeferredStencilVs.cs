﻿using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Deferred
{
    /// <summary>
    /// Deferred stencil vertex shader
    /// </summary>
    public class DeferredStencilVs : IShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DeferredStencilVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<DeferredStencilVs>("main", DeferredRenderingResources.DeferredStencil_vs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetVertexShaderConstantBuffer(0, BuiltInShaders.GetPerFrameConstantBuffer());
        }
    }
}
