
namespace Engine.BuiltIn.Deferred
{
    using Engine.BuiltIn.Common;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-color instanced drawer
    /// </summary>
    public class BuiltInPositionNormalColorSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorSkinnedVsI>();
            SetPixelShader<PositionNormalColorPs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(EngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(dc, PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorSkinnedVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
