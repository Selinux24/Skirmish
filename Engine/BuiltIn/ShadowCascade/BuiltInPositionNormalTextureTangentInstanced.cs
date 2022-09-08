
namespace Engine.BuiltIn.ShadowCascade
{
    using Engine.Common;

    /// <summary>
    /// Shadow position-normal-texture-tangent instanced drawer
    /// </summary>
    public class BuiltInPositionNormalTextureTangentInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLight> cbPerLight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTextureTangentInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalTextureTangentVsI>();
            SetGeometryShader<CascadeGs>();
       
            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLight>();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            cbPerLight.WriteData(PerCastingLight.Build(context));

            var geometryShader = GetGeometryShader<CascadeGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
    }
}
