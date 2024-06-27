using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Shadows
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Skinned position instanced vertex shader
    /// </summary>
    public class PositionSkinnedVsI : IBuiltInShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionSkinnedVsI()
        {
            Shader = BuiltInShaders.CompileVertexShader<PositionSkinnedVsI>("main", ShadowRenderingResources.PositionSkinnedI_vs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobalConstantBuffer(),
            };

            dc.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetAnimationPaletteResourceView(),
            };

            dc.SetVertexShaderResourceViews(0, rv);
        }
    }
}
