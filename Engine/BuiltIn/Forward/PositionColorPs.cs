using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Forward
{
    using Engine.Common;

    /// <summary>
    /// Position color pixel shader
    /// </summary>
    public class PositionColorPs : IBuiltInShader<EnginePixelShader>
    {
        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionColorPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<PositionColorPs>("main", ForwardRenderingResources.PositionColor_ps);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetPixelShaderConstantBuffer(0, BuiltInShaders.GetPerFrameConstantBuffer());
        }
    }
}
