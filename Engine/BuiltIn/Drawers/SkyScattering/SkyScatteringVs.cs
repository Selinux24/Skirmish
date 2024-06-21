using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.SkyScattering
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Sky scatering vertex shader
    /// </summary>
    public class SkyScatteringVs : IBuiltInShader<EngineVertexShader>
    {
        /// <summary>
        /// Per object constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerObject;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkyScatteringVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<SkyScatteringVs>("main", ForwardRenderingResources.SkyScattering_vs);
        }

        /// <summary>
        /// Sets per object constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerObjectConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerObject = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerObject,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
