﻿
namespace Engine.BuiltIn.DefaultShadow
{
    /// <summary>
    /// Shadow Skinned position-texture instanced drawer
    /// </summary>
    public class BuiltInPositionTextureSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionTextureSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionTextureSkinnedVsI>();
        }
    }
}
