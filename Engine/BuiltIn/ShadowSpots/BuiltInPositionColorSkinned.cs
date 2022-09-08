
namespace Engine.BuiltIn.ShadowSpots
{
    using Engine.Common;

    /// <summary>
    /// Shadow skinned position-color drawer
    /// </summary>
    public class BuiltInPositionColorSkinned : BuiltInDrawer
    {
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
        public BuiltInPositionColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionColorSkinnedVs>();

            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSkinned>();
        }

        /// <inheritdoc/>
        public override void UpdateMesh(BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMeshSkinned.Build(state));

            var vertexShader = GetVertexShader<PositionColorSkinnedVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
    }
}
