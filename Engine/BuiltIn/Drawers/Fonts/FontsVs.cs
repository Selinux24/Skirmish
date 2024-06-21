using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Fonts
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Fonts vertex shader
    /// </summary>
    public class FontsVs : IBuiltInShader<EngineVertexShader>
    {
        /// <summary>
        /// Per text constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerText;

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FontsVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<FontsVs>("main", UIRenderingResources.Font_vs);
        }

        /// <summary>
        /// Sets per text constant buffer
        /// </summary>
        public void SetPerTextConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerText = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerText,
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
