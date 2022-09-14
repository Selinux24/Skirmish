
namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;

    /// <summary>
    /// Point shadow skinned position instanced drawer
    /// </summary>
    public class BuiltInPointPositionSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLightPoint> cbPerLight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPointPositionSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionSkinnedVsI>();
            SetGeometryShader<PointGs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLightPoint>();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            cbPerLight.WriteData(PerCastingLightPoint.Build(context));

            var geometryShader = GetGeometryShader<PointGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
    }
}
