using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Deferred
{
    /// <summary>
    /// Deferred light vertex shader
    /// </summary>
    public class DeferredLightOrthoVs : IShader<EngineVertexShader>
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
