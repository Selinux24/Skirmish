using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-texture instanced drawer
    /// </summary>
    public class BasicPositionNormalTextureSkinnedInstanced : BuiltInDrawer
    {
        private readonly EngineSamplerState linear;
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalTextureSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionNormalTextureSkinnedVsI>();
            SetPixelShader<BasicPositionNormalTexturePs>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public void Update(MaterialDrawInfo material, Color4 tintColor)
        {
            var vertexShader = GetVertexShader<BasicPositionNormalTextureSkinnedVsI>();
            vertexShader?.WriteCBPerObject(material, tintColor);

            var sampler = material.UseAnisotropic ? anisotropic : linear;
            var pixelShader = GetPixelShader<BasicPositionNormalTexturePs>();
            pixelShader?.SetDiffuseMap(material.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(sampler);
        }
    }
}
