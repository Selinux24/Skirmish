using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Deferred stencil vertex shader
    /// </summary>
    public class DeferredStencilVs : IBuiltInShader<EngineVertexShader>
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
