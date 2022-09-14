
namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;

    /// <summary>
    /// Point shadow position instanced drawer
    /// </summary>
    public class BuiltInPointPositionInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLightPoint> cbPerLight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPointPositionInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionVsI>();
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
