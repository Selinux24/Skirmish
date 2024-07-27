using Engine.Common;

namespace Engine.BuiltIn.Drawers.Shadows
{
    /// <summary>
    /// Point shadow skinned position instanced drawer
    /// </summary>
    public class BuiltInPositionSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLight> cbPerLight;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPositionSkinnedInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionSkinnedVsI>();
            SetGeometryShader<ShadowsGs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLight>();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            context.DeviceContext.UpdateConstantBuffer(cbPerLight, PerCastingLight.Build(context));

            var geometryShader = GetGeometryShader<ShadowsGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
    }
}
