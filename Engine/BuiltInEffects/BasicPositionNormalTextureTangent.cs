using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-texture-tangent drawer
    /// </summary>
    public class BasicPositionNormalTextureTangent : BuiltInDrawer<BasicPositionNormalTextureTangentVs, BasicPositionNormalTextureTangentPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalTextureTangent(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            VertexShader.WriteCBPerInstance(material, tintColor, textureIndex);

            var sampler = material.UseAnisotropic ?
                BuiltInShaders.GetSamplerAnisotropic() :
                BuiltInShaders.GetSamplerLinear();

            PixelShader.SetDiffuseMap(material.Material?.DiffuseTexture);
            PixelShader.SetDiffseSampler(sampler);
            PixelShader.SetNormalMap(material.Material?.NormalMap);
            PixelShader.SetNormalSampler(sampler);
        }
    }
}
