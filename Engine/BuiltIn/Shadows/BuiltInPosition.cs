
namespace Engine.BuiltIn.Shadows
{
    using Engine.BuiltIn.Foliage;
    using Engine.Common;

    /// <summary>
    /// Point shadow position drawer
    /// </summary>
    public class BuiltInPosition : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLight> cbPerLight;
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSingle> cbPerMesh;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPosition(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionVs>();
            SetGeometryShader<ShadowsGs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLight>();
            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSingle>();
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
        public override void UpdateMesh(IEngineDeviceContext dc, BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMeshSingle.Build(state));
            dc.UpdateConstantBuffer(cbPerMesh);

            var vertexShader = GetVertexShader<PositionVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
    }
}
