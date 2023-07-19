
namespace Engine.BuiltIn.Forward
{
    using Engine.BuiltIn.Common;
    using Engine.Common;

    /// <summary>
    /// Skinned position-color instanced drawer
    /// </summary>
    public class BuiltInPositionColorSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionColorSkinnedVsI>();
            SetPixelShader<PositionColorPs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(EngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterialColor.Build(state));
            dc.UpdateConstantBuffer(cbPerMaterial);

            var vertexShader = GetVertexShader<PositionColorSkinnedVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
