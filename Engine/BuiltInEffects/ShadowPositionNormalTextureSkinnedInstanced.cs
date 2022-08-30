﻿
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.DefaultShadow;

    /// <summary>
    /// Shadow Skinned position-normal-texture instanced drawer
    /// </summary>
    public class ShadowPositionNormalTextureSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowSkinnedPositionNormalTextureVsI>();
        }
    }
}
