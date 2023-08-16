using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Deferred light vertex shader
    /// </summary>
    public class DeferredLightOrthoVs : IBuiltInShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DeferredLightOrthoVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<DeferredLightOrthoVs>("main", DeferredRenderingResources.DeferredLightOrtho_vs);
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
