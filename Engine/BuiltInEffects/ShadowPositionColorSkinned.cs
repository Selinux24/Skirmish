﻿
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.DefaultShadow;
    using Engine.Common;

    /// <summary>
    /// Shadow skinned position-color drawer
    /// </summary>
    public class ShadowPositionColorSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionColorVsSkinned">Skinned position color vertex shader</param>
        /// <param name="positionColorPs">Position color pixel shader</param>
        public ShadowPositionColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowSkinnedPositionColorVs>();
        }

        /// <inheritdoc/>
        public void Update(AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<ShadowSkinnedPositionColorVs>();
            vertexShader?.WriteCBPerInstance(animation);
        }
    }
}
