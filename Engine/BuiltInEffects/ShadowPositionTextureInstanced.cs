﻿
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow position-texture instanced drawer
    /// </summary>
    public class ShadowPositionTextureInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionTextureInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionTextureVsI>();
        }
    }
}
