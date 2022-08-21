using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-texture instanced drawer
    /// </summary>
    public class SkinnedPositionNormalTextureInstanced : BuiltInDrawer<SkinnedPositionNormalTextureVsI, PositionNormalTexturePs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SkinnedPositionNormalTextureInstanced(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            VertexShader.WriteCBPerObject(material, tintColor);

            PixelShader.SetDiffuseMap(material.Material?.DiffuseTexture);
            PixelShader.SetDiffseSampler(BuiltInShaders.GetSamplerLinear());
        }
    }
}
