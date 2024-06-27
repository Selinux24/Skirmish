using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Deferred
{
    using Engine.BuiltIn.Drawers;
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
            Shader = BuiltInShaders.CompilePixelShader<PositionColorPs>("main", DeferredRenderingResources.PositionColor_ps);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            //No resources
        }
    }
}
