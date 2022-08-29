using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-texture-tangent drawer
    /// </summary>
    public class BasicPositionNormalTextureTangent : BuiltInDrawer
    {
        private readonly EngineSamplerState linear;
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalTextureTangent(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionNormalTextureTangentVs>();
            SetPixelShader<BasicPositionNormalTextureTangentPs>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionNormalTextureTangentVs>();
            vertexShader?.WriteCBPerInstance(material, tintColor, textureIndex);

            var sampler = material.UseAnisotropic ? anisotropic : linear;
            var pixelShader = GetPixelShader<BasicPositionNormalTextureTangentPs>();
            pixelShader?.SetDiffuseMap(material.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(sampler);
            pixelShader?.SetNormalMap(material.Material?.NormalMap);
            pixelShader?.SetNormalSampler(sampler);
        }
    }
}
