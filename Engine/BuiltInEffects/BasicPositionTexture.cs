using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Basic position-texture drawer
    /// </summary>
    public class BasicPositionTexture : BuiltInDrawer<BasicPositionTextureVs, BasicPositionTexturePs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionTexture(Graphics graphics) : base(graphics)
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
        }
    }
}
