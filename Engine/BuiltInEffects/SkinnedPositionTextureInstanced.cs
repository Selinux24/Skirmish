using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-texture instanced drawer
    /// </summary>
    public class SkinnedPositionTextureInstanced : BuiltInDrawer<SkinnedPositionTextureVsI, PositionTexturePs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SkinnedPositionTextureInstanced(Graphics graphics) : base(graphics)
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
