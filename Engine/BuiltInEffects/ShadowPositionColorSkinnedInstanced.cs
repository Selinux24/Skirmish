﻿
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.DefaultShadow;

    /// <summary>
    /// Shadow Skinned position-color instanced drawer
    /// </summary>
    public class ShadowPositionColorSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowSkinnedPositionColorVsI>();
        }
    }
}
