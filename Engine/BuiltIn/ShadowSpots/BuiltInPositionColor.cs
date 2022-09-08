
namespace Engine.BuiltIn.ShadowSpots
{
    using Engine.Common;

    /// <summary>
    /// Shadow position-color drawer
    /// </summary>
    public class BuiltInPositionColor : BuiltInDrawer
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSingle> cbPerMesh;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionColorVs>();

            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSingle>();
        }

        /// <inheritdoc/>
        public override void UpdateMesh(BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMeshSingle.Build(state));

            var vertexShader = GetVertexShader<PositionColorVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
    }
}
