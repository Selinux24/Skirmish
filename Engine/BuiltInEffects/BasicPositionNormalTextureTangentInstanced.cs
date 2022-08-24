using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-texture-tangent instanced drawer
    /// </summary>
    public class BasicPositionNormalTextureTangentInstanced : BuiltInDrawer<BasicPositionNormalTextureTangentVsI, EmptyGs, BasicPositionNormalTextureTangentPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalTextureTangentInstanced(Graphics graphics) : base(graphics)
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
