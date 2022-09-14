
namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;

    /// <summary>
    /// Spot shadow skinned position instanced drawer
    /// </summary>
    public class BuiltInSpotPositionSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLightSpot> cbPerLight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInSpotPositionSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionSkinnedVsI>();
            SetGeometryShader<SpotGs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLightSpot>();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            cbPerLight.WriteData(PerCastingLightSpot.Build(context));

            var geometryShader = GetGeometryShader<SpotGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
    }
}
