using Engine.Common;

namespace Engine.BuiltIn.Drawers.Shadows
{
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
        public BuiltInPosition(Game game) : base(game)
        {
            SetVertexShader<PositionVs>();
            SetGeometryShader<ShadowsGs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLight>();
            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSingle>();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            context.DeviceContext.UpdateConstantBuffer(cbPerLight, PerCastingLight.Build(context));

            var geometryShader = GetGeometryShader<ShadowsGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
        /// <inheritdoc/>
        public override void UpdateMesh(IEngineDeviceContext dc, BuiltInDrawerMeshState state)
        {
            dc.UpdateConstantBuffer(cbPerMesh, PerMeshSingle.Build(state));

            var vertexShader = GetVertexShader<PositionVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
    }
}
