﻿using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-texture drawer
    /// </summary>
    public class SkinnedPositionTexture : BuiltInDrawer<SkinnedPositionTextureVs, PositionTexturePs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SkinnedPositionTexture(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            VertexShader.WriteCBPerInstance(material, tintColor, textureIndex, animation);

            PixelShader.SetDiffuseMap(material.Material?.DiffuseTexture);
            PixelShader.SetDiffseSampler(BuiltInShaders.GetSamplerLinear());
        }
    }
}