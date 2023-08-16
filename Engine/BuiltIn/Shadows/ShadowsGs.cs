using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;

    /// <summary>
    /// Point geometry shader
    /// </summary>
    public class ShadowsGs : IBuiltInShader<EngineGeometryShader>
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerCastingLight;

        /// <inheritdoc/>
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ShadowsGs()
        {
            Shader = BuiltInShaders.CompileGeometryShader<ShadowsGs>("main", ShadowRenderingResources.Shadow_gs);
        }

        /// <summary>
        /// Sets per mesh constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerCastingLightConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerCastingLight = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                cbPerCastingLight,
            };

            dc.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
