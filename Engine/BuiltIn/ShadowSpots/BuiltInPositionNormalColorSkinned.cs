
namespace Engine.BuiltIn.ShadowSpots
{
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-normal-color drawer
    /// </summary>
    public class BuiltInPositionNormalColorSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSkinned> cbPerMesh;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorSkinnedVs>();

            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSkinned>();
        }

        /// <inheritdoc/>
        public override void UpdateMesh(BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMeshSkinned.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorSkinnedVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
    }
}
