﻿
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow position-color instanced drawer
    /// </summary>
    public class ShadowPositionColorInstanced : BuiltInDrawer<ShadowPositionColorVsI, EmptyGs, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionColorInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}