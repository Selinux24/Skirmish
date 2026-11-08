using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Decals
{
    /// <summary>
    /// Decals vertex shader
    /// </summary>
    public class DecalsVs : IShader<EngineVertexShader>
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
