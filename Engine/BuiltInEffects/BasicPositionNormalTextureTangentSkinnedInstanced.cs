using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-texture-tangent instanced drawer
    /// </summary>
    public class BasicPositionNormalTextureTangentSkinnedInstanced : BuiltInDrawer
    {
        private readonly EngineSamplerState linear;
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalTextureTangentSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionNormalTextureTangentSkinnedVsI>();
            SetPixelShader<BasicPositionNormalTextureTangentPs>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionNormalTextureTangentSkinnedVsI>();
            vertexShader?.WriteCBPerObject(material, tintColor);

            var sampler = material.UseAnisotropic ? anisotropic : linear;
            var pixelShader = GetPixelShader<BasicPositionNormalTextureTangentPs>();
            pixelShader?.SetDiffuseMap(material.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(sampler);
            pixelShader?.SetNormalMap(material.Material?.DiffuseTexture);
            pixelShader?.SetNormalSampler(sampler);
        }
    }
}
