
namespace Engine.BuiltIn.Shadows
{
    using Engine.BuiltIn.Sprites;
    using Engine.Common;

    /// <summary>
    /// Point shadow skinned position drawer
    /// </summary>
    public class BuiltInPositionSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLight> cbPerLight;
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSkinned> cbPerMesh;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionColorVsSkinned">Skinned position color vertex shader</param>
        /// <param name="positionColorPs">Position color pixel shader</param>
        public BuiltInPositionSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionSkinnedVs>();
            SetGeometryShader<ShadowsGs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLight>();
            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSkinned>();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            cbPerLight.WriteData(PerCastingLight.Build(context));
            context.DeviceContext.UpdateConstantBuffer(cbPerLight);

            var geometryShader = GetGeometryShader<ShadowsGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
        /// <inheritdoc/>
        public override void UpdateMesh(EngineDeviceContext dc, BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMeshSkinned.Build(state));
            dc.UpdateConstantBuffer(cbPerMesh);

            var vertexShader = GetVertexShader<PositionSkinnedVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
    }
}
