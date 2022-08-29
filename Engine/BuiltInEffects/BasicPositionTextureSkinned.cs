using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Skinned position-texture drawer
    /// </summary>
    public class BasicPositionTextureSkinned : BuiltInDrawer
    {
        private readonly EngineSamplerState linear;
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionTextureSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionTextureSkinnedVs>();
            SetPixelShader<BasicPositionTexturePs>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionTextureSkinnedVs>();
            vertexShader?.WriteCBPerInstance(material, tintColor, textureIndex, animation);

            var sampler = material.UseAnisotropic ? anisotropic : linear;
            var pixelShader = GetPixelShader<BasicPositionTexturePs>();
            pixelShader?.SetDiffuseMap(material.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(sampler);
        }
    }
}
