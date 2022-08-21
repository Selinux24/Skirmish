﻿using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-color instanced drawer
    /// </summary>
    public class SkinnedPositionNormalColorInstanced : BuiltInDrawer<SkinnedPositionNormalColorVsI, PositionNormalColorPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SkinnedPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            VertexShader.WriteCBPerObject(material, tintColor);
        }
    }
}