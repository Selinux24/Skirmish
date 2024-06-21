using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Decals
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Decals geometry shader
    /// </summary>
    public class DecalsGS : IBuiltInShader<EngineGeometryShader>
    {
        /// <inheritdoc/>
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DecalsGS()
        {
            Shader = BuiltInShaders.CompileGeometryShader<DecalsGS>("main", ForwardRenderingResources.Decal_gs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
            };

            dc.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
