using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Shadows;
    using Engine.Common;

    /// <summary>
    /// Shadow skinned position-normal-color instanced drawer
    /// </summary>
    public class ShadowSkinnedPositionNormalColorInstanced : BuiltInDrawer<ShadowSkinnedPositionNormalColorVsI, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowSkinnedPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {

        }
    }
}
