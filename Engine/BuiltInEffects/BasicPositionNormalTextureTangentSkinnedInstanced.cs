using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-texture-tangent instanced drawer
    /// </summary>
    public class BasicPositionNormalTextureTangentSkinnedInstanced : BuiltInDrawer<BasicPositionNormalTextureTangentSkinnedVsI, EmptyGs, BasicPositionNormalTextureTangentPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalTextureTangentSkinnedInstanced(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            VertexShader.WriteCBPerObject(material, tintColor);

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
