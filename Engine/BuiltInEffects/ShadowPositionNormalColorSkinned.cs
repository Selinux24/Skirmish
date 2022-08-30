
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.DefaultShadow;
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-normal-color drawer
    /// </summary>
    public class ShadowPositionNormalColorSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowSkinnedPositionNormalColorVs>();
        }

        /// <inheritdoc/>
        public void Update(AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<ShadowSkinnedPositionNormalColorVs>();
            vertexShader?.WriteCBPerInstance(animation);
        }
    }
}
