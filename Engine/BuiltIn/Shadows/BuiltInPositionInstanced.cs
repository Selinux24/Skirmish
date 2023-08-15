
namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;

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
        public BuiltInPositionInstanced() : base()
        {
            SetVertexShader<PositionVsI>();
            SetGeometryShader<ShadowsGs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLight>();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            cbPerLight.WriteData(PerCastingLight.Build(context));
            context.DeviceContext.UpdateConstantBuffer(cbPerLight);

            var geometryShader = GetGeometryShader<ShadowsGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
    }
}
