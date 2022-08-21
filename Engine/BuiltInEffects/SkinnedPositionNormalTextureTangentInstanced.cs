using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-texture-tangent instanced drawer
    /// </summary>
    public class SkinnedPositionNormalTextureTangentInstanced : BuiltInDrawer<SkinnedPositionNormalTextureTangentVsI, PositionNormalTextureTangentPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SkinnedPositionNormalTextureTangentInstanced(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            VertexShader.WriteCBPerObject(material, tintColor);

            PixelShader.SetDiffuseMap(material.Material?.DiffuseTexture);
            PixelShader.SetDiffseSampler(BuiltInShaders.GetSamplerLinear());
            PixelShader.SetNormalMap(material.Material?.NormalMap);
            PixelShader.SetNormalSampler(BuiltInShaders.GetSamplerLinear());
        }
    }
}
