
namespace Engine.BuiltIn.ShadowCascade
{
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-normal-texture instanced drawer
    /// </summary>
    public class BuiltInPositionNormalTextureSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLight> cbPerLight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTextureSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalTextureSkinnedVsI>();
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
