using Engine.Common;

namespace Engine.BuiltIn.Drawers.Shadows
{
    /// <summary>
    /// Point shadow position instanced drawer
    /// </summary>
    public class BuiltInPositionInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLight> cbPerLight;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPositionInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionVsI>();
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
