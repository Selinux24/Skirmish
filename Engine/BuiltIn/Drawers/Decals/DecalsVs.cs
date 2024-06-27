using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Decals
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Decals vertex shader
    /// </summary>
    public class DecalsVs : IBuiltInShader<EngineVertexShader>
    {
        /// <summary>
        /// Per decal constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerDecal;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DecalsVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<DecalsVs>("main", ForwardRenderingResources.Decal_vs);
        }

        /// <summary>
        /// Sets per decal constant buffer
        /// </summary>
        public void SetPerDecalConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerDecal = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerDecal,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
