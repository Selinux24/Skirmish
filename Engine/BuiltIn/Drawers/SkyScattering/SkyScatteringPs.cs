using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.SkyScattering
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Sky scatering pixel shader
    /// </summary>
    public class SkyScatteringPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Per object constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerObject;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkyScatteringPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<SkyScatteringPs>("main", ForwardRenderingResources.SkyScattering_ps);
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
            dc.SetPixelShaderConstantBuffer(0, cbPerObject);
        }
    }
}
