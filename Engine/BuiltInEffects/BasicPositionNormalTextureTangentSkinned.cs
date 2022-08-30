using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-texture-tangent drawer
    /// </summary>
    public class BasicPositionNormalTextureTangentSkinned : BuiltInDrawer
    {
        private readonly EngineSamplerState linear;
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalTextureTangentSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionNormalTextureTangentSkinnedVs>();
            SetPixelShader<BasicPositionNormalTextureTangentPs>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionNormalTextureTangentSkinnedVs>();
            vertexShader?.WriteCBPerInstance(material, tintColor, textureIndex, animation);

            var sampler = material.UseAnisotropic ? anisotropic : linear;
            var pixelShader = GetPixelShader<BasicPositionNormalTextureTangentPs>();
            pixelShader?.SetDiffuseMap(material.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(sampler);
            pixelShader?.SetNormalMap(material.Material?.NormalMap);
            pixelShader?.SetNormalSampler(sampler);
        }
    }
}
